namespace SystemMagazynu.Services
{
    public interface IChangeHistoryService
    {
        Task LogAsync(string tableName, int recordId, string operationType,
                      object? oldValue, object? newValue, string userId);
    }
}
