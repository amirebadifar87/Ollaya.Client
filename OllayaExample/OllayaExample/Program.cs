using Ollaya.Client;
using Ollaya.Client.Extensions;
using Ollaya.Client.Models;

var client = new OllayaClient(); // defaults http://localhost:11435

var state = "I installed the app, but my account has not been fully verified yet. Please follow up as soon as possible.";

var response = await client.DecideAsync(
    model: "laya:multilingual",
    state: state,
    questions: QuestionBuilders.Questions(
        ("Department", QuestionBuilders.Choice(new[] { "Account Management", "Billing", "Feedback & Suggestions" })),
        ("Priority", QuestionBuilders.Score(new[] { "Low", "Medium", "High", "Urgent" })),
        ("Spam", QuestionBuilders.Noul("It was spam."))
    )
);

var department = (ChoiceAnswer)response.Answers["Department"];
var priority = (ScoreAnswer)response.Answers["Priority"];
var spam = (NoulAnswer)response.Answers["Spam"];

Console.WriteLine($"State: {state}");
Console.WriteLine($"Department: {department.Choice}");
Console.WriteLine($"Priority: {priority.Score}");
Console.WriteLine($"Spam: {(spam.Noul <0.5 ? "False":"True")}");
