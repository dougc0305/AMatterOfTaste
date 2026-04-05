namespace AMatterOfTaste.Api.Models.Entities;

public class RecipePhoto : IntIdentityBase
{
    public int RecipeId { get; set; }
    public string Filename { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }

    public Recipe Recipe { get; set; } = null!;
}
