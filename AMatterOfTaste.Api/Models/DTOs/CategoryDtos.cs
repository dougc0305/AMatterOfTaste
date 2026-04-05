namespace AMatterOfTaste.Api.Models.DTOs;

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class CategoryCreateDto
{
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class CategoryUpdateDto
{
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
