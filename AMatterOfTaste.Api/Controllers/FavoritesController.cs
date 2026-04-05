using System.Security.Claims;
using AMatterOfTaste.Api.Data;
using AMatterOfTaste.Api.Models.DTOs;
using AMatterOfTaste.Api.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AMatterOfTaste.Api.Controllers;

[ApiController]
[Route("api/favorites")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly AppDbContext _db;

    public FavoritesController(AppDbContext db)
    {
        _db = db;
    }

    private int GetUserId() =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetMyFavorites()
    {
        var userId = GetUserId();

        var favorites = await _db.UserFavorites
            .Include(f => f.Recipe)
                .ThenInclude(r => r.Photos)
            .Where(f => f.UserId == userId && f.IsActive && f.Recipe.IsActive)
            .OrderBy(f => f.Recipe.Title)
            .Select(f => new FavoriteDto
            {
                Id = f.Id,
                RecipeId = f.RecipeId,
                RecipeTitle = f.Recipe.Title,
                PrimaryPhotoFilename = f.Recipe.Photos
                    .Where(p => p.IsActive && p.IsPrimary)
                    .Select(p => p.Filename)
                    .FirstOrDefault()
            })
            .ToListAsync();

        return Ok(favorites);
    }

    [HttpPost]
    public async Task<IActionResult> AddFavorite(FavoriteCreateDto dto)
    {
        var userId = GetUserId();

        var exists = await _db.UserFavorites
            .AnyAsync(f => f.UserId == userId && f.RecipeId == dto.RecipeId && f.IsActive);

        if (exists)
            return Conflict(new { message = "Already in favorites" });

        var favorite = new UserFavorite
        {
            UserId = userId,
            RecipeId = dto.RecipeId,
            IsActive = true,
            CreatedById = userId,
            ModifiedById = userId,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        _db.UserFavorites.Add(favorite);
        await _db.SaveChangesAsync();

        return Ok(new { favorite.Id });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> RemoveFavorite(int id)
    {
        var userId = GetUserId();

        var favorite = await _db.UserFavorites
            .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId && f.IsActive);

        if (favorite == null)
            return NotFound();

        favorite.IsActive = false;
        favorite.ModifiedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return NoContent();
    }
}
