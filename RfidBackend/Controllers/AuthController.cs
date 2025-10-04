using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RfidBackend.Services;

namespace RfidBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly TokenService _tokenService;

    public AuthController(AppDbContext context, TokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    public class LoginRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class RegisterRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string RfidUid { get; set; } = null!;
        public decimal HourlyRate { get; set; }
        public string Role { get; set; } = "Worker";
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
            return Unauthorized(new { message = "Hibás email vagy jelszó!" });

        // Jelszó ellenőrzés
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Hibás email vagy jelszó!" });

        // Token generálás
        var token = _tokenService.GenerateToken(user);

        return Ok(new
        {
            token,
            user = new
            {
                id = user.Id,
                email = user.Email,
                role = user.Role,
                employeeId = user.EmployeeId,
                employeeName = user.Employee?.Name
            }
        });
    }

    // POST: api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Email ellenőrzés
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest(new { message = "Ez az email már használatban van!" });

        // RFID ellenőrzés
        if (await _context.Employees.AnyAsync(e => e.RfidUid == request.RfidUid))
            return BadRequest(new { message = "Ez az RFID kártya már használatban van!" });

        // Employee létrehozása
        var employee = new Employee
        {
            Name = request.Name,
            RfidUid = request.RfidUid,
            HourlyRate = request.HourlyRate,
            Role = request.Role
        };

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        // User létrehozása
        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            EmployeeId = employee.Id
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _tokenService.GenerateToken(user);

        return Ok(new
        {
            message = "Felhasználó sikeresen létrehozva!",
            token,
            user = new
            {
                id = user.Id,
                email = user.Email,
                role = user.Role,
                employeeId = employee.Id,
                employeeName = employee.Name
            }
        });
    }

    // GET: api/auth/me
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");

        var user = await _context.Users
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return NotFound();

        return Ok(new
        {
            id = user.Id,
            email = user.Email,
            role = user.Role,
            employeeId = user.EmployeeId,
            employeeName = user.Employee?.Name
        });
    }
}