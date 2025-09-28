using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAccessService _accessSvc;

    public AdminController(AppDbContext db, IAccessService accessSvc)
    {
        _db = db;
        _accessSvc = accessSvc;
    }

    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployees()
    {
        var list = await _db.Employees.Select(e => new { e.Id, e.Name, e.RfidUid, e.HourlyRate }).ToListAsync();
        return Ok(list);
    }

    [HttpGet("summary/{employeeId}")]
    public async Task<IActionResult> GetSummary(int employeeId, DateTime? from, DateTime? to)
    {
        var fromUtc = from ?? DateTime.UtcNow.Date.AddMonths(-1);
        var toUtc = to ?? DateTime.UtcNow;
        var data = await _accessSvc.GetEmployeeSummaryAsync(employeeId, fromUtc, toUtc);
        return Ok(data);
    }
}
