using EduScoring.Application.DTOs;
using EduScoring.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace EduScoring.API.Controllers;

/// <summary>
/// Manages scoring rubrics (grading criteria) used to evaluate essays.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class GradingCriteriaController : ControllerBase
{
    private readonly ManageGradingCriteriaUseCase _manageUseCase;
    private readonly ILogger<GradingCriteriaController> _logger;

    public GradingCriteriaController(
        ManageGradingCriteriaUseCase manageUseCase,
        ILogger<GradingCriteriaController> logger)
    {
        _manageUseCase = manageUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new grading criteria (scoring rubric).
    /// </summary>
    /// <param name="request">The grading criteria payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created grading criteria.</returns>
    /// <response code="201">Criteria created successfully.</response>
    /// <response code="400">Invalid request data.</response>
    [HttpPost]
    [ProducesResponseType(typeof(GradingCriteriaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCriteria(
        [FromBody] CreateGradingCriteriaRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var criteria = await _manageUseCase.CreateCriteriaAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetCriteriaById), new { id = criteria.Id }, criteria);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid grading criteria request.");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Returns all available grading criteria.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of all grading criteria.</returns>
    /// <response code="200">List of criteria returned successfully.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<GradingCriteriaResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllCriteria(CancellationToken cancellationToken)
    {
        var criteria = await _manageUseCase.GetAllCriteriaAsync(cancellationToken);
        return Ok(criteria);
    }

    /// <summary>
    /// Returns the grading criteria with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the grading criteria.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The requested grading criteria.</returns>
    /// <response code="200">Criteria found and returned.</response>
    /// <response code="404">Criteria not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(GradingCriteriaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCriteriaById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var criteria = await _manageUseCase.GetCriteriaByIdAsync(id, cancellationToken);
        return criteria is null
            ? NotFound(new { error = $"Grading criteria '{id}' not found." })
            : Ok(criteria);
    }
}
