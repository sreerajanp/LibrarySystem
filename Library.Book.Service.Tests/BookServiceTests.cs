using Grpc.Core;
using Library.Book;
using Library.Book.Service.Data.Context;
using Library.Book.Service.Services;
using Library.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;

public class BookServiceTests
{
    private LibraryContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<LibraryContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
            .Options;
        var context = new LibraryContext(options);
        return context;
    }

    private ServerCallContext GetMockContext() => Mock.Of<ServerCallContext>();

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetMostBorrowedBooks_ReturnsTopBooks()
    {
        // Arrange
        var context = GetInMemoryContext();
        var book1 = new BookRecord { Id = 1, Title = "Book1", Author = "Author1", TotalCopies = 5 };
        var book2 = new BookRecord { Id = 2, Title = "Book2", Author = "Author2", TotalCopies = 3 };
        context.Books.AddRange(book1, book2);
        context.BorrowRecords.AddRange(
            new BorrowRecord { BookRecordId = 1, BookRecord = book1 },
            new BorrowRecord { BookRecordId = 1, BookRecord = book1 },
            new BorrowRecord { BookRecordId = 2, BookRecord = book2 }
        );
        context.SaveChanges();

        var service = new BookServiceImpl(context);

        // Act
        var result = await service.GetMostBorrowedBooks(new TopNRequest { Limit = 2 }, GetMockContext());

        // Assert
        Assert.Equal("Top borrowed books retrieved successfully.", result.Message);
        Assert.Equal(2, result.BookDetails.Count);
        Assert.Equal(1, result.BookDetails[0].Id); // Book1 has more borrows
        Assert.Equal(2, result.BookDetails[1].Id);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetBookAvailability_Throws_WhenTotalCopiesIsZero()
    {
        var context = GetInMemoryContext();

        var book1 = new BookRecord { Id = 3, Title = "Book1", Author = "Author1", TotalCopies = -3 };
        var book2 = new BookRecord { Id = 2, Title = "Book2", Author = "Author2", TotalCopies = 3 };
        context.Books.AddRange(book1, book2);

        await context.SaveChangesAsync();
        var service = new BookServiceImpl(context);
        var request = new IdRequest { Id = 3 }; // Book with TotalCopies = 0

        var ex = await Assert.ThrowsAsync<RpcException>(() => service.GetBookAvailability(request, GetMockContext()));
        Assert.Equal(StatusCode.FailedPrecondition, ex.StatusCode);
        Assert.Contains("no available copies", ex.Status.Detail);
    }

    [Fact]
    public async Task GetBookAvailability_Throws_WhenBorrowedExceedsTotalCopies()
    {
        var context = GetInMemoryContext();

        var book1 = new BookRecord { Id = 1, Title = "Book1", Author = "Author1", TotalCopies = 3 };
        var book2 = new BookRecord { Id = 2, Title = "Book2", Author = "Author2", TotalCopies = 3 };
        context.Books.AddRange(book1, book2);

        // Add inconsistent borrow records exceeding total copies for Book Id = 1 (TotalCopies = 5)
        context.BorrowRecords.AddRange(
            new BorrowRecord { Id = 6, BookRecordId = 1, BorrowedAt = DateTime.UtcNow },
            new BorrowRecord { Id = 7, BookRecordId = 1, BorrowedAt = DateTime.UtcNow },
            new BorrowRecord { Id = 8, BookRecordId = 1, BorrowedAt = DateTime.UtcNow },
            new BorrowRecord { Id = 9, BookRecordId = 1, BorrowedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = new BookServiceImpl(context);
        var request = new IdRequest { Id = 1 };

        var ex = await Assert.ThrowsAsync<RpcException>(() => service.GetBookAvailability(request, GetMockContext()));
        Assert.Equal(StatusCode.Internal, ex.StatusCode);
        Assert.Contains("exceed total copies", ex.Status.Detail);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetMostBorrowedBooks_ReturnsNoBorrowDataMessage_WhenNoneExist()
    {
        var context = GetInMemoryContext();
        var service = new BookServiceImpl(context);

        var book1 = new BookRecord { Id = 1, Title = "Book1", Author = "Author1", TotalCopies = 3 };
        var book2 = new BookRecord { Id = 2, Title = "Book2", Author = "Author2", TotalCopies = 3 };
        context.Books.AddRange(book1, book2);
        await context.SaveChangesAsync();


        var result = await service.GetMostBorrowedBooks(new TopNRequest { Limit = 5 }, GetMockContext());

        Assert.Equal("No borrow data available.", result.Message);
        Assert.Empty(result.BookDetails);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetMostBorrowedBooks_ReturnsNoBooksAvailableMessage_WhenNoneExist()
    {
        var context = GetInMemoryContext();
        var service = new BookServiceImpl(context);
        var request = new TopNRequest { Limit = 5 };

        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            service.GetMostBorrowedBooks(request, GetMockContext()));

        Assert.Equal(StatusCode.FailedPrecondition, ex.StatusCode);
        Assert.Equal("No books available in the system.", ex.Status.Detail);
    }

    [Fact]
    public async Task GetMostBorrowedBooks_Throws_WhenLimitIsLessThanEqualToZero()
    {
        // Arrange
        var context = GetInMemoryContext();
        var service = new BookServiceImpl(context);
        var request = new TopNRequest { Limit = 0 };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            service.GetMostBorrowedBooks(request, GetMockContext()));

        Assert.Equal(StatusCode.InvalidArgument, ex.StatusCode);
        Assert.Equal("Limit must be greater than zero.", ex.Status.Detail);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetBookAvailability_Throws_WhenIdIsZero()
    {
        // Arrange
        var context = GetInMemoryContext();
        var service = new BookServiceImpl(context);
        var request = new IdRequest { Id = 0 };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            service.GetBookAvailability(request, GetMockContext()));

        Assert.Equal(StatusCode.InvalidArgument, ex.StatusCode);
        Assert.Equal("Book ID must be greater than zero.", ex.Status.Detail);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetBookAvailability_Throws_WhenIdIsNegative()
    {
        // Arrange
        var context = GetInMemoryContext();
        var service = new BookServiceImpl(context);
        var request = new IdRequest { Id = -1 };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            service.GetBookAvailability(request, GetMockContext()));

        Assert.Equal(StatusCode.InvalidArgument, ex.StatusCode);
        Assert.Equal("Book ID must be greater than zero.", ex.Status.Detail);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetBookAvailability_ReturnsCorrectAvailability()
    {
        var context = GetInMemoryContext();
        var book = new BookRecord { Id = 1, Title = "Book1", Author = "Author1", TotalCopies = 3 };
        context.Books.Add(book);
        context.BorrowRecords.AddRange(
            new BorrowRecord { BookRecordId = 1, BookRecord = book, ReturnedAt = null },
            new BorrowRecord { BookRecordId = 1, BookRecord = book, ReturnedAt = System.DateTimeOffset.UtcNow }
        );
        context.SaveChanges();

        var service = new BookServiceImpl(context);

        var result = await service.GetBookAvailability(new IdRequest { Id = 1 }, GetMockContext());

        Assert.Equal(1, result.BorrowedCopies);
        Assert.Equal(2, result.AvailableCopies);
        Assert.Equal("Book availability retrieved successfully.", result.Message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetBookAvailability_ThrowsNotFound_WhenBookDoesNotExist()
    {
        var context = GetInMemoryContext();
        var service = new BookServiceImpl(context);

        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            service.GetBookAvailability(new IdRequest { Id = 99 }, GetMockContext()));

        Assert.Equal(StatusCode.NotFound, ex.StatusCode);
        Assert.Contains("Book with ID 99 not found", ex.Status.Detail);
    }
}