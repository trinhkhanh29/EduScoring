using EduScoring.Application.DTOs;
using EduScoring.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace EduScoring.API.Controllers;

/// <summary>
/// Manages essay submissions and triggers LLM-based automated grading.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EssaySubmissionsController : ControllerBase
{
    private readonly EvaluateEssayUseCase _evaluateUseCase;
    private readonly ILogger<EssaySubmissionsController> _logger;

    public EssaySubmissionsController(
        EvaluateEssayUseCase evaluateUseCase,
        ILogger<EssaySubmissionsController> logger)
    {
        _evaluateUseCase = evaluateUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Submits a new student essay for evaluation.
    /// </summary>
    /// <param name="request">The essay submission payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created submission record.</returns>
    /// <response code="201">Submission created successfully.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="404">Grading criteria not found.</response>
    [HttpPost]
    [ProducesResponseType(typeof(SubmissionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SubmitEssay(
        [FromBody] SubmitEssayRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var submission = await _evaluateUseCase.SubmitEssayAsync(request, cancellationToken);
            return CreatedAtAction(
                nameof(GetEvaluationResult),
                new { submissionId = submission.SubmissionId },
                submission);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid submission request.");
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Triggers LLM evaluation for an existing submission.
    /// </summary>
    /// <param name="submissionId">The unique identifier of the submission.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The detailed evaluation result with scores and feedback.</returns>
    /// <response code="200">Evaluation completed successfully.</response>
    /// <response code="404">Submission not found.</response>
    /// <response code="502">LLM service call failed.</response>
    [HttpPost("{submissionId:guid}/evaluate")]
    [ProducesResponseType(typeof(EvaluationResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> EvaluateSubmission(
        [FromRoute] Guid submissionId,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _evaluateUseCase.EvaluateSubmissionAsync(submissionId, cancellationToken);
            return Ok(result);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { error = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "LLM service call failed for submission {SubmissionId}.", submissionId);
            return StatusCode(StatusCodes.Status502BadGateway, new { error = "LLM service is unavailable. Please try again later." });
        }
    }

    /// <summary>
    /// Retrieves the evaluation result for a submitted essay.
    /// </summary>
    /// <param name="submissionId">The unique identifier of the submission.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The evaluation result, or 404 if not yet evaluated.</returns>
    /// <response code="200">Evaluation result found.</response>
    /// <response code="404">Result not found; the essay may not have been evaluated yet.</response>
    [HttpGet("{submissionId:guid}/result")]
    [ProducesResponseType(typeof(EvaluationResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEvaluationResult(
        [FromRoute] Guid submissionId,
        CancellationToken cancellationToken)
    {
        var result = await _evaluateUseCase.GetEvaluationResultAsync(submissionId, cancellationToken);
        return result is null
            ? NotFound(new { error = $"No evaluation result found for submission '{submissionId}'." })
            : Ok(result);
    }
}
