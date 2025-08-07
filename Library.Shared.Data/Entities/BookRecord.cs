namespace Library.Shared.Entities;

public class BookRecord
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Author { get; set; } = "";
    public int Pages { get; set; }
    public int TotalCopies { get; set; }

    public ICollection<BorrowRecord>? BorrowRecords { get; set; }
}

