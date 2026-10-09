using System.Text.Json.Serialization;

namespace Ollaya.Client.Models;

/// <summary>
/// Question types supported by Ollaya
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum QuestionType
{
    /// <summary>
    /// Choice question - select one from multiple options
    /// </summary>
    Choice,

    /// <summary>
    /// Score question - rate on a scale
    /// </summary>
    Score,

    /// <summary>
    /// NOUL (Not One Universal Label) - true/false probability
    /// </summary>
    Noul
}

/// <summary>
/// Base question class for polymorphic serialization
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ChoiceQuestion), "choice")]
[JsonDerivedType(typeof(ScoreQuestion), "score")]
[JsonDerivedType(typeof(NoulQuestion), "noul")]
public abstract record QuestionBase
{
    /// <summary>
    /// Question type (ignored during serialization - polymorphic discriminator handles it)
    /// </summary>
    [JsonIgnore]
    public abstract QuestionType Type { get; }
    public virtual object? Instructions { get; init; }
}

/// <summary>
/// Choice question - select one from multiple options (2-255 options)
/// </summary>
public sealed record ChoiceQuestion : QuestionBase
{
    [JsonIgnore]
    public override QuestionType Type => QuestionType.Choice;

    /// <summary>
    /// Optional instructions for the model
    /// </summary>
    public override object? Instructions { get; init; }

    /// <summary>
    /// Criteria: label -> description, or array of labels (2-255 options)
    /// </summary>
    [JsonPropertyName("criteria")]
    public required object Criteria { get; init; }
}

/// <summary>
/// Score question - rate on a scale (2-10 levels)
/// </summary>
public sealed record ScoreQuestion : QuestionBase
{
    [JsonIgnore]
    public override QuestionType Type => QuestionType.Score;

    /// <summary>
    /// Optional instructions for the model
    /// </summary>
    public override object? Instructions { get; init; }

    /// <summary>
    /// Array of level descriptions, level 0 first (2-10 levels)
    /// </summary>
    [JsonPropertyName("criteria")]
    public required string[] Criteria { get; init; }
}

/// <summary>
/// NOUL question - true/false probability
/// </summary>
public sealed record NoulQuestion : QuestionBase
{
    [JsonIgnore]
    public override QuestionType Type => QuestionType.Noul;

    /// <summary>
    /// Optional instructions for the model
    /// </summary>
    public override object? Instructions { get; init; }

    /// <summary>
    /// Optional criteria with custom true/false labels
    /// </summary>
    [JsonPropertyName("criteria")]
    public NoulCriteria? Criteria { get; init; }
}

/// <summary>
/// NOUL criteria with custom labels
/// </summary>
public sealed record NoulCriteria
{
    [JsonPropertyName("true")]
    public string TrueLabel { get; init; } = "True";

    [JsonPropertyName("false")]
    public string FalseLabel { get; init; } = "False";
}