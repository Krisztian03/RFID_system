using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace RfidBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Csak bejelentkezett userek
public class EmployeeController : ControllerBase
{
    private readonly AppDbContext _db;

    public EmployeeController(AppDbContext db)
    {
        _db = db;
    }

    // GET: api/employee (Admin látja az összest, Worker csak saját magát)
    [HttpGet]
    public async Task<IActionResult> GetEmployees()
    {
        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

        if (userRole == "Admin")
        {
            // Admin látja az összest
            var all = await _db.Employees
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.RfidUid,
                    e.HourlyRate,
                    e.Role
                })
                .ToListAsync();

            return Ok(all);
        }
        else
        {
            // Worker csak saját magát látja
            var employee = await _db.Employees
                .Where(e => e.Name.Replace(" ", "").ToLower() + "@rfid.com" == userEmail)
                .Select(e => new
                {
                    e.Id,
                    e.Name,
                    e.RfidUid,
                    e.HourlyRate,
                    e.Role
                })
                .FirstOrDefaultAsync();

            if (employee == null)
                return NotFound(new { message = "Nincs kapcsolódó dolgozó!" });

            return Ok(new[] { employee });
        }
    }

    // GET: api/employee/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetEmployee(int id)
    {
        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

        var employee = await _db.Employees.FindAsync(id);
        if (employee == null)
            return NotFound(new { message = "Dolgozó nem található!" });

        // Worker csak saját magát nézheti meg
        if (userRole != "Admin")
        {
            var currentEmployee = await _db.Employees
                .FirstOrDefaultAsync(e => e.Name.Replace(" ", "").ToLower() + "@rfid.com" == userEmail);

            if (currentEmployee?.Id != id)
                return Forbid();
        }

        return Ok(new
        {
            employee.Id,
            employee.Name,
            employee.RfidUid,
            employee.HourlyRate,
            employee.Role
        });
    }

    // PUT: api/employee/{id} (csak Admin)
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeRequest request)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee == null)
            return NotFound(new { message = "Dolgozó nem található!" });

        // RFID egyediség ellenőrzése
        if (employee.RfidUid != request.RfidUid)
        {
            var existing = await _db.Employees
                .FirstOrDefaultAsync(e => e.RfidUid == request.RfidUid && e.Id != id);

            if (existing != null)
                return BadRequest(new { message = "Ez az RFID kártya már használatban van!" });
        }

        employee.Name = request.Name;
        employee.RfidUid = request.RfidUid;
        employee.HourlyRate = request.HourlyRate;
        employee.Role = request.Role;

        await _db.SaveChangesAsync();

        return Ok(new { message = "Dolgozó frissítve!" });
    }

    // DELETE: api/employee/{id} (csak Admin)
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee == null)
            return NotFound(new { message = "Dolgozó nem található!" });

        _db.Employees.Remove(employee);
        await _db.SaveChangesAsync();

        return Ok(new { message = "Dolgozó törölve!" });
    }

    public class UpdateEmployeeRequest
    {
        public string Name { get; set; } = null!;
        public string RfidUid { get; set; } = null!;
        public decimal HourlyRate { get; set; }
        public string Role { get; set; } = "Worker";
    }
}