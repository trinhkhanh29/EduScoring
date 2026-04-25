using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Polly;
using Polly.Retry;
using EduScoring.Features.Submissions.Services;
using EduScoring.Infrastructure;
using EduScoring.Features.Submissions.Models;
using EduScoring.Features.Exams.Models;
using Microsoft.EntityFrameworkCore;

namespace EduScoring.Infrastructure.Workers;

public class AiScoringWorker : BackgroundService
{
    private readonly IConnection _rabbitConnection;
    private readonly ILogger<AiScoringWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    private const string QueueName = "submission_reevaluation_triggered_queue";
    private const string DlqQueueName = "submission_reevaluation_triggered_queue.dlq";
    private const string DlxExchange = "submission_reevaluation.dlx";

    // Polly: retry 3 lần, chờ 2s / 4s / 8s (exponential backoff)
    private static readonly AsyncRetryPolicy _retryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
            onRetry: (ex, delay, attempt, _) =>
            {
                Console.WriteLine($"[AiScoringWorker][Polly] Retry lần {attempt} sau {delay.TotalSeconds}s — Lỗi: {ex.Message}");
            });

    public AiScoringWorker(IConnection rabbitConnection, ILogger<AiScoringWorker> logger, IServiceScopeFactory scopeFactory)
    {
        _rabbitConnection = rabbitConnection;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = await _rabbitConnection.CreateChannelAsync(cancellationToken: stoppingToken);

        // ── Khai báo Dead Letter Exchange và DLQ ──────────────────────────────
        await channel.ExchangeDeclareAsync(
            exchange: DlxExchange,
            type: ExchangeType.Direct,
            durable: true,
            cancellationToken: stoppingToken);

        await channel.QueueDeclareAsync(
            queue: DlqQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        await channel.QueueBindAsync(
            queue: DlqQueueName,
            exchange: DlxExchange,
            routingKey: QueueName,
            cancellationToken: stoppingToken);

        // ── Khai báo Queue chính — gắn DLX để khi Nack không requeue sẽ sang DLQ ──
        var queueArgs = new Dictionary<string, object?>
        {
            { "x-dead-letter-exchange", DlxExchange },
            { "x-dead-letter-routing-key", QueueName }
        };

        await channel.QueueDeclareAsync(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: queueArgs,
            cancellationToken: stoppingToken);

        await channel.BasicQosAsync(0, 1, false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (_, ea) =>
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());

            try
            {
                await ProcessMessageAsync(message, stoppingToken);
                await channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                // Polly đã retry 3 lần bên trong ProcessMessageAsync mà vẫn lỗi
                // → Nack không requeue → RabbitMQ tự chuyển sang DLQ
                _logger.LogError(ex, "[AiScoringWorker] Thất bại sau 3 lần retry — Đẩy sang DLQ. Message: {Message}", message);
                await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: false);
            }
        };

        await channel.BasicConsumeAsync(
            queue: QueueName,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        // Giữ worker sống đến khi app shutdown
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ProcessMessageAsync(string message, CancellationToken ct)
    {
        // Parse SubmissionId từ message
        using var jsonDoc = JsonDocument.Parse(message);
        var root = jsonDoc.RootElement;

        if (!root.TryGetProperty("SubmissionId", out var idElement) || !idElement.TryGetGuid(out var submissionId))
        {
            _logger.LogWarning("[AiScoringWorker] Message không hợp lệ hoặc thiếu SubmissionId: {Message}", message);
            return; // Message rác → không retry, không vào DLQ
        }

        // Bọc toàn bộ logic AI trong Polly retry
        await _retryPolicy.ExecuteAsync(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var aiScoring = scope.ServiceProvider.GetRequiredService<IAiScoringService>();

            var submission = await db.Submissions
                .Include(s => s.Exam)
                    .ThenInclude(e => e.Rubrics)
                .FirstOrDefaultAsync(s => s.Id == submissionId, ct);

            if (submission == null)
            {
                _logger.LogWarning("[AiScoringWorker] Không tìm thấy Submission: {Id}", submissionId);
                return; // Không retry nếu không tìm thấy
            }

            _logger.LogInformation("[AiScoringWorker] Bắt đầu chấm AI cho Submission: {Id}", submissionId);

            var ocrText = submission.CombinedOcrText ?? string.Empty;

            // ── CASE 1: OCR rỗng → gắn điểm 0, lưu DB, không gọi Gemini ────────
            if (string.IsNullOrWhiteSpace(ocrText))
            {
                _logger.LogWarning("[AiScoringWorker] SubmissionId={Id} không có OCR text — Gắn điểm 0.", submissionId);

                var rubrics = submission.Exam?.Rubrics?.ToList() ?? new List<Rubric>();

                var emptyEval = new AiEvaluation
                {
                    SubmissionId = submission.Id,
                    TotalScore = 0,
                    OverallFeedback = "Không có nội dung bài làm để chấm điểm.",
                    ConfidenceScore = 0,
                    ModelName = "N/A",
                    PromptVersion = "N/A",
                    Status = "NoContent",
                    EvaluatedAt = DateTimeOffset.UtcNow,
                    Details = rubrics.Select(r => new AiEvaluationDetail
                    {
                        Id = Guid.NewGuid(),
                        CriteriaName = r.CriteriaName,
                        Score = 0,
                        MaxScore = (double)r.MaxScore,
                        Reasoning = "Không có nội dung bài làm.",
                        CriteriaKey = r.CriteriaName,
                        CriteriaGroup = string.Empty
                    }).ToList()
                };

                db.AiEvaluations.Add(emptyEval);
                submission.LatestAiScore = 0;
                submission.Status = "NoContent";
                await db.SaveChangesAsync(ct);

                _logger.LogInformation("[AiScoringWorker] Đã lưu điểm 0 cho SubmissionId={Id} — lý do: Không có OCR text.", submissionId);
                return;
            }

            // ── CASE 2: Có OCR text → gọi Gemini ────────────────────────────────
            var rubricList = submission.Exam?.Rubrics?.ToList() ?? new List<Rubric>();
            var rubricJson = JsonSerializer.Serialize(rubricList.Select(r => new
            {
                r.CriteriaName,
                r.MaxScore,
                r.Description
            }));

            var language = submission.Language ?? "vi";
            var aiResult = await aiScoring.EvaluateAsync(ocrText, rubricJson, language);

            // ── Validate: Score không được vượt MaxScore ─────────────────────────
            // (GeminiScoringService đã clamp, nhưng double-check ở đây cho chắc)
            var totalMaxScore = rubricList.Sum(r => (double)r.MaxScore);
            if (aiResult.TotalScore > (decimal)totalMaxScore)
            {
                _logger.LogWarning(
                    "[AiScoringWorker] TotalScore={Score} vượt MaxScore={Max} — Clamp về MaxScore.",
                    aiResult.TotalScore, totalMaxScore);
                aiResult = aiResult with { TotalScore = (decimal)totalMaxScore };
            }

            var aiEval = new AiEvaluation
            {
                SubmissionId = submission.Id,
                TotalScore = (double)aiResult.TotalScore,
                OverallFeedback = aiResult.OverallFeedback,
                ConfidenceScore = (double)aiResult.ConfidenceScore,
                ModelName = submission.Language ?? "gemini",
                Status = "Completed",
                EvaluatedAt = DateTimeOffset.UtcNow,
                Details = aiResult.CriteriaScores.Select(c => new AiEvaluationDetail
                {
                    Id = Guid.NewGuid(),
                    CriteriaName = c.CriteriaName,
                    Score = (double)c.Score,
                    MaxScore = (double)c.MaxScore,
                    Reasoning = c.Reasoning,
                    CriteriaKey = c.CriteriaName,
                    CriteriaGroup = string.Empty
                }).ToList()
            };

            db.AiEvaluations.Add(aiEval);
            submission.LatestAiScore = aiResult.TotalScore;
            submission.Status = "Evaluated";
            await db.SaveChangesAsync(ct);

            _logger.LogInformation("[AiScoringWorker] THÀNH CÔNG — SubmissionId={Id} | Điểm: {Score}/{Max}",
                submissionId, aiResult.TotalScore, totalMaxScore);
        });
    }
}