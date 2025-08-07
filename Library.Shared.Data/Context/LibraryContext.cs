using Library.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Library.Book.Service.Data.Context;

public class LibraryContext(DbContextOptions<LibraryContext> options) : DbContext(options)
{
    public DbSet<BookRecord> Books => Set<BookRecord>();
    public DbSet<UserRecord> Users => Set<UserRecord>();
    public DbSet<BorrowRecord> BorrowRecords => Set<BorrowRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BorrowRecord>()
            .HasOne(l => l.BookRecord)
            .WithMany(b => b.BorrowRecords)
            .HasForeignKey(l => l.BookRecordId);

        modelBuilder.Entity<BorrowRecord>()
            .HasOne(l => l.UserRecord)
            .WithMany(u => u.BorrowRecords)
            .HasForeignKey(l => l.UserRecordId);
    }
}

