namespace AMatterOfTaste.Api.Models.DTOs;

public class LegacyRecipeDto
{
    public int Recipe_Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public string? Ingredients { get; set; }
    public string? Instructions { get; set; }
    public string? Add_Date { get; set; }
}
