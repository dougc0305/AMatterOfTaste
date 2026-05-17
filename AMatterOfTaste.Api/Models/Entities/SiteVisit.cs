namespace AMatterOfTaste.Api.Models.Entities;

public class SiteVisit : IntIdentityBase
{
    public DateOnly VisitDate { get; set; }
    public int Count { get; set; }
}
