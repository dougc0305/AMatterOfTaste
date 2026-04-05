namespace AMatterOfTaste.Api.Models.DTOs;

public class RecipeListDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Attribution { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? PrimaryPhotoFilename { get; set; }
}

public class RecipeDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public string? Story { get; set; }
    public string? Attribution { get; set; }
    public string? OriginalText { get; set; }
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int? Servings { get; set; }
    public int? PrepTimeMinutes { get; set; }
    public int? CookTimeMinutes { get; set; }
    public List<IngredientDto> Ingredients { get; set; } = [];
    public List<StepDto> Steps { get; set; } = [];
    public List<PhotoDto> Photos { get; set; } = [];
}

public class IngredientDto
{
    public int Id { get; set; }
    public int SortOrder { get; set; }
    public string? Quantity { get; set; }
    public string? Unit { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class StepDto
{
    public int Id { get; set; }
    public int StepNumber { get; set; }
    public string Instruction { get; set; } = string.Empty;
}

public class PhotoDto
{
    public int Id { get; set; }
    public string Filename { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
}

public class RecipeCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public string? Story { get; set; }
    public string? Attribution { get; set; }
    public string? OriginalText { get; set; }
    public int CategoryId { get; set; }
    public int? Servings { get; set; }
    public int? PrepTimeMinutes { get; set; }
    public int? CookTimeMinutes { get; set; }
    public List<IngredientCreateDto> Ingredients { get; set; } = [];
    public List<StepCreateDto> Steps { get; set; } = [];
}

public class RecipeUpdateDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public string? Story { get; set; }
    public string? Attribution { get; set; }
    public string? OriginalText { get; set; }
    public int CategoryId { get; set; }
    public int? Servings { get; set; }
    public int? PrepTimeMinutes { get; set; }
    public int? CookTimeMinutes { get; set; }
    public List<IngredientCreateDto> Ingredients { get; set; } = [];
    public List<StepCreateDto> Steps { get; set; } = [];
}

public class IngredientCreateDto
{
    public int SortOrder { get; set; }
    public string? Quantity { get; set; }
    public string? Unit { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class StepCreateDto
{
    public int StepNumber { get; set; }
    public string Instruction { get; set; } = string.Empty;
}

public class RecipePagedResultDto
{
    public List<RecipeListDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
