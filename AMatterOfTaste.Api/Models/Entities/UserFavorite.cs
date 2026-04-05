namespace AMatterOfTaste.Api.Models.Entities;

public class UserFavorite : IntIdentityBase
{
    public int UserId { get; set; }
    public int RecipeId { get; set; }

    public AppUser User { get; set; } = null!;
    public Recipe Recipe { get; set; } = null!;
}
