namespace AMatterOfTaste.Api.Models.Entities;

public class AppUser : IntIdentityBase
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? CookbookSlug { get; set; }
    public bool IsAdmin { get; set; }

    public ICollection<UserFavorite> Favorites { get; set; } = [];
}
