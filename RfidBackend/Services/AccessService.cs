using Microsoft.EntityFrameworkCore;



public class AccessService : IAccessService
{
	private readonly AppDbContext _db;

	public AccessService(AppDbContext db)
	{
		_db = db;
	}

	public async Task<AccessResult?> ProcessUidAsync(string uid)
	{
		var employee = await _db.Employees.FirstOrDefaultAsync(e => e.RfidUid == uid);
		if (employee == null) return null;

		// Legutols� bel�p�s/kil�p�s volt, majd ment�s
		var last = await _db.AccessLogs
			.Where(l => l.EmployeeId == employee.Id)
			.OrderByDescending(l => l.TimestampUtc)
			.FirstOrDefaultAsync();

		var newType = (last == null || last.Type == AccessType.Exit) ? AccessType.Entry : AccessType.Exit;

		var log = new AccessLog
		{
			EmployeeId = employee.Id,
			TimestampUtc = DateTime.UtcNow,
			Type = newType,
			RawUid = uid
		};

		_db.AccessLogs.Add(log);
		await _db.SaveChangesAsync();

		return new AccessResult
		{
			EmployeeId = employee.Id,
			EmployeeName = employee.Name,
			Action = newType,
			TimestampUtc = log.TimestampUtc
		};
	}

	// Egyszer� napi �sszes�t�: p�ros�tjuk bel�ptet�seket �s kil�ptet�seket adott peri�duson bel�l
	public async Task<IEnumerable<WorkDaySummary>> GetEmployeeSummaryAsync(int employeeId, DateTime fromUtc, DateTime toUtc)
	{
		var logs = await _db.AccessLogs
			.Where(l => l.EmployeeId == employeeId && l.TimestampUtc >= fromUtc && l.TimestampUtc <= toUtc)
			.OrderBy(l => l.TimestampUtc)
			.ToListAsync();

		var daily = logs
			.GroupBy(l => l.TimestampUtc.Date)
			.Select(g =>
			{
				double totalHours = 0.0;
				// p�ros�t�s egyszer�s�tett logika: (0 with 1), (2 with 3) ...
				var arr = g.Select(x => x).ToList();
				for (int i = 0; i + 1 < arr.Count; i += 2)
				{
					var start = arr[i];
					var end = arr[i + 1];
					if (start.Type == AccessType.Entry && end.Type == AccessType.Exit)
					{
						totalHours += (end.TimestampUtc - start.TimestampUtc).TotalHours;
					}
					
				}

				return new WorkDaySummary
				{
					Day = g.Key,
					Hours = totalHours,
					Amount = 0m
				};
			})
			.ToList();

		// Amount v�ltoz� felt�lt�se az Employee �rab�r�b�l:
		var emp = await _db.Employees.FindAsync(employeeId);
		if (emp != null)
		{
			foreach (var d in daily)
				d.Amount = (decimal)d.Hours * emp.HourlyRate;
		}

		return daily;
	}
}
