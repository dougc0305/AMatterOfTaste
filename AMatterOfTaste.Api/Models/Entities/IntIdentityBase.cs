namespace AMatterOfTaste.Api.Models.Entities;

public abstract class IntIdentityBase
{
    public int Id { get; set; }
    public int CreatedById { get; set; }
    public DateTime CreatedDate { get; set; }
    public int ModifiedById { get; set; }
    public DateTime ModifiedDate { get; set; }
    public bool IsActive { get; set; } = true;
}
