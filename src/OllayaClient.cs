using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ollaya.Client.Exceptions;
using Ollaya.Client.Models;

namespace Ollaya.Client;

/// <summary>
/// Configuration options for OllayaClient
/// </summary>
public sealed class OllayaClientOptions
{
    /// <summary>
    /// Base URL of the Ollaya server (default: http://localhost:11435)
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:11435";

    /// <summary>
    /// API key for authentication (if server requires it)
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Request timeout (default: 5 minutes)
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Custom HttpClient handler
    /// </summary>
    public HttpMessageHandler? HttpMessageHandler { get; set; }
}

/// <summary>
/// Client for interacting with Ollaya API
/// </summary>
public sealed class OllayaClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly OllayaClientOptions _options;
    private bool _disposed;

    /// <summary>
    /// Creates a new OllayaClient with default options
    /// </summary>
    public OllayaClient() : this(new OllayaClientOptions()) { }

    /// <summary>
    /// Creates a new OllayaClient with custom options
    /// </summary>
    public OllayaClient(OllayaClientOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));

        var handler = options.HttpMessageHandler ?? new HttpClientHandler();
        _httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/"),
            Timeout = _options.Timeout
        };

        if (!string.IsNullOrEmpty(_options.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        }

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) }
        };
    }

    #region Decide (Native API)

    /// <summary>
    /// Answers typed questions about a state using the native API
    /// </summary>
    public async Task<DecideResponse> DecideAsync(
        DecideRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PostAsync<DecideRequest, DecideResponse>("/api/decide", request, cancellationToken);
    }

    /// <summary>
    /// Simple decide with string state and questions dictionary
    /// </summary>
    public async Task<DecideResponse> DecideAsync(
        string model,
        object state,
        Dictionary<string, QuestionBase> questions,
        string? preset = null,
        string[]? images = null,
        object? keepAlive = null,
        string[]? extras = null,
        CancellationToken cancellationToken = default)
    {
        var request = new DecideRequest
        {
            Model = model,
            State = state,
            Questions = questions,
            Preset = preset,
            Images = images,
            KeepAlive = keepAlive,
            Extras = extras
        };
        return await DecideAsync(request, cancellationToken);
    }

    /// <summary>
    /// Load a model (keep it in memory)
    /// </summary>
    public async Task<DecideResponse> LoadModelAsync(
        string model,
        object? keepAlive = null,
        CancellationToken cancellationToken = default)
    {
        var request = new DecideRequest
        {
            Model = model,
            State = "", // Empty state to trigger load
            KeepAlive = keepAlive
        };
        return await DecideAsync(request, cancellationToken);
    }

    /// <summary>
    /// Unload a model from memory
    /// </summary>
    public async Task<DecideResponse> UnloadModelAsync(
        string model,
        CancellationToken cancellationToken = default)
    {
        var request = new DecideRequest
        {
            Model = model,
            State = "",
            KeepAlive = 0
        };
        return await DecideAsync(request, cancellationToken);
    }

    #endregion

    #region TypeSafe-Compatible API

    /// <summary>
    /// Answers typed questions using the TypeSafe-compatible API
    /// </summary>
    public async Task<SystemOneResponse> SystemOneAsync(
        SystemOneRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PostAsync<SystemOneRequest, SystemOneResponse>("/v1/systemone", request, cancellationToken);
    }

    /// <summary>
    /// Simple SystemOne call
    /// </summary>
    public async Task<SystemOneResponse> SystemOneAsync(
        string model,
        object state,
        Dictionary<string, QuestionBase> questions,
        CancellationToken cancellationToken = default)
    {
        var request = new SystemOneRequest
        {
            Model = model,
            State = state,
            Questions = questions
        };
        return await SystemOneAsync(request, cancellationToken);
    }

    /// <summary>
    /// Alias for SystemOneAsync
    /// </summary>
    public Task<SystemOneResponse> DecisionsAsync(
        SystemOneRequest request,
        CancellationToken cancellationToken = default)
        => SystemOneAsync(request, cancellationToken);

    /// <summary>
    /// Lists models in TypeSafe format
    /// </summary>
    public async Task<TypeSafeModelsResponse> GetTypeSafeModelsAsync(
        CancellationToken cancellationToken = default)
    {
        return await GetAsync<TypeSafeModelsResponse>("/v1/models", cancellationToken);
    }

    /// <summary>
    /// TypeSafe-compatible models response
    /// </summary>
    public sealed record TypeSafeModelsResponse
    {
        [JsonPropertyName("models")]
        public required TypeSafeModel[] Models { get; init; }
    }

    /// <summary>
    /// TypeSafe model info
    /// </summary>
    public sealed record TypeSafeModel
    {
        [JsonPropertyName("name")]
        public required string Name { get; init; }

        [JsonPropertyName("description")]
        public string? Description { get; init; }

        [JsonPropertyName("release_date")]
        public string? ReleaseDate { get; init; }
    }

    #endregion

    #region Model Management

    /// <summary>
    /// Lists local models
    /// </summary>
    public async Task<TagsResponse> ListModelsAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<TagsResponse>("/api/tags", cancellationToken);
    }

    /// <summary>
    /// Shows detailed model information
    /// </summary>
    public async Task<ShowResponse> ShowModelAsync(
        string model,
        CancellationToken cancellationToken = default)
    {
        var request = new ShowRequest { Model = model };
        return await PostAsync<ShowRequest, ShowResponse>("/api/show", request, cancellationToken);
    }

    /// <summary>
    /// Lists running models
    /// </summary>
    public async Task<PsResponse> ListRunningModelsAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<PsResponse>("/api/ps", cancellationToken);
    }

    /// <summary>
    /// Pulls a model from the registry (with progress streaming)
    /// </summary>
    public async IAsyncEnumerable<PullProgress> PullModelAsync(
        string model,
        bool stream = true,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var request = new PullRequest { Model = model, Stream = stream };
        await foreach (var progress in PostStreamAsync<PullRequest, PullProgress>("/api/pull", request, cancellationToken))
        {
            yield return progress;
        }
    }

    /// <summary>
    /// Pulls a model and waits for completion (non-streaming)
    /// </summary>
    public async Task<PullProgress> PullModelWaitAsync(
        string model,
        CancellationToken cancellationToken = default)
    {
        PullProgress? lastProgress = null;
        await foreach (var progress in PullModelAsync(model, stream: true, cancellationToken))
        {
            lastProgress = progress;
            if (progress.IsError)
            {
                throw CreateException(progress.Error!, progress.Code);
            }
        }
        return lastProgress ?? new PullProgress { Status = "unknown" };
    }

    /// <summary>
    /// Deletes a model
    /// </summary>
    public async Task DeleteModelAsync(string model, CancellationToken cancellationToken = default)
    {
        var request = new DeleteRequest { Model = model };
        await DeleteAsync<DeleteRequest, object>("/api/delete", request, cancellationToken);
    }

    /// <summary>
    /// Copies a model to a new name
    /// </summary>
    public async Task CopyModelAsync(
        string source,
        string destination,
        CancellationToken cancellationToken = default)
    {
        var request = new CopyRequest { Source = source, Destination = destination };
        await PostAsync<CopyRequest, object>("/api/copy", request, cancellationToken);
    }

    /// <summary>
    /// Creates a model from another model (with progress streaming)
    /// </summary>
    public async IAsyncEnumerable<CreateProgress> CreateModelAsync(
        CreateRequest request,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var progress in PostStreamAsync<CreateRequest, CreateProgress>("/api/create", request, cancellationToken))
        {
            yield return progress;
        }
    }

    /// <summary>
    /// Creates a model and waits for completion
    /// </summary>
    public async Task<CreateProgress> CreateModelWaitAsync(
        CreateRequest request,
        CancellationToken cancellationToken = default)
    {
        CreateProgress? lastProgress = null;
        await foreach (var progress in CreateModelAsync(request, cancellationToken))
        {
            lastProgress = progress;
            if (progress.IsError)
            {
                throw CreateException(progress.Error!, progress.Code);
            }
        }
        return lastProgress ?? new CreateProgress { Status = "unknown" };
    }

    /// <summary>
    /// Gets server version
    /// </summary>
    public async Task<VersionResponse> GetVersionAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<VersionResponse>("/api/version", cancellationToken);
    }

    #endregion

    #region Presets

    /// <summary>
    /// Lists all presets
    /// </summary>
    public async Task<PresetsResponse> ListPresetsAsync(CancellationToken cancellationToken = default)
    {
        return await GetAsync<PresetsResponse>("/api/presets", cancellationToken);
    }

    /// <summary>
    /// Creates a custom preset
    /// </summary>
    public async Task CreatePresetAsync(
        CreatePresetRequest request,
        CancellationToken cancellationToken = default)
    {
        await PostAsync<CreatePresetRequest, object>("/api/presets/create", request, cancellationToken);
    }

    /// <summary>
    /// Shows a preset
    /// </summary>
    public async Task<ShowResponse> ShowPresetAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        var request = new ShowPresetRequest { Name = name };
        return await PostAsync<ShowPresetRequest, ShowResponse>("/api/presets/show", request, cancellationToken);
    }

    /// <summary>
    /// Deletes a custom preset
    /// </summary>
    public async Task DeletePresetAsync(string name, CancellationToken cancellationToken = default)
    {
        var request = new DeletePresetRequest { Name = name };
        await PostAsync<DeletePresetRequest, object>("/api/presets/delete", request, cancellationToken);
    }

    #endregion

    #region HTTP Helpers

    private async Task<TResponse> GetAsync<TResponse>(string path, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(path, cancellationToken);
        return await HandleResponse<TResponse>(response);
    }

    private async Task<TResponse> PostAsync<TRequest, TResponse>(
        string path,
        TRequest request,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _httpClient.PostAsync(path, content, cancellationToken);
        return await HandleResponse<TResponse>(response);
    }

    private async Task<TResponse> DeleteAsync<TRequest, TResponse>(
        string path,
        TRequest request,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var httpRequest = new HttpRequestMessage(HttpMethod.Delete, path)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        return await HandleResponse<TResponse>(response);
    }

    private async IAsyncEnumerable<TResponse> PostStreamAsync<TRequest, TResponse>(
        string path,
        TRequest request,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(request, _jsonOptions);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        using var response = await _httpClient.PostAsync(path, content, cancellationToken);

        await EnsureSuccessStatusCode(response);

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream, Encoding.UTF8);

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            var progress = JsonSerializer.Deserialize<TResponse>(line, _jsonOptions);
            if (progress != null)
                yield return progress;
        }
    }

    private async Task<TResponse> HandleResponse<TResponse>(HttpResponseMessage response)
    {
        await EnsureSuccessStatusCode(response);

        var json = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(json))
        {
            return default!;
        }

        return JsonSerializer.Deserialize<TResponse>(json, _jsonOptions)!;
    }

    private async Task EnsureSuccessStatusCode(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var json = await response.Content.ReadAsStringAsync();
        string? errorMessage = null;
        string? errorCode = null;
        ValidationError[]? validationErrors = null;

        try
        {
            var errorDoc = JsonDocument.Parse(json);
            if (errorDoc.RootElement.TryGetProperty("error", out var errorProp))
                errorMessage = errorProp.GetString();
            if (errorDoc.RootElement.TryGetProperty("code", out var codeProp))
                errorCode = codeProp.GetString();
            if (errorDoc.RootElement.TryGetProperty("detail", out var detailProp))
            {
                validationErrors = JsonSerializer.Deserialize<ValidationError[]>(detailProp.GetRawText(), _jsonOptions);
            }
        }
        catch
        {
            errorMessage = json;
        }

        var code = string.IsNullOrEmpty(errorCode)
            ? OllayaErrorCode.Unknown
            : OllayaException.ParseCode(errorCode);

        var requestId = response.Headers.TryGetValues("X-Request-Id", out var ids)
            ? ids.FirstOrDefault()
            : null;

        if (code == OllayaErrorCode.InvalidRequest && validationErrors?.Length > 0)
        {
            throw new OllayaValidationException(
                errorMessage ?? "Validation failed",
                validationErrors,
                (int)response.StatusCode,
                requestId);
        }

        throw new OllayaException(
            errorMessage ?? $"HTTP {(int)response.StatusCode}",
            code,
            (int)response.StatusCode,
            validationErrors != null ? JsonSerializer.Serialize(validationErrors) : null,
            requestId);
    }

    private static OllayaException CreateException(string message, string? code)
    {
        var errorCode = string.IsNullOrEmpty(code)
            ? OllayaErrorCode.Unknown
            : OllayaException.ParseCode(code);
        return new OllayaException(message, errorCode, 500);
    }

    #endregion

    public void Dispose()
    {
        if (!_disposed)
        {
            _httpClient.Dispose();
            _disposed = true;
        }
    }
}