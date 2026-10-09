using System.Text.Json.Serialization;

namespace Ollaya.Client.Models;

/// <summary>
/// Base answer class for polymorphic serialization
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ChoiceAnswer), "choice")]
[JsonDerivedType(typeof(ScoreAnswer), "score")]
[JsonDerivedType(typeof(NoulAnswer), "noul")]
public abstract record AnswerBase
{
    /// <summary>
    /// Answer type (ignored during serialization - polymorphic discriminator handles it)
    /// </summary>
    [JsonIgnore]
    public abstract QuestionType Type { get; }
}

/// <summary>
/// Choice answer
/// </summary>
public sealed record ChoiceAnswer : AnswerBase
{
    [JsonIgnore]
    public override QuestionType Type => QuestionType.Choice;

    /// <summary>
    /// The most likely label
    /// </summary>
    [JsonPropertyName("choice")]
    public required string Choice { get; init; }

    /// <summary>
    /// Normalized top probability confidence (0-1)
    /// </summary>
    [JsonPropertyName("confidence")]
    public required double Confidence { get; init; }

    /// <summary>
    /// Label -> probability mapping in criteria order
    /// </summary>
    [JsonPropertyName("probabilities")]
    public required Dictionary<string, double> Probabilities { get; init; }

    /// <summary>
    /// Laya-specific extras (when requested)
    /// </summary>
    [JsonPropertyName("laya")]
    public LayaExtras? Laya { get; init; }
}

/// <summary>
/// Score answer
/// </summary>
public sealed record ScoreAnswer : AnswerBase
{
    [JsonIgnore]
    public override QuestionType Type => QuestionType.Score;

    /// <summary>
    /// Expected level Σ i·pᵢ (can fall between levels)
    /// </summary>
    [JsonPropertyName("score")]
    public required double Score { get; init; }

    /// <summary>
    /// Normalized confidence (0-1)
    /// </summary>
    [JsonPropertyName("confidence")]
    public required double Confidence { get; init; }

    /// <summary>
    /// Level -> description mapping
    /// </summary>
    [JsonPropertyName("legend")]
    public required Dictionary<string, string> Legend { get; init; }

    /// <summary>
    /// Level -> probability mapping
    /// </summary>
    [JsonPropertyName("probabilities")]
    public required Dictionary<string, double> Probabilities { get; init; }

    /// <summary>
    /// Laya-specific extras (when requested)
    /// </summary>
    [JsonPropertyName("laya")]
    public LayaExtras? Laya { get; init; }
}

/// <summary>
/// NOUL answer
/// </summary>
public sealed record NoulAnswer : AnswerBase
{
    [JsonIgnore]
    public override QuestionType Type => QuestionType.Noul;

    /// <summary>
    /// Probability that the statement holds
    /// </summary>
    [JsonPropertyName("noul")]
    public required double Noul { get; init; }

    /// <summary>
    /// Laya-specific extras (when requested)
    /// </summary>
    [JsonPropertyName("laya")]
    public LayaExtras? Laya { get; init; }
}

/// <summary>
/// Laya-specific extras added when "extras": ["laya"] is requested
/// </summary>
public sealed record LayaExtras
{
    /// <summary>
    /// Laya's entropy-based confidence
    /// </summary>
    [JsonPropertyName("confidence")]
    public required double Confidence { get; init; }

    /// <summary>
    /// Act probability from model's act head (or null)
    /// </summary>
    [JsonPropertyName("act_probability")]
    public double? ActProbability { get; init; }
}