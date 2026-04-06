using EduScoring.Application.DTOs;
using EduScoring.Application.Interfaces;
using EduScoring.Domain.Entities;

namespace EduScoring.Application.UseCases;

/// <summary>
/// Handles creation and retrieval of grading criteria.
/// </summary>
public class ManageGradingCriteriaUseCase
{
    private readonly IGradingCriteriaRepository _criteriaRepository;

    public ManageGradingCriteriaUseCase(IGradingCriteriaRepository criteriaRepository)
    {
        _criteriaRepository = criteriaRepository;
    }

    public async Task<GradingCriteriaResponse> CreateCriteriaAsync(
        CreateGradingCriteriaRequest request,
        CancellationToken cancellationToken = default)
    {
        var dimensions = request.Dimensions.Select(d =>
            new ScoringDimension(d.Name, d.Description, d.MaxPoints));

        var criteria = GradingCriteria.Create(
            request.Name,
            request.Description,
            dimensions,
            request.PromptTemplate);

        await _criteriaRepository.AddAsync(criteria, cancellationToken);

        return MapToResponse(criteria);
    }

    public async Task<IEnumerable<GradingCriteriaResponse>> GetAllCriteriaAsync(
        CancellationToken cancellationToken = default)
    {
        var allCriteria = await _criteriaRepository.GetAllAsync(cancellationToken);
        return allCriteria.Select(MapToResponse);
    }

    public async Task<GradingCriteriaResponse?> GetCriteriaByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var criteria = await _criteriaRepository.GetByIdAsync(id, cancellationToken);
        return criteria is null ? null : MapToResponse(criteria);
    }

    private static GradingCriteriaResponse MapToResponse(GradingCriteria criteria)
    {
        var dimensions = criteria.Dimensions
            .Select(d => new ScoringDimensionRequest(d.Name, d.Description, d.MaxPoints))
            .ToList()
            .AsReadOnly();

        return new GradingCriteriaResponse(
            criteria.Id,
            criteria.Name,
            criteria.Description,
            criteria.MaxScore,
            dimensions);
    }
}
