using EduScoring.Application.Interfaces;
using EduScoring.Infrastructure.Repositories;
using EduScoring.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EduScoring.Infrastructure.DependencyInjection;

/// <summary>
/// Extension methods for registering Infrastructure layer services in the DI container.
/// </summary>
public static class InfrastructureServiceExtensions
{
    /// <summary>
    /// Registers all Infrastructure services including repositories, LLM service, and options.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind LLM options from configuration
        services.Configure<LlmOptions>(configuration.GetSection(LlmOptions.SectionName));

        // Register in-memory repositories as singletons so state persists across requests
        services.AddSingleton<ISubmissionRepository, InMemorySubmissionRepository>();
        services.AddSingleton<IGradingCriteriaRepository, InMemoryGradingCriteriaRepository>();
        services.AddSingleton<IEvaluationResultRepository, InMemoryEvaluationResultRepository>();

        // Register the LLM evaluation service with a named HttpClient
        services.AddHttpClient<ILLMEvaluationService, LLMEvaluationService>(client =>
        {
            var baseUrl = configuration[$"{LlmOptions.SectionName}:BaseUrl"]
                          ?? "https://api.openai.com/v1";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(120);
        });

        return services;
    }
}
