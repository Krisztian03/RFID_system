public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;          
    public string RfidUid { get; set; } = null!;    
    public decimal HourlyRate { get; set; }            
    public string Role { get; set; } = "Worker";       // Worker / Admin / Manager

    public ICollection<AccessLog> AccessLogs { get; set; } = new List<AccessLog>();
}
