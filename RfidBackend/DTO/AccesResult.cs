public class AccessResult
{
	public int EmployeeId { get; set; }
	public string EmployeeName { get; set; } = null!;
	public AccessType Action { get; set; }
	public DateTime TimestampUtc { get; set; }
}
