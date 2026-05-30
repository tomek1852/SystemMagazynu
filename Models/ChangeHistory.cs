namespace SystemMagazynu.Models
{
    public class ChangeHistory
    {
        public int Id { get; set; }
        public string TableName { get; set; } = string.Empty;
        public int RecordId { get; set; }
        public string OperationType { get; set; } = string.Empty;
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser? User { get; set; }
    }
}
