using EduScoring.Application.DTOs;
using EduScoring.Application.Interfaces;
using EduScoring.Domain.Entities;

namespace EduScoring.Application.UseCases;

/// <summary>
/// Handles the end-to-end workflow for evaluating an essay submission using an LLM.
/// </summary>
public class EvaluateEssayUseCase
{
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IGradingCriteriaRepository _criteriaRepository;
    private readonly IEvaluationResultRepository _evaluationRepository;
    private readonly ILLMEvaluationService _llmService;

    public EvaluateEssayUseCase(
        ISubmissionRepository submissionRepository,
        IGradingCriteriaRepository criteriaRepository,
        IEvaluationResultRepository evaluationRepository,
        ILLMEvaluationService llmService)
    {
        _submissionRepository = submissionRepository;
        _criteriaRepository = criteriaRepository;
        _evaluationRepository = evaluationRepository;
        _llmService = llmService;
    }

    /// <summary>
    /// Submits a new essay and stores it, returning the submission details.
    /// </summary>
    public async Task<SubmissionResponse> SubmitEssayAsync(
        SubmitEssayRequest request,
        CancellationToken cancellationToken = default)
    {
        var criteria = await _criteriaRepository.GetByIdAsync(request.GradingCriteriaId, cancellationToken)
            ?? throw new InvalidOperationException($"Grading criteria '{request.GradingCriteriaId}' not found.");

        var submission = StudentEssaySubmission.Create(
            request.StudentId,
            request.StudentName,
            request.Title,
            request.EssayContent,
            criteria.Id);

        await _submissionRepository.AddAsync(submission, cancellationToken);

        return SubmissionResponse.FromDomain(submission);
    }

    /// <summary>
    /// Triggers LLM evaluation for an existing submission and returns the result.
    /// </summary>
    public async Task<EvaluationResultResponse> EvaluateSubmissionAsync(
        Guid submissionId,
        CancellationToken cancellationToken = default)
    {
        var submission = await _submissionRepository.GetByIdAsync(submissionId, cancellationToken)
            ?? throw new InvalidOperationException($"Submission '{submissionId}' not found.");

        var criteria = await _criteriaRepository.GetByIdAsync(submission.GradingCriteriaId, cancellationToken)
            ?? throw new InvalidOperationException($"Grading criteria '{submission.GradingCriteriaId}' not found.");

        submission.MarkAsProcessing();
        await _submissionRepository.UpdateAsync(submission, cancellationToken);

        try
        {
            var result = await _llmService.EvaluateAsync(submission, criteria, cancellationToken);
            await _evaluationRepository.AddAsync(result, cancellationToken);

            submission.MarkAsCompleted();
            await _submissionRepository.UpdateAsync(submission, cancellationToken);

            return EvaluationResultResponse.FromDomain(result);
        }
        catch
        {
            submission.MarkAsFailed();
            await _submissionRepository.UpdateAsync(submission, cancellationToken);
            throw;
        }
    }

    /// <summary>
    /// Retrieves the evaluation result for a given submission.
    /// </summary>
    public async Task<EvaluationResultResponse?> GetEvaluationResultAsync(
        Guid submissionId,
        CancellationToken cancellationToken = default)
    {
        var result = await _evaluationRepository.GetBySubmissionIdAsync(submissionId, cancellationToken);
        return result is null ? null : EvaluationResultResponse.FromDomain(result);
    }
}
