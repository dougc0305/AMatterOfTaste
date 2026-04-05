namespace AMatterOfTaste.Api.Models.Entities;

public class RecipeCategory : IntIdentityBase
{
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public ICollection<Recipe> Recipes { get; set; } = [];
}
