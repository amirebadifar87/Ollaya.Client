# Ollaya.Client

A simple, idiomatic .NET client for [Ollaya](https://ollaya.dev) - local decision models API.

## Installation

```bash
dotnet add package Ollaya.Client
```

## Quick Start

```csharp
using Ollaya.Client;
using Ollaya.Client.Extensions;
using Ollaya.Client.Models;

// Create client (defaults to http://localhost:11435)
var client = new OllayaClient();

// Or with custom options
var client = new OllayaClient(new OllayaClientOptions
{
    BaseUrl = "http://localhost:11435",
    ApiKey = "your-api-key", // If server requires auth
    Timeout = TimeSpan.FromMinutes(5)
});
```

## Making Decisions

### Native API (/api/decide)

```csharp
using Ollaya.Client.Extensions;

// Simple decision with fluent question builders
var response = await client.DecideAsync(
    model: "laya:en",
    state: "I was charged twice for my subscription this month. Please refund the second charge.",
    questions: QuestionBuilders.Questions(
        ("department", QuestionBuilders.Choice(new Dictionary<string, string>
        {
            ["billing"] = "Payments, invoices and refunds",
            ["technical"] = "Bugs, errors and outages",
            ["account"] = "Login, profile and settings"
        }, "Which team should handle this ticket?")),
        ("urgency", QuestionBuilders.Score(new[]
        {
            "Can wait",
            "Needs attention this week",
            "Needs attention today"
        }, "How urgent is this ticket?")),
        ("refund", QuestionBuilders.Noul(
            "The customer asks for money back.",
            trueLabel: "Asks for a refund",
            falseLabel: "Does not ask for a refund"))
    ),
    keepAlive: "10m" // Keep model loaded for 10 minutes
);

// Access answers
var department = response.Answers["department"] as ChoiceAnswer;
Console.WriteLine($"Department: {department.Choice} (confidence: {department.Confidence:P})");

var urgency = response.Answers["urgency"] as ScoreAnswer;
Console.WriteLine($"Urgency score: {urgency.Score:F2} ({urgency.Legend[((int)urgency.Score).ToString()]})");

var refund = response.Answers["refund"] as NoulAnswer;
Console.WriteLine($"Refund probability: {refund.Noul:P}");
```

### TypeSafe-Compatible API (/v1/systemone)

```csharp
// Use TypeSafe-compatible endpoint (same wire format as TypeSafe)
var response = await client.SystemOneAsync(
    model: "laya:en",
    state: "Customer is asking for a refund on a duplicate charge.",
    questions: QuestionBuilders.Questions(
        ("refund", QuestionBuilders.Noul("Customer wants money back"))
    )
);
```

## Model Management

```csharp
// List local models
var models = await client.ListModelsAsync();
foreach (var model in models.Models)
{
    Console.WriteLine($"{model.Name} - {model.Details.ParameterSize} ({model.Details.Format})");
}

// Pull a model (with progress)
await foreach (var progress in client.PullModelAsync("laya:en"))
{
    Console.WriteLine($"Pull: {progress.Status} - {progress.Completed}/{progress.Total}");
}

// Or wait for completion
await client.PullModelWaitAsync("laya:en");

// Show model details
var details = await client.ShowModelAsync("laya:en");
Console.WriteLine($"License: {details.License}");
Console.WriteLine($"Capabilities: {string.Join(", ", details.Capabilities ?? [])}");

// List running models
var running = await client.ListRunningModelsAsync();

// Delete a model
await client.DeleteModelAsync("old-model");

// Copy a model
await client.CopyModelAsync("laya:en", "my-custom-model");

// Create a model from a Modelfile-like config
await client.CreateModelWaitAsync(new CreateRequest
{
    Model = "my-triage",
    From = "laya:en",
    Questions = QuestionBuilders.Questions(
        ("department", QuestionBuilders.Choice(
            new[] { "billing", "technical", "account" },
            "Which team should handle this ticket?"))
    ),
    Parameters = new CreateParameters { Precision = "fp32" },
    Description = "Support ticket triage"
});
```

## Presets

```csharp
// List presets
var presets = await client.ListPresetsAsync();

// Create a custom preset
await client.CreatePresetAsync(new CreatePresetRequest
{
    Name = "billing-check",
    Description = "Billing, and how upset the customer is",
    Questions = QuestionBuilders.Questions(
        ("billing", QuestionBuilders.Noul("The message is about a charge, an invoice or a refund.")),
        ("tone", QuestionBuilders.Choice(new Dictionary<string, string>
        {
            ["calm"] = "",
            ["annoyed"] = "",
            ["angry"] = ""
        }, "How does the customer sound?"))
    )
});

// Use preset in decision
var response = await client.DecideAsync(new DecideRequest
{
    Model = "winnow:e4b",
    State = "I was charged twice this month.",
    Preset = "billing-check"
});
```

## Load/Unload Models

```csharp
// Preload a model (keep in memory indefinitely)
await client.LoadModelAsync("laya:en", keepAlive: -1);

// Unload a model
await client.UnloadModelAsync("laya:en");
```

## Vision Models

```csharp
// For vision models (decider:2b-vision, winnow:e4b-vision, winnow:12b-vision)
var imageBase64 = Convert.ToBase64String(File.ReadAllBytes("shelf.png"));

var response = await client.DecideAsync(new DecideRequest
{
    Model = "decider:2b-vision",
    State = "A photo from the warehouse camera.",
    Images = [imageBase64], // Base64 PNG
    Questions = QuestionBuilders.Questions(
        ("blocked", QuestionBuilders.Noul("Is the aisle blocked?")),
        ("fill", QuestionBuilders.Score(new[] { "empty", "half full", "full" }, "How full is the shelf?"))
    )
});
```

## Dependency Injection

```csharp
// Program.cs
builder.Services.AddOllayaClient(options =>
{
    options.BaseUrl = "http://localhost:11435";
    options.ApiKey = "your-key";
});

// Or from configuration
builder.Services.AddOllayaClient(builder.Configuration, "Ollaya");

// Usage in your service
public class MyService
{
    private readonly OllayaClient _ollaya;

    public MyService(OllayaClient ollaya)
    {
        _ollaya = ollaya;
    }

    public async Task<TriageResult> TriageTicket(string ticketText)
    {
        var response = await _ollaya.DecideAsync(
            model: "laya:en",
            state: ticketText,
            questions: QuestionBuilders.Questions(
                ("department", QuestionBuilders.Choice(
                    new[] { "billing", "technical", "account" })),
                ("urgency", QuestionBuilders.Score(
                    new[] { "low", "medium", "high", "critical" }))
            )
        );

        return new TriageResult
        {
            Department = ((ChoiceAnswer)response.Answers["department"]).Choice,
            Urgency = ((ScoreAnswer)response.Answers["urgency"]).Score
        };
    }
}
```

## Configuration (appsettings.json)

```json
{
  "Ollaya": {
    "BaseUrl": "http://localhost:11435",
    "ApiKey": "optional-api-key",
    "Timeout": "00:05:00"
  }
}
```

## Error Handling

```csharp
try
{
    var response = await client.DecideAsync(...);
}
catch (OllayaValidationException ex)
{
    // 422 INVALID_REQUEST - validation errors in ex.Errors
    foreach (var error in ex.Errors)
    {
        Console.WriteLine($"{string.Join(".", error.Loc)}: {error.Msg}");
    }
}
catch (OllayaException ex) when (ex.Code == OllayaErrorCode.ModelNotFound)
{
    // 404 - model not found, pull it first
    await client.PullModelWaitAsync("laya:en");
}
catch (OllayaException ex) when (OllayaException.IsRetryable(ex.Code))
{
    // Retryable errors: QueueFull, ModelLoadFailed, InferenceFailed, etc.
    await Task.Delay(1000);
    // retry...
}
catch (OllayaException ex)
{
    // Other errors
    Console.WriteLine($"Error: {ex.Code} - {ex.Message}");
}
```

## Keep Alive Options

```csharp
// Duration strings (Ollama-compatible)
keepAlive: "5m"      // 5 minutes
keepAlive: "1h30m"   // 1 hour 30 minutes
keepAlive: "300ms"   // 300 milliseconds
keepAlive: 300       // 300 nanoseconds (as number)
keepAlive: "300"     // 300 nanoseconds (as string)

// Special values
keepAlive: 0         // Unload immediately after request
keepAlive: "0"       // Unload immediately
keepAlive: -1        // Keep loaded until server stops
keepAlive: "-5m"     // Negative = keep loaded
keepAlive: null      // Use server default (OLLAYA_KEEP_ALIVE, default 5m)
```

## Laya Extras

```csharp
// Request laya-specific confidence and act probability
var response = await client.DecideAsync(new DecideRequest
{
    Model = "laya:en",
    State = "...",
    Questions = ...,
    Extras = ["laya"]
});

var answer = response.Answers["question"] as ChoiceAnswer;
if (answer.Laya != null)
{
    Console.WriteLine($"Laya confidence: {answer.Laya.Confidence}");
    Console.WriteLine($"Act probability: {answer.Laya.ActProbability}");
}
```

## Routers

```csharp
// Use router model (e.g., "laya" or "laya:latest")
// The router automatically picks laya:en or laya:multilingual based on language
var response = await client.DecideAsync(
    model: "laya", // Router
    state: "This is English text",
    questions: ...
);

// Check which model actually answered
Console.WriteLine($"Answered by: {response.Model}"); // e.g., "laya:en"

// Access routing info
if (response.Routing != null)
{
    Console.WriteLine($"Route: {response.Routing.Route}"); // "english" or "multilingual"
    Console.WriteLine($"Reason: {response.Routing.Reason}");
}
```

## License

MIT