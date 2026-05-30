using System.Text.Json;
using SystemMagazynu.Data;
using SystemMagazynu.Models;

namespace SystemMagazynu.Services
{
    public class ChangeHistoryService : IChangeHistoryService
    {
        private readonly ApplicationDbContext _db;

        public ChangeHistoryService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task LogAsync(string tableName, int recordId, string operationType,
                                   object? oldValue, object? newValue, string userId)
        {
            var entry = new ChangeHistory
            {
                TableName = tableName,
                RecordId = recordId,
                OperationType = operationType,
                OldValue = oldValue != null ? JsonSerializer.Serialize(oldValue) : null,
                NewValue = newValue != null ? JsonSerializer.Serialize(newValue) : null,
                UserId = userId,
                ChangedAt = DateTime.UtcNow
            };

            _db.ChangeHistories.Add(entry);
            await _db.SaveChangesAsync();
        }
    }
}
