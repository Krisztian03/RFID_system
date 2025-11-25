using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace RfidBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExportController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IAccessService _accessSvc;

    public ExportController(AppDbContext db, IAccessService accessSvc)
    {
        _db = db;
        _accessSvc = accessSvc;
    }

    // GET: api/export/employee/{id}/excel
    [HttpGet("employee/{employeeId}/excel")]
    public async Task<IActionResult> ExportEmployeeToExcel(
        int employeeId,
        DateTime? from,
        DateTime? to)
    {
        // Jogosultság ellenőrzés
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        var currentUserId = int.Parse(
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"
        );

        var currentUser = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == currentUserId);

        if (userRole != "Admin" && currentUser?.EmployeeId != employeeId)
            return Forbid();

        // Adatok lekérése
        var employee = await _db.Employees.FindAsync(employeeId);
        if (employee == null)
            return NotFound();

        var fromUtc = from ?? DateTime.UtcNow.Date.AddMonths(-1);
        var toUtc = to ?? DateTime.UtcNow;

        var summary = await _accessSvc.GetEmployeeSummaryAsync(
            employeeId,
            fromUtc,
            toUtc
        );

        // Excel generálás
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Munkaidő");

        // Fejléc
        worksheet.Cell(1, 1).Value = "MUNKAIDŐ KIMUTATÁS";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 16;

        worksheet.Cell(2, 1).Value = $"Dolgozó: {employee.Name}";
        worksheet.Cell(3, 1).Value = $"Időszak: {fromUtc:yyyy-MM-dd} - {toUtc:yyyy-MM-dd}";
        worksheet.Cell(4, 1).Value = $"Órabér: {employee.HourlyRate} Ft/óra";

        // Táblázat fejléc
        worksheet.Cell(6, 1).Value = "Dátum";
        worksheet.Cell(6, 2).Value = "Munkaórák";
        worksheet.Cell(6, 3).Value = "Fizetés (Ft)";

        worksheet.Range(6, 1, 6, 3).Style.Font.Bold = true;
        worksheet.Range(6, 1, 6, 3).Style.Fill.BackgroundColor = XLColor.LightGray;

        // Adatok
        int row = 7;
        foreach (var day in summary)
        {
            worksheet.Cell(row, 1).Value = day.Day.ToString("yyyy-MM-dd");
            worksheet.Cell(row, 2).Value = day.Hours;
            worksheet.Cell(row, 3).Value = day.Amount;
            row++;
        }

        // Összesítő
        row++;
        worksheet.Cell(row, 1).Value = "ÖSSZESEN:";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        worksheet.Cell(row, 2).Value = summary.Sum(s => s.Hours);
        worksheet.Cell(row, 2).Style.Font.Bold = true;
        worksheet.Cell(row, 3).Value = summary.Sum(s => s.Amount);
        worksheet.Cell(row, 3).Style.Font.Bold = true;

        // Oszlopok szélessége
        worksheet.Column(1).Width = 15;
        worksheet.Column(2).Width = 12;
        worksheet.Column(3).Width = 15;

        // Fájl visszaadása
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        var fileName = $"Munkaido_{employee.Name.Replace(" ", "_")}_{fromUtc:yyyyMM}.xlsx";

        return File(
            content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName
        );
    }
}