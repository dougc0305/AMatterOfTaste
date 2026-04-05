namespace AMatterOfTaste.Api.Models.Entities;

public class RecipeIngredient : IntIdentityBase
{
    public int RecipeId { get; set; }
    public int SortOrder { get; set; }
    public string? Quantity { get; set; }
    public string? Unit { get; set; }
    public string Name { get; set; } = string.Empty;

    public Recipe Recipe { get; set; } = null!;
}
