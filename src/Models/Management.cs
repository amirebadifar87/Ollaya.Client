using System.Text.Json.Serialization;

namespace Ollaya.Client.Models;

/// <summary>
/// Model details from /api/tags
/// </summary>
public sealed record ModelInfo
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("modified_at")]
    public required DateTimeOffset ModifiedAt { get; init; }

    [JsonPropertyName("size")]
    public required long Size { get; init; }

    [JsonPropertyName("digest")]
    public required string Digest { get; init; }

    [JsonPropertyName("details")]
    public required ModelDetails Details { get; init; }
}

/// <summary>
/// Model details
/// </summary>
public sealed record ModelDetails
{
    [JsonPropertyName("parent_model")]
    public string ParentModel { get; init; } = "";

    [JsonPropertyName("format")]
    public required string Format { get; init; }

    [JsonPropertyName("family")]
    public required string Family { get; init; }

    [JsonPropertyName("families")]
    public required string[] Families { get; init; }

    [JsonPropertyName("parameter_size")]
    public required string ParameterSize { get; init; }

    [JsonPropertyName("quantization_level")]
    public required string QuantizationLevel { get; init; }
}

/// <summary>
/// Response from /api/tags
/// </summary>
public sealed record TagsResponse
{
    [JsonPropertyName("models")]
    public required ModelInfo[] Models { get; init; }
}

/// <summary>
/// Request for /api/show
/// </summary>
public sealed record ShowRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; init; }
}

/// <summary>
/// Detailed model info from /api/show
/// </summary>
public sealed record ShowResponse
{
    [JsonPropertyName("license")]
    public string? License { get; init; }

    [JsonPropertyName("modelfile")]
    public string? Modelfile { get; init; }

    [JsonPropertyName("parameters")]
    public string? Parameters { get; init; }

    [JsonPropertyName("questions")]
    public Dictionary<string, QuestionBase>? Questions { get; init; }

    [JsonPropertyName("router")]
    public RouterInfo? Router { get; init; }

    [JsonPropertyName("details")]
    public required ModelDetails Details { get; init; }

    [JsonPropertyName("model_info")]
    public ModelInfoData? ModelInfo { get; init; }

    [JsonPropertyName("capabilities")]
    public string[]? Capabilities { get; init; }

    [JsonPropertyName("modified_at")]
    public required DateTimeOffset ModifiedAt { get; init; }
}

/// <summary>
/// Router information
/// </summary>
public sealed record RouterInfo
{
    [JsonPropertyName("strategy")]
    public required string Strategy { get; init; }

    [JsonPropertyName("default")]
    public required string Default { get; init; }

    [JsonPropertyName("routes")]
    public required Dictionary<string, string> Routes { get; init; }
}

/// <summary>
/// Model info data
/// </summary>
public sealed record ModelInfoData
{
    [JsonPropertyName("general.architecture")]
    public string? Architecture { get; init; }

    [JsonPropertyName("general.languages")]
    public string[]? Languages { get; init; }

    [JsonPropertyName("general.source")]
    public string? Source { get; init; }

    [JsonPropertyName("laya.context_length")]
    public int? ContextLength { get; init; }
}

/// <summary>
/// Running model from /api/ps
/// </summary>
public sealed record RunningModel
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("size")]
    public required long Size { get; init; }

    [JsonPropertyName("digest")]
    public required string Digest { get; init; }

    [JsonPropertyName("details")]
    public required RunningModelDetails Details { get; init; }

    [JsonPropertyName("expires_at")]
    public DateTimeOffset? ExpiresAt { get; init; }

    [JsonPropertyName("size_vram")]
    public long SizeVram { get; init; }

    [JsonPropertyName("context_length")]
    public int ContextLength { get; init; }

    [JsonPropertyName("device")]
    public required string Device { get; init; }
}

/// <summary>
/// Running model details
/// </summary>
public sealed record RunningModelDetails
{
    [JsonPropertyName("format")]
    public required string Format { get; init; }

    [JsonPropertyName("family")]
    public required string Family { get; init; }

    [JsonPropertyName("parameter_size")]
    public required string ParameterSize { get; init; }

    [JsonPropertyName("quantization_level")]
    public required string QuantizationLevel { get; init; }
}

/// <summary>
/// Response from /api/ps
/// </summary>
public sealed record PsResponse
{
    [JsonPropertyName("models")]
    public required RunningModel[] Models { get; init; }
}

/// <summary>
/// Request for /api/pull
/// </summary>
public sealed record PullRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("stream")]
    public bool Stream { get; init; } = true;
}

/// <summary>
/// Pull progress response (streamed)
/// </summary>
public sealed record PullProgress
{
    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("digest")]
    public string? Digest { get; init; }

    [JsonPropertyName("total")]
    public long? Total { get; init; }

    [JsonPropertyName("completed")]
    public long? Completed { get; init; }

    [JsonPropertyName("error")]
    public string? Error { get; init; }

    [JsonPropertyName("code")]
    public string? Code { get; init; }

    /// <summary>
    /// Whether this is the final success message
    /// </summary>
    [JsonIgnore]
    public bool IsSuccess => Status == "success";

    /// <summary>
    /// Whether this is an error
    /// </summary>
    [JsonIgnore]
    public bool IsError => !string.IsNullOrEmpty(Error);
}

/// <summary>
/// Request for /api/delete
/// </summary>
public sealed record DeleteRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; init; }
}

/// <summary>
/// Request for /api/copy
/// </summary>
public sealed record CopyRequest
{
    [JsonPropertyName("source")]
    public required string Source { get; init; }

    [JsonPropertyName("destination")]
    public required string Destination { get; init; }
}

/// <summary>
/// Request for /api/create
/// </summary>
public sealed record CreateRequest
{
    [JsonPropertyName("model")]
    public required string Model { get; init; }

    [JsonPropertyName("from")]
    public required string From { get; init; }

    [JsonPropertyName("questions")]
    public Dictionary<string, QuestionBase>? Questions { get; init; }

    [JsonPropertyName("calibration")]
    public Calibration? Calibration { get; init; }

    [JsonPropertyName("parameters")]
    public CreateParameters? Parameters { get; init; }

    [JsonPropertyName("license")]
    public object? License { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("stream")]
    public bool Stream { get; init; } = true;
}

/// <summary>
/// Calibration settings for create
/// </summary>
public sealed record Calibration
{
    [JsonPropertyName("temperature")]
    public double[]? Temperature { get; init; }

    [JsonPropertyName("temperature_by_options")]
    public Dictionary<string, double>? TemperatureByOptions { get; init; }
}

/// <summary>
/// Create parameters
/// </summary>
public sealed record CreateParameters
{
    [JsonPropertyName("precision")]
    public string? Precision { get; init; } // "fp16" or "fp32"
}

/// <summary>
/// Create progress response (streamed)
/// </summary>
public sealed record CreateProgress
{
    [JsonPropertyName("status")]
    public required string Status { get; init; }

    [JsonPropertyName("digest")]
    public string? Digest { get; init; }

    [JsonPropertyName("error")]
    public string? Error { get; init; }

    [JsonPropertyName("code")]
    public string? Code { get; init; }

    [JsonIgnore]
    public bool IsSuccess => Status == "success";

    [JsonIgnore]
    public bool IsError => !string.IsNullOrEmpty(Error);
}

/// <summary>
/// Version response from /api/version
/// </summary>
public sealed record VersionResponse
{
    [JsonPropertyName("version")]
    public required string Version { get; init; }
}

/// <summary>
/// Preset info from /api/presets
/// </summary>
public sealed record PresetInfo
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("builtin")]
    public bool Builtin { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("question_ids")]
    public string[]? QuestionIds { get; init; }

    [JsonPropertyName("modified_at")]
    public DateTimeOffset? ModifiedAt { get; init; }
}

/// <summary>
/// Response from /api/presets
/// </summary>
public sealed record PresetsResponse
{
    [JsonPropertyName("presets")]
    public required PresetInfo[] Presets { get; init; }
}

/// <summary>
/// Request for /api/presets/create
/// </summary>
public sealed record CreatePresetRequest
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("questions")]
    public required Dictionary<string, QuestionBase> Questions { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }
}

/// <summary>
/// Request for /api/presets/show
/// </summary>
public sealed record ShowPresetRequest
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }
}

/// <summary>
/// Request for /api/presets/delete
/// </summary>
public sealed record DeletePresetRequest
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }
}