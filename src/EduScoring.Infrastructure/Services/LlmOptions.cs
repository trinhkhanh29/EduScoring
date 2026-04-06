namespace EduScoring.Infrastructure.Services;

/// <summary>
/// Configuration options for connecting to an LLM provider (OpenAI-compatible API).
/// </summary>
public class LlmOptions
{
    public const string SectionName = "LLM";

    /// <summary>
    /// The base URL of the LLM provider API (e.g. https://api.openai.com/v1).
    /// For Anthropic Claude, use an OpenAI-compatible proxy endpoint.
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";

    /// <summary>
    /// The API key used to authenticate with the LLM provider.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// The model identifier to use for evaluations (e.g. gpt-4o, claude-3-5-sonnet-20241022).
    /// </summary>
    public string Model { get; set; } = "gpt-4o";

    /// <summary>
    /// Maximum tokens to generate in the LLM response.
    /// </summary>
    public int MaxTokens { get; set; } = 2000;

    /// <summary>
    /// Temperature setting for the LLM response (0 = deterministic, 1 = creative).
    /// </summary>
    public double Temperature { get; set; } = 0.3;
}
