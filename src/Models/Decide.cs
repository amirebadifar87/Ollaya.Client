using System.Text.Json.Serialization;

namespace Ollaya.Client.Models;

/// <summary>
/// Request for /api/decide endpoint
/// </summary>
public sealed record DecideRequest
{
    /// <summary>
    /// Model name (e.g., "laya:en", "laya", "winnow:e4b")
    /// </summary>
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    /// <summary>
    /// State to analyze (string, object, or array) - up to 65,536 tokens
    /// </summary>
    [JsonPropertyName("state")]
    public required object State { get; init; }

    /// <summary>
    /// Questions to answer (1-256 questions), keyed by any ID
    /// </summary>
    [JsonPropertyName("questions")]
    public Dictionary<string, QuestionBase>? Questions { get; init; }

    /// <summary>
    /// Preset name to use instead of questions
    /// </summary>
    [JsonPropertyName("preset")]
    public string? Preset { get; init; }

    /// <summary>
    /// Base64-encoded PNG images for vision models
    /// </summary>
    [JsonPropertyName("images")]
    public string[]? Images { get; init; }

    /// <summary>
    /// How long to keep model loaded after request
    /// </summary>
    [JsonPropertyName("keep_alive")]
    public object? KeepAlive { get; init; }

    /// <summary>
    /// Extra fields to include in response (e.g., ["laya"])
    /// </summary>
    [JsonPropertyName("extras")]
    public string[]? Extras { get; init; }

    /// <summary>
    /// Reserved for future streaming support
    /// </summary>
    [JsonPropertyName("stream")]
    public bool? Stream { get; init; }
}

/// <summary>
/// Response from /api/decide endpoint
/// </summary>
public sealed record DecideResponse
{
    /// <summary>
    /// The model that answered (for routers, this is the target model)
    /// </summary>
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    /// <summary>
    /// Question ID -> answer, in question order
    /// </summary>
    [JsonPropertyName("answers")]
    public required Dictionary<string, AnswerBase> Answers { get; init; }

    /// <summary>
    /// Token usage
    /// </summary>
    [JsonPropertyName("usage")]
    public required Usage Usage { get; init; }

    /// <summary>
    /// Routing info (for router models)
    /// </summary>
    [JsonPropertyName("routing")]
    public RoutingInfo? Routing { get; init; }

    /// <summary>
    /// Whether state was truncated to fit context
    /// </summary>
    [JsonPropertyName("state_truncated")]
    public bool StateTruncated { get; init; }

    /// <summary>
    /// Reason: "decide", "load", or "unload"
    /// </summary>
    [JsonPropertyName("done_reason")]
    public required string DoneReason { get; init; }

    /// <summary>
    /// Response timestamp
    /// </summary>
    [JsonPropertyName("created_at")]
    public required DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Total duration in nanoseconds (including queue time)
    /// </summary>
    [JsonPropertyName("total_duration")]
    public required long TotalDuration { get; init; }

    /// <summary>
    /// Load duration in nanoseconds
    /// </summary>
    [JsonPropertyName("load_duration")]
    public required long LoadDuration { get; init; }

    /// <summary>
    /// Evaluation duration in nanoseconds
    /// </summary>
    [JsonPropertyName("eval_duration")]
    public required long EvalDuration { get; init; }
}

/// <summary>
/// Token usage information
/// </summary>
public sealed record Usage
{
    [JsonPropertyName("input_tokens")]
    public required long InputTokens { get; init; }

    [JsonPropertyName("output_tokens")]
    public required long OutputTokens { get; init; }
}

/// <summary>
/// Routing information for router models
/// </summary>
public sealed record RoutingInfo
{
    [JsonPropertyName("router")]
    public required string Router { get; init; }

    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("route")]
    public required string Route { get; init; }

    [JsonPropertyName("reason")]
    public required string Reason { get; init; }
}

/// <summary>
/// Request for TypeSafe-compatible /v1/systemone endpoint
/// </summary>
public sealed record SystemOneRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("state")]
    public required object State { get; init; }

    [JsonPropertyName("questions")]
    public required Dictionary<string, QuestionBase> Questions { get; init; }
}

/// <summary>
/// Response from TypeSafe-compatible /v1/systemone endpoint
/// </summary>
public sealed record SystemOneResponse
{
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("answers")]
    public required Dictionary<string, AnswerBase> Answers { get; init; }

    [JsonPropertyName("usage")]
    public required Usage Usage { get; init; }
}