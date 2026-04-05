using AMatterOfTaste.Api.Data;
using AMatterOfTaste.Api.Models.DTOs;
using AMatterOfTaste.Api.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AMatterOfTaste.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public CategoriesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _db.RecipeCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                SortOrder = c.SortOrder
            })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CategoryCreateDto dto)
    {
        var category = new RecipeCategory
        {
            Name = dto.Name,
            SortOrder = dto.SortOrder,
            IsActive = true,
            CreatedById = 1,
            ModifiedById = 1,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        _db.RecipeCategories.Add(category);
        await _db.SaveChangesAsync();

        return Ok(new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            SortOrder = category.SortOrder
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, CategoryUpdateDto dto)
    {
        var category = await _db.RecipeCategories
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

        if (category == null)
            return NotFound();

        category.Name = dto.Name;
        category.SortOrder = dto.SortOrder;
        category.ModifiedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            SortOrder = category.SortOrder
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.RecipeCategories
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

        if (category == null)
            return NotFound();

        category.IsActive = false;
        category.ModifiedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return NoContent();
    }
}
