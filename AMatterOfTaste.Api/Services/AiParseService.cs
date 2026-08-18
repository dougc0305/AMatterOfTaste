using System.Text.Json;
using Anthropic;
using Anthropic.Models.Messages;

namespace AMatterOfTaste.Api.Services;

/// <summary>
/// Server-side proxy for the AI recipe parser. Holds the Anthropic API key
/// (config key Anthropic:ApiKey, typically set via the Anthropic__ApiKey
/// environment variable on the server) so it never reaches the browser.
/// </summary>
public class AiParseService
{
    private const string Model = "claude-sonnet-5";

    private readonly AnthropicClient? _client;

    public AiParseService(IConfiguration config)
    {
        var apiKey = config["Anthropic:ApiKey"];
        _client = string.IsNullOrWhiteSpace(apiKey)
            ? null
            : new AnthropicClient { ApiKey = apiKey };
    }

    /// <summary>
    /// Sends the prompt to Claude and returns the response as validated JSON text.
    /// Throws InvalidOperationException on configuration or parse problems — the
    /// global exception handler surfaces the message to the admin UI.
    /// </summary>
    public async Task<string> ParseAsync(string prompt)
    {
        if (_client == null)
            throw new InvalidOperationException(
                "Anthropic API key is not configured. Set the Anthropic__ApiKey environment variable on the server (then restart IIS).");

        var response = await _client.Messages.Create(new MessageCreateParams
        {
            Model = Model,
            MaxTokens = 4096,
            OutputConfig = new OutputConfig { Effort = Effort.Low },
            Messages = [new() { Role = Role.User, Content = prompt }],
        });

        var text = string.Concat(
            response.Content.Select(b => b.Value).OfType<TextBlock>().Select(t => t.Text));

        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("The AI returned an empty response. Try again.");

        // The prompt asks for bare JSON, but strip markdown fences defensively.
        text = text.Trim();
        if (text.StartsWith("```"))
        {
            var start = text.IndexOf('\n');
            var end = text.LastIndexOf("```", StringComparison.Ordinal);
            if (start >= 0 && end > start)
                text = text[(start + 1)..end].Trim();
        }

        try
        {
            using var _ = JsonDocument.Parse(text);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException("The AI response was not valid JSON. Try again or enter the details manually.");
        }

        return text;
    }
}
