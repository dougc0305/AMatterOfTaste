namespace AMatterOfTaste.Api.Models.Entities;

public class RecipeStep : IntIdentityBase
{
    public int RecipeId { get; set; }
    public int StepNumber { get; set; }
    public string Instruction { get; set; } = string.Empty;

    public Recipe Recipe { get; set; } = null!;
}
