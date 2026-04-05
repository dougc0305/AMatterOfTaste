using AMatterOfTaste.Api.Data;
using AMatterOfTaste.Api.Models.DTOs;
using AMatterOfTaste.Api.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AMatterOfTaste.Api.Controllers;

[ApiController]
[Route("api/import")]
public class ImportController : ControllerBase
{
    private readonly AppDbContext _db;

    public ImportController(AppDbContext db)
    {
        _db = db;
    }

    [HttpPost("json")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ImportJson([FromBody] List<LegacyRecipeDto> recipes)
    {
        var categories = await _db.RecipeCategories
            .Where(c => c.IsActive)
            .ToListAsync();

        var existingTitles = await _db.Recipes
            .Where(r => r.IsActive)
            .Select(r => r.Title.ToLower())
            .ToListAsync();

        var imported = 0;
        var skipped = 0;

        foreach (var dto in recipes)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                skipped++;
                continue;
            }

            if (existingTitles.Contains(dto.Title.ToLower().Trim()))
            {
                skipped++;
                continue;
            }

            // Look up or create category
            var categoryName = string.IsNullOrWhiteSpace(dto.Category) ? "Other" : dto.Category.Trim();
            var category = categories.FirstOrDefault(c =>
                string.Equals(c.Name, categoryName, StringComparison.OrdinalIgnoreCase));

            if (category == null)
            {
                category = new RecipeCategory
                {
                    Name = categoryName,
                    SortOrder = categories.Count,
                    IsActive = true,
                    CreatedById = 1,
                    ModifiedById = 1,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedDate = DateTime.UtcNow
                };
                _db.RecipeCategories.Add(category);
                await _db.SaveChangesAsync();
                categories.Add(category);
            }

            // Build notes from raw ingredients + instructions
            var notesParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(dto.Ingredients))
            {
                notesParts.Add("INGREDIENTS:\n" + dto.Ingredients.Trim());
            }
            if (!string.IsNullOrWhiteSpace(dto.Instructions))
            {
                notesParts.Add("INSTRUCTIONS:\n" + dto.Instructions.Trim());
            }

            var recipe = new Recipe
            {
                Title = dto.Title.Trim(),
                Description = dto.Description?.Trim(),
                Notes = notesParts.Count > 0 ? string.Join("\n\n", notesParts) : null,
                CategoryId = category.Id,
                IsActive = true,
                CreatedById = 1,
                ModifiedById = 1,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            };

            _db.Recipes.Add(recipe);
            existingTitles.Add(dto.Title.ToLower().Trim());
            imported++;
        }

        await _db.SaveChangesAsync();

        return Ok(new { imported, skipped, total = recipes.Count });
    }
}
