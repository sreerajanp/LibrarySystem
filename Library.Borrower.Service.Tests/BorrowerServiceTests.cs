using Grpc.Core;
using Library.Book.Service.Data.Context;
using Library.Borrow;
using Library.Borrower.Service.Services;
using Library.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

public class BorrowerServiceTests
{
    private LibraryContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<LibraryContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
            .Options;
        return new LibraryContext(options);
    }

    private ServerCallContext GetMockContext() => Mock.Of<ServerCallContext>();

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetRelatedBooks_ReturnsNoUsersMessage_WhenNoneBorrowed()
    {
        var context = GetInMemoryContext();
        context.SaveChanges();

        var service = new BorrowerServiceImpl(context);

        var result = await service.GetRelatedBooks(new BookIdRequest { BookId = 1 }, GetMockContext());

        Assert.Equal("No users have borrowed the specified book.", result.Message);
        Assert.Empty(result.RelatedBooks);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetRelatedBooks_ReturnsRelatedBooks()
    {
        var context = GetInMemoryContext();

        var book1 = new BookRecord { Id = 1, Title = "Book1", Author = "A", Pages = 100 };
        var book2 = new BookRecord { Id = 2, Title = "Book2", Author = "B", Pages = 200 };
        var user1 = new UserRecord { Id = 1, FullName = "User1", Email = "u1@mail.com", BorrowRecords = [] };
        var user2 = new UserRecord { Id = 2, FullName = "User2", Email = "u2@mail.com", BorrowRecords = [] };

        context.Books.AddRange(book1, book2);
        context.Users.AddRange(user1, user2);
        context.BorrowRecords.AddRange(
            new BorrowRecord { Id = 1, BookRecordId = 1, UserRecordId = 1, BookRecord = book1, UserRecord = user1, BorrowedAt = DateTimeOffset.Now },
            new BorrowRecord { Id = 2, BookRecordId = 2, UserRecordId = 1, BookRecord = book2, UserRecord = user1, BorrowedAt = DateTimeOffset.Now },
            new BorrowRecord { Id = 3, BookRecordId = 2, UserRecordId = 2, BookRecord = book2, UserRecord = user2, BorrowedAt = DateTimeOffset.Now }
        );
        context.SaveChanges();

        var service = new BorrowerServiceImpl(context);

        var result = await service.GetRelatedBooks(new BookIdRequest { BookId = 1 }, GetMockContext());

        Assert.Equal("Related books retrieved successfully.", result.Message);
        Assert.Single(result.RelatedBooks);
        Assert.Equal(2, result.RelatedBooks[0].BookId);
        Assert.Equal("Book2", result.RelatedBooks[0].Title);
        Assert.Equal("B", result.RelatedBooks[0].Author);
        Assert.Equal(1, result.RelatedBooks[0].CommonBorrowerCount);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task EstimateReadingRate_ReturnsBookNotFound()
    {
        var context = GetInMemoryContext();
        var service = new BorrowerServiceImpl(context);

        var result = await service.EstimateReadingRate(new BookIdRequest { BookId = 1 }, GetMockContext());

        Assert.Equal(1, result.BookId);
        Assert.Equal("Book not found.", result.Message);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task EstimateReadingRate_ReturnsPageCountNotAvailable()
    {
        var context = GetInMemoryContext();
        var book = new BookRecord { Id = 1, Title = "Book1", Author = "A", Pages = 0 };
        context.Books.Add(book);
        context.SaveChanges();

        var service = new BorrowerServiceImpl(context);

        var result = await service.EstimateReadingRate(new BookIdRequest { BookId = 1 }, GetMockContext());

        Assert.Equal(1, result.BookId);
        Assert.Equal("Page count not available for this book.", result.Message);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task EstimateReadingRate_ReturnsNoCompletedBorrows()
    {
        var context = GetInMemoryContext();
        var book = new BookRecord { Id = 1, Title = "Book1", Author = "A", Pages = 100 };
        context.Books.Add(book);
        context.SaveChanges();

        var service = new BorrowerServiceImpl(context);

        var result = await service.EstimateReadingRate(new BookIdRequest { BookId = 1 }, GetMockContext());

        Assert.Equal(1, result.BookId);
        Assert.Equal("No completed borrow records found for this book.", result.Message);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task EstimateReadingRate_ReturnsAveragePagesPerDay()
    {
        var context = GetInMemoryContext();
        var book = new BookRecord { Id = 1, Title = "Book1", Author = "A", Pages = 100 };
        context.Books.Add(book);

        context.BorrowRecords.AddRange(
            new BorrowRecord
            {
                Id = 1,
                BookRecordId = 1,
                UserRecordId = 1,
                BorrowedAt = DateTimeOffset.UtcNow.AddDays(-5),
                ReturnedAt = DateTimeOffset.UtcNow
            },
            new BorrowRecord
            {
                Id = 2,
                BookRecordId = 1,
                UserRecordId = 2,
                BorrowedAt = DateTimeOffset.UtcNow.AddDays(-10),
                ReturnedAt = DateTimeOffset.UtcNow.AddDays(-5)
            }
        );
        context.SaveChanges();

        var service = new BorrowerServiceImpl(context);

        var result = await service.EstimateReadingRate(new BookIdRequest { BookId = 1 }, GetMockContext());

        Assert.Equal(1, result.BookId);
        Assert.Equal("Book1", result.Title);
        Assert.Equal(2, result.BorrowCount);
        Assert.Equal("Reading rate estimated successfully.", result.Message);
        Assert.True(result.AveragePagesPerDay > 0);
    }
}