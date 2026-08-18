using AMatterOfTaste.Api.Models.DTOs;
using AMatterOfTaste.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AMatterOfTaste.Api.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize(Roles = "Admin")]
public class AiController : ControllerBase
{
    private readonly AiParseService _ai;

    public AiController(AiParseService ai)
    {
        _ai = ai;
    }

    [HttpPost("parse-recipe")]
    public async Task<IActionResult> ParseRecipe(AiParseRecipeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Text))
            return BadRequest(new { message = "Recipe text is required." });

        var json = await _ai.ParseAsync($@"Parse the following recipe text into structured JSON. Return ONLY valid JSON, no markdown, no preamble. Use this exact shape:
{{
  ""title"": ""string"",
  ""description"": ""brief description"",
  ""servings"": number or null,
  ""preptimeminutes"": number or null,
  ""cooktimeminutes"": number or null,
  ""ingredients"": [{{ ""quantity"": ""string"", ""unit"": ""string"", ""name"": ""string"" }}],
  ""steps"": [{{ ""stepnumber"": 1, ""instruction"": ""string"" }}]
}}

Recipe text:
{dto.Text}");

        return Content(json, "application/json");
    }

    [HttpPost("parse-notes")]
    public async Task<IActionResult> ParseNotes(AiParseNotesDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Notes))
            return BadRequest(new { message = "Notes text is required." });

        var json = await _ai.ParseAsync($@"Parse the following recipe notes into structured ingredients and steps. The notes contain both ingredients and instructions mixed together. Separate them out. Return ONLY valid JSON, no markdown, no preamble. Use this exact shape:
{{
  ""ingredients"": [{{ ""quantity"": ""string or empty"", ""unit"": ""string or empty"", ""name"": ""string"" }}],
  ""steps"": [{{ ""stepnumber"": 1, ""instruction"": ""string"" }}]
}}

Recipe title: {dto.Title}

Notes:
{dto.Notes}");

        return Content(json, "application/json");
    }
}
