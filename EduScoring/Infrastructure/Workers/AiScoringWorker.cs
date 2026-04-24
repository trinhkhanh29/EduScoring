using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
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

    public AiScoringWorker(IConnection rabbitConnection, ILogger<AiScoringWorker> logger, IServiceScopeFactory scopeFactory)
    {
        _rabbitConnection = rabbitConnection;
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var channel = await _rabbitConnection.CreateChannelAsync(cancellationToken: stoppingToken);
        await channel.QueueDeclareAsync(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            try
            {
                using var jsonDoc = JsonDocument.Parse(message);
                var root = jsonDoc.RootElement;

                if (!root.TryGetProperty("SubmissionId", out var idElement) || !idElement.TryGetGuid(out var submissionId))
                {
                    _logger.LogWarning("[AiScoringWorker] Message không hợp lệ hoặc thiếu SubmissionId: {Message}", message);
                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var aiScoring = scope.ServiceProvider.GetRequiredService<IAiScoringService>();

                var submission = await db.Submissions
                    .Include(s => s.Exam)
                    .ThenInclude(e => e.Rubrics)
                    .FirstOrDefaultAsync(s => s.Id == submissionId);

                if (submission == null)
                {
                    _logger.LogWarning("[AiScoringWorker] Không tìm thấy Submission: {Id}", submissionId);
                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                    return;
                }

                _logger.LogInformation("[AiScoringWorker] Bắt đầu chấm AI cho Submission: {Id}", submissionId);
                var ocrText = submission.CombinedOcrText ?? string.Empty;

                if (string.IsNullOrWhiteSpace(ocrText))
                {
                    _logger.LogWarning("[AiScoringWorker] SubmissionId={Id} không có OCR text. Bỏ qua chấm AI.", submissionId);
                    submission.Status = "PendingOcr";
                    await db.SaveChangesAsync();
                    await channel.BasicAckAsync(ea.DeliveryTag, false);
                    return;
                }

                var rubrics = submission.Exam?.Rubrics?.ToList() ?? new List<Rubric>();

                var rubricJson = JsonSerializer.Serialize(rubrics.Select(r => new
                {
                    r.CriteriaName,
                    r.MaxScore,
                    r.Description
                }));

                var language = submission.Language ?? "vi";

                var aiResult = await aiScoring.EvaluateAsync(ocrText, rubricJson, language);

                var aiEval = new AiEvaluation
                {
                    SubmissionId = submission.Id,
                    TotalScore = (double)aiResult.TotalScore,
                    OverallFeedback = aiResult.OverallFeedback,
                    ConfidenceScore = (double)aiResult.ConfidenceScore,
                    EvaluatedAt = DateTimeOffset.UtcNow,
                    Details = aiResult.CriteriaScores.Select(c => new AiEvaluationDetail
                    {
                        Id = Guid.NewGuid(),
                        CriteriaName = c.CriteriaName,
                        Score = (double)c.Score,
                        MaxScore = (double)c.MaxScore,
                        Reasoning = c.Reasoning
                    }).ToList()
                };

                db.AiEvaluations.Add(aiEval);
                submission.LatestAiScore = aiResult.TotalScore;
                submission.Status = "Evaluated";

                await db.SaveChangesAsync();

                _logger.LogInformation("[AiScoringWorker] THÀNH CÔNG — Đã chấm xong điểm: {Score}", aiResult.TotalScore);
                await channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AiScoringWorker] Lỗi khi xử lý message: {Message}", message);
                await channel.BasicNackAsync(ea.DeliveryTag, false, false);
            }
        };

        await channel.BasicQosAsync(0, 1, false, cancellationToken: stoppingToken);
        await channel.BasicConsumeAsync(queue: QueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
    }
}