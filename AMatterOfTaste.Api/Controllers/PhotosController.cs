using AMatterOfTaste.Api.Data;
using AMatterOfTaste.Api.Models.Entities;
using AMatterOfTaste.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AMatterOfTaste.Api.Controllers;

[ApiController]
[Route("api/photos")]
[Authorize(Roles = "Admin")]
public class PhotosController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;

    public PhotosController(AppDbContext db, IWebHostEnvironment env, IConfiguration config)
    {
        _db = db;
        _env = env;
        _config = config;
    }

    [HttpPost("upload/{recipeId}")]
    public async Task<IActionResult> Upload(int recipeId, IFormFile file)
    {
        var recipe = await _db.Recipes.FirstOrDefaultAsync(r => r.Id == recipeId && r.IsActive);
        if (recipe == null)
            return NotFound();

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No file provided" });

        var photosDir = PhotoStorage.GetDirectory(_config, _env);

        var ext = Path.GetExtension(file.FileName).ToLower();
        var filename = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(photosDir, filename);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var hasPrimary = await _db.RecipePhotos
            .AnyAsync(p => p.RecipeId == recipeId && p.IsPrimary && p.IsActive);

        var photo = new RecipePhoto
        {
            RecipeId = recipeId,
            Filename = filename,
            IsPrimary = !hasPrimary,
            SortOrder = 0,
            IsActive = true,
            CreatedById = 1,
            ModifiedById = 1,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow
        };

        _db.RecipePhotos.Add(photo);
        await _db.SaveChangesAsync();

        return Ok(new { photo.Id, photo.Filename, photo.IsPrimary });
    }

    [HttpPut("{id}/primary")]
    public async Task<IActionResult> SetPrimary(int id)
    {
        var photo = await _db.RecipePhotos
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (photo == null)
            return NotFound();

        // Clear primary from all photos on this recipe
        var siblings = await _db.RecipePhotos
            .Where(p => p.RecipeId == photo.RecipeId && p.IsActive)
            .ToListAsync();

        foreach (var sibling in siblings)
            sibling.IsPrimary = false;

        photo.IsPrimary = true;
        photo.ModifiedDate = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var photo = await _db.RecipePhotos
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        if (photo == null)
            return NotFound();

        photo.IsActive = false;
        photo.ModifiedDate = DateTime.UtcNow;

        // If this was the primary, promote another photo
        if (photo.IsPrimary)
        {
            photo.IsPrimary = false;
            var next = await _db.RecipePhotos
                .FirstOrDefaultAsync(p => p.RecipeId == photo.RecipeId && p.Id != id && p.IsActive);
            if (next != null)
                next.IsPrimary = true;
        }

        await _db.SaveChangesAsync();

        return NoContent();
    }
}
