using Microsoft.AspNetCore.Mvc;

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

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] RfidRequest request)
    {
        var result = await _service.ProcessUidAsync(request.Uid);
        if (result == null)
            return NotFound(new { message = "Ismeretlen UID" });

        return Ok(new
        {
            name = result.EmployeeName,
            action = result.Action.ToString(),
            timestampUtc = result.TimestampUtc
        });
    }
}
