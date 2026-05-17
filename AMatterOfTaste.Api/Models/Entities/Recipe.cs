namespace AMatterOfTaste.Api.Models.Entities;

public class Recipe : IntIdentityBase
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public string? Story { get; set; }
    public string? Attribution { get; set; }
    public string? OriginalText { get; set; }
    public int CategoryId { get; set; }
    public int? Servings { get; set; }
    public int? PrepTimeMinutes { get; set; }
    public int? CookTimeMinutes { get; set; }
    public int ViewCount { get; set; }

    public RecipeCategory Category { get; set; } = null!;
    public ICollection<RecipeIngredient> Ingredients { get; set; } = [];
    public ICollection<RecipeStep> Steps { get; set; } = [];
    public ICollection<RecipePhoto> Photos { get; set; } = [];
    public ICollection<UserFavorite> Favorites { get; set; } = [];
}
