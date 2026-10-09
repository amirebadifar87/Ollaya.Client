namespace Ollaya.Client.Exceptions;

/// <summary>
/// Ollaya API error codes
/// </summary>
public enum OllayaErrorCode
{
    InvalidJson,
    InvalidRequest,
    TooManyOptions,
    InputTooLong,
    StateTruncated,
    Unauthorized,
    Forbidden,
    ModelNotFound,
    NotFound,
    MethodNotAllowed,
    OperationInProgress,
    RequestTooLarge,
    QueueFull,
    ModelLoadFailed,
    InferenceFailed,
    StorageError,
    Internal,
    UnsupportedModel,
    NotImplemented,
    RegistryError,
    DigestMismatch,
    Unknown
}

/// <summary>
/// Base exception for Ollaya API errors
/// </summary>
public class OllayaException : Exception
{
    public OllayaErrorCode Code { get; }
    public int HttpStatusCode { get; }
    public string? Detail { get; }
    public string? RequestId { get; }

    public OllayaException(
        string message,
        OllayaErrorCode code,
        int httpStatusCode,
        string? detail = null,
        string? requestId = null)
        : base(message)
    {
        Code = code;
        HttpStatusCode = httpStatusCode;
        Detail = detail;
        RequestId = requestId;
    }

    public static OllayaErrorCode ParseCode(string code) => code switch
    {
        "INVALID_JSON" => OllayaErrorCode.InvalidJson,
        "INVALID_REQUEST" => OllayaErrorCode.InvalidRequest,
        "TOO_MANY_OPTIONS" => OllayaErrorCode.TooManyOptions,
        "INPUT_TOO_LONG" => OllayaErrorCode.InputTooLong,
        "STATE_TRUNCATED" => OllayaErrorCode.StateTruncated,
        "UNAUTHORIZED" => OllayaErrorCode.Unauthorized,
        "FORBIDDEN" => OllayaErrorCode.Forbidden,
        "MODEL_NOT_FOUND" => OllayaErrorCode.ModelNotFound,
        "NOT_FOUND" => OllayaErrorCode.NotFound,
        "METHOD_NOT_ALLOWED" => OllayaErrorCode.MethodNotAllowed,
        "OPERATION_IN_PROGRESS" => OllayaErrorCode.OperationInProgress,
        "REQUEST_TOO_LARGE" => OllayaErrorCode.RequestTooLarge,
        "QUEUE_FULL" => OllayaErrorCode.QueueFull,
        "MODEL_LOAD_FAILED" => OllayaErrorCode.ModelLoadFailed,
        "INFERENCE_FAILED" => OllayaErrorCode.InferenceFailed,
        "STORAGE_ERROR" => OllayaErrorCode.StorageError,
        "INTERNAL" => OllayaErrorCode.Internal,
        "UNSUPPORTED_MODEL" => OllayaErrorCode.UnsupportedModel,
        "NOT_IMPLEMENTED" => OllayaErrorCode.NotImplemented,
        "REGISTRY_ERROR" => OllayaErrorCode.RegistryError,
        "DIGEST_MISMATCH" => OllayaErrorCode.DigestMismatch,
        _ => OllayaErrorCode.Unknown
    };

    public static bool IsRetryable(OllayaErrorCode code) => code switch
    {
        OllayaErrorCode.QueueFull => true,
        OllayaErrorCode.ModelLoadFailed => true,
        OllayaErrorCode.InferenceFailed => true,
        OllayaErrorCode.Internal => true,
        OllayaErrorCode.RegistryError => true,
        OllayaErrorCode.DigestMismatch => true,
        OllayaErrorCode.OperationInProgress => true,
        _ => false
    };
}

/// <summary>
/// Exception for validation errors (INVALID_REQUEST)
/// </summary>
public class OllayaValidationException : OllayaException
{
    public ValidationError[] Errors { get; }

    public OllayaValidationException(
        string message,
        ValidationError[] errors,
        int httpStatusCode,
        string? requestId = null)
        : base(message, OllayaErrorCode.InvalidRequest, httpStatusCode, null, requestId)
    {
        Errors = errors;
    }
}

/// <summary>
/// Validation error detail
/// </summary>
public sealed record ValidationError
{
    public required string[] Loc { get; init; }
    public required string Msg { get; init; }
    public required string Type { get; init; }
    public object? Ctx { get; init; }
}