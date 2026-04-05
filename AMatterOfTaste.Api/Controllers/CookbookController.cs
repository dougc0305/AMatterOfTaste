using AMatterOfTaste.Api.Data;
using AMatterOfTaste.Api.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AMatterOfTaste.Api.Controllers;

[ApiController]
[Route("api/cookbook")]
public class CookbookController : ControllerBase
{
    private readonly AppDbContext _db;

    public CookbookController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var user = await _db.AppUsers
            .FirstOrDefaultAsync(u => u.CookbookSlug == slug && u.IsActive);

        if (user == null)
            return NotFound();

        var favorites = await _db.UserFavorites
            .Include(f => f.Recipe)
                .ThenInclude(r => r.Photos)
            .Where(f => f.UserId == user.Id && f.IsActive && f.Recipe.IsActive)
            .OrderBy(f => f.Recipe.Title)
            .Select(f => new RecipeListDto
            {
                Id = f.Recipe.Id,
                Title = f.Recipe.Title,
                Description = f.Recipe.Description,
                Attribution = f.Recipe.Attribution,
                CategoryId = f.Recipe.CategoryId,
                CategoryName = f.Recipe.Category.Name,
                PrimaryPhotoFilename = f.Recipe.Photos
                    .Where(p => p.IsActive && p.IsPrimary)
                    .Select(p => p.Filename)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(new
        {
            UserName = user.Name,
            Recipes = favorites
        });
    }
}
