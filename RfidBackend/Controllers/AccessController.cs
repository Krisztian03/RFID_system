using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace RfidBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccessController : ControllerBase
{
    private readonly IAccessService _service;

    public AccessController(IAccessService service)
    {
        _service = service;
    }

    public class RfidRequest
    {
        public string Uid { get; set; } = null!;
    }

    // ESP32 hívja ezt az endpointot - NINCS autentikáció!
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Post([FromBody] RfidRequest request)
    {
        var result = await _service.ProcessUidAsync(request.Uid);

        if (result == null)
            return NotFound(new { message = "Ismeretlen RFID kártya", uid = request.Uid });

        return Ok(new
        {
            success = true,
            employeeName = result.EmployeeName,
            action = result.Action.ToString(),
            timestamp = result.TimestampUtc
        });
    }

    // Összes access log lekérése (Admin jogosultság kell)
    [HttpGet("logs")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllLogs([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        // TODO: Implementáld az összes log lekérését
        return Ok(new { message = "Coming soon..." });
    }

    // Egy dolgozó access logjainak lekérése
    [HttpGet("logs/{employeeId}")]
    [Authorize]
    public async Task<IActionResult> GetEmployeeLogs(int employeeId, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

        // TODO: Ellenőrizd, hogy a user csak saját logját nézheti (kivéve Admin)

        var fromUtc = from ?? DateTime.UtcNow.Date.AddMonths(-1);
        var toUtc = to ?? DateTime.UtcNow;

        var summary = await _service.GetEmployeeSummaryAsync(employeeId, fromUtc, toUtc);

        return Ok(summary);
    }
}