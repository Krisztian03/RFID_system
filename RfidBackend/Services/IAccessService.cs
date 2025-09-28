public interface IAccessService
{
    Task<AccessResult?> ProcessUidAsync(string uid);
    Task<IEnumerable<WorkDaySummary>> GetEmployeeSummaryAsync(int employeeId, DateTime fromUtc, DateTime toUtc);
}
