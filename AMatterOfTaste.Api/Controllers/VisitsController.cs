using AMatterOfTaste.Api.Data;
using AMatterOfTaste.Api.Models.DTOs;
using AMatterOfTaste.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AMatterOfTaste.Api.Controllers;

[ApiController]
[Route("api/visits")]
public class VisitsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public VisitsController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost("ping")]
    public async Task<IActionResult> Ping()
    {
        // Don't count the owner's own visits (logged-in admin or ignored IP).
        if (CountFilter.IsExcluded(HttpContext, _config))
            return NoContent();

        var today = DateOnly.FromDateTime(DateTime.Now);

        await _db.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO sitevisit (visitdate, count)
            VALUES ({today}, 1)
            ON CONFLICT (visitdate) DO UPDATE
            SET count = sitevisit.count + 1, modifieddate = now()");

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetVisitDays([FromQuery] int days = 90)
    {
        if (days < 1) days = 1;
        if (days > 365) days = 365;

        var endDate = DateOnly.FromDateTime(DateTime.Now);
        var startDate = endDate.AddDays(-(days - 1));

        var rows = await _db.SiteVisits
            .Where(v => v.VisitDate >= startDate && v.VisitDate <= endDate)
            .ToDictionaryAsync(v => v.VisitDate, v => v.Count);

        var result = new List<VisitDayDto>();
        for (var d = startDate; d <= endDate; d = d.AddDays(1))
        {
            result.Add(new VisitDayDto
            {
                Date = d,
                Count = rows.TryGetValue(d, out var c) ? c : 0
            });
        }

        return Ok(result);
    }
}
