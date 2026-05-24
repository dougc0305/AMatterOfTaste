using AMatterOfTaste.Api.Data;
using AMatterOfTaste.Api.Models.DTOs;
using AMatterOfTaste.Api.Models.Entities;
using AMatterOfTaste.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AMatterOfTaste.Api.Controllers;

[ApiController]
[Route("api/recipes")]
public class RecipesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public RecipesController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? search = null)
    {
        var query = _db.Recipes
            .Include(r => r.Category)
            .Include(r => r.Photos)
            .Where(r => r.IsActive);

        if (categoryId.HasValue)
            query = query.Where(r => r.CategoryId == categoryId.Value);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => r.Title.ToLower().Contains(search.ToLower()));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(r => r.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new RecipeListDto
            {
                Id = r.Id,
                Title = r.Title,
                Description = r.Description,
                Attribution = r.Attribution,
                CategoryId = r.CategoryId,
                CategoryName = r.Category.Name,
                PrimaryPhotoFilename = r.Photos
                    .Where(p => p.IsActive && p.IsPrimary)
                    .Select(p => p.Filename)
                    .FirstOrDefault(),
                ViewCount = r.ViewCount
            })
            .ToListAsync();

        return Ok(new RecipePagedResultDto
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, [FromQuery] bool noCount = false)
    {
        var recipe = await _db.Recipes
            .Include(r => r.Category)
            .Include(r => r.Ingredients.Where(i => i.IsActive).OrderBy(i => i.SortOrder))
            .Include(r => r.Steps.Where(s => s.IsActive).OrderBy(s => s.StepNumber))
            .Include(r => r.Photos.Where(p => p.IsActive).OrderBy(p => p.SortOrder))
            .FirstOrDefaultAsync(r => r.Id == id && r.IsActive);

        if (recipe == null)
            return NotFound();

        var shouldCount = !noCount && !CountFilter.IsExcluded(HttpContext, _config);
        if (shouldCount)
        {
            await _db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE recipe SET viewcount = viewcount + 1 WHERE id = {id}");
        }

        return Ok(new RecipeDetailDto
        {
            Id = recipe.Id,
            Title = recipe.Title,
            Description = recipe.Description,
            Notes = recipe.Notes,
            Story = recipe.Story,
            Attribution = recipe.Attribution,
            OriginalText = recipe.OriginalText,
            CategoryId = recipe.CategoryId,
            CategoryName = recipe.Category.Name,
            Servings = recipe.Servings,
            PrepTimeMinutes = recipe.PrepTimeMinutes,
            CookTimeMinutes = recipe.CookTimeMinutes,
            ViewCount = shouldCount ? recipe.ViewCount + 1 : recipe.ViewCount,
            Ingredients = recipe.Ingredients.Select(i => new IngredientDto
            {
                Id = i.Id,
                SortOrder = i.SortOrder,
                Quantity = i.Quantity,
                Unit = i.Unit,
                Name = i.Name
            }).ToList(),
            Steps = recipe.Steps.Select(s => new StepDto
            {
                Id = s.Id,
                StepNumber = s.StepNumber,
                Instruction = s.Instruction
            }).ToList(),
            Photos = recipe.Photos.Select(p => new PhotoDto
            {
                Id = p.Id,
                Filename = p.Filename,
                IsPrimary = p.IsPrimary,
                SortOrder = p.SortOrder
            }).ToList()
        });
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(RecipeCreateDto dto)
    {
        var recipe = new Recipe
        {
            Title = dto.Title,
            Description = dto.Description,
            Notes = dto.Notes,
            Story = dto.Story,
            Attribution = dto.Attribution,
            OriginalText = dto.OriginalText,
            CategoryId = dto.CategoryId,
            Servings = dto.Servings,
            PrepTimeMinutes = dto.PrepTimeMinutes,
            CookTimeMinutes = dto.CookTimeMinutes,
            IsActive = true,
            CreatedById = 1,
            ModifiedById = 1,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow,
            Ingredients = dto.Ingredients.Select(i => new RecipeIngredient
            {
                SortOrder = i.SortOrder,
                Quantity = i.Quantity,
                Unit = i.Unit,
                Name = i.Name,
                IsActive = true,
                CreatedById = 1,
                ModifiedById = 1,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            }).ToList(),
            Steps = dto.Steps.Select(s => new RecipeStep
            {
                StepNumber = s.StepNumber,
                Instruction = s.Instruction,
                IsActive = true,
                CreatedById = 1,
                ModifiedById = 1,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            }).ToList()
        };

        _db.Recipes.Add(recipe);
        await _db.SaveChangesAsync();

        return Ok(new { recipe.Id });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, RecipeUpdateDto dto)
    {
        var recipe = await _db.Recipes
            .Include(r => r.Ingredients)
            .Include(r => r.Steps)
            .FirstOrDefaultAsync(r => r.Id == id && r.IsActive);

        if (recipe == null)
            return NotFound();

        recipe.Title = dto.Title;
        recipe.Description = dto.Description;
        recipe.Notes = dto.Notes;
        recipe.Story = dto.Story;
        recipe.Attribution = dto.Attribution;
        recipe.OriginalText = dto.OriginalText;
        recipe.CategoryId = dto.CategoryId;
        recipe.Servings = dto.Servings;
        recipe.PrepTimeMinutes = dto.PrepTimeMinutes;
        recipe.CookTimeMinutes = dto.CookTimeMinutes;
        recipe.ModifiedDate = DateTime.UtcNow;

        // Soft-delete existing ingredients and steps, then add new ones
        foreach (var ingredient in recipe.Ingredients)
            ingredient.IsActive = false;
        foreach (var step in recipe.Steps)
            step.IsActive = false;

        foreach (var i in dto.Ingredients)
        {
            recipe.Ingredients.Add(new RecipeIngredient
            {
                SortOrder = i.SortOrder,
                Quantity = i.Quantity,
                Unit = i.Unit,
                Name = i.Name,
                IsActive = true,
                CreatedById = 1,
                ModifiedById = 1,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            });
        }

        foreach (var s in dto.Steps)
        {
            recipe.Steps.Add(new RecipeStep
            {
                StepNumber = s.StepNumber,
                Instruction = s.Instruction,
                IsActive = true,
                CreatedById = 1,
                ModifiedById = 1,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();

        return Ok(new { recipe.Id });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var recipe = await _db.Recipes
            .FirstOrDefaultAsync(r => r.Id == id && r.IsActive);

        if (recipe == null)
            return NotFound();

        recipe.IsActive = false;
        recipe.ModifiedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return NoContent();
    }
}
