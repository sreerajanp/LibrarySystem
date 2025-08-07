namespace Library.Shared.Entities;

public class BorrowRecord
{
    public int Id { get; set; }
    public int BookRecordId { get; set; }
    public BookRecord? BookRecord { get; set; }
    public int UserRecordId { get; set; }
    public UserRecord? UserRecord { get; set; }
    public DateTimeOffset BorrowedAt { get; set; }
    public DateTimeOffset? ReturnedAt { get; set; }
}
