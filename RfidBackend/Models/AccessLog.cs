using System;

public class AccessLog
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!; 
    public DateTime TimestampUtc { get; set; }
    public AccessType Type { get; set; }
    public string RawUid { get; set; } = null!;
}