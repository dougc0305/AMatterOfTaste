namespace AMatterOfTaste.Api.Models.DTOs;

public class AiParseRecipeDto
{
    public string Text { get; set; } = string.Empty;
}

public class AiParseNotesDto
{
    public string Title { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
