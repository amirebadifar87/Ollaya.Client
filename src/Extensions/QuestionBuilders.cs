using Ollaya.Client.Models;

namespace Ollaya.Client.Extensions;

/// <summary>
/// Fluent builders for creating questions easily
/// </summary>
public static class QuestionBuilders
{
    /// <summary>
    /// Creates a choice question
    /// </summary>
    public static ChoiceQuestion Choice(
        object criteria,
        object? instructions = null)
    {
        return new ChoiceQuestion
        {
            Criteria = criteria,
            Instructions = instructions
        };
    }

    /// <summary>
    /// Creates a choice question with label->description criteria
    /// </summary>
    public static ChoiceQuestion Choice(
        Dictionary<string, string> criteria,
        object? instructions = null)
    {
        return new ChoiceQuestion
        {
            Criteria = criteria,
            Instructions = instructions
        };
    }

    /// <summary>
    /// Creates a choice question with array of labels
    /// </summary>
    public static ChoiceQuestion Choice(
        string[] criteria,
        object? instructions = null)
    {
        return new ChoiceQuestion
        {
            Criteria = criteria,
            Instructions = instructions
        };
    }

    /// <summary>
    /// Creates a score question
    /// </summary>
    public static ScoreQuestion Score(
        string[] criteria,
        object? instructions = null)
    {
        return new ScoreQuestion
        {
            Criteria = criteria,
            Instructions = instructions
        };
    }

    /// <summary>
    /// Creates a NOUL question
    /// </summary>
    public static NoulQuestion Noul(
        object? instructions = null,
        string? trueLabel = null,
        string? falseLabel = null)
    {
        return new NoulQuestion
        {
            Instructions = instructions,
            Criteria = (trueLabel != null || falseLabel != null)
                ? new NoulCriteria
                {
                    TrueLabel = trueLabel ?? "True",
                    FalseLabel = falseLabel ?? "False"
                }
                : null
        };
    }

    /// <summary>
    /// Creates a dictionary of questions from tuples
    /// </summary>
    public static Dictionary<string, QuestionBase> Questions(
        params (string Key, QuestionBase Question)[] questions)
    {
        return questions.ToDictionary(q => q.Key, q => q.Question);
    }
}