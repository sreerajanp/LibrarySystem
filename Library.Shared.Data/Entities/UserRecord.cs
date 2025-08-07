namespace Library.Shared.Entities;
public class UserRecord
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";

    public required ICollection<BorrowRecord> BorrowRecords { get; set; }
}

