namespace AMatterOfTaste.Api.Models.DTOs;

public class FavoriteDto
{
    public int Id { get; set; }
    public int RecipeId { get; set; }
    public string RecipeTitle { get; set; } = string.Empty;
    public string? PrimaryPhotoFilename { get; set; }
}

public class FavoriteCreateDto
{
    public int RecipeId { get; set; }
}
