using Grpc.Core;
using Library.Book.Service.Data.Context;
using Library.Shared.Entities;
using Library.User;
using Library.User.Service.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

public class UserServiceTests
{
    private LibraryContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<LibraryContext>()
            .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
            .Options;
        return new LibraryContext(options);
    }

    private ServerCallContext GetMockContext() => Moq.Mock.Of<ServerCallContext>();

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetUsersWithMostBorrows_ReturnsNoRecordsMessage_WhenNoneExist()
    {
        var context = GetInMemoryContext();
        context.SaveChanges();

        var service = new UserServiceImpl(context);

        var req = new UserBorrowRequest
        {
            StartDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-10).ToUniversalTime()),
            EndDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow.ToUniversalTime()),
            Limit = 3
        };

        var result = await service.GetUsersWithMostBorrows(req, GetMockContext());

        Assert.Equal("No borrow records found for the specified date range.", result.Message);
        Assert.Empty(result.UserBorrowDetails);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetUsersWithMostBorrows_ReturnsTopUsers()
    {
        var context = GetInMemoryContext();

        var user1 = new UserRecord { Id = 1, FullName = "User1", Email = "u1@mail.com", BorrowRecords = new List<BorrowRecord>() };
        var user2 = new UserRecord { Id = 2, FullName = "User2", Email = "u2@mail.com", BorrowRecords = new List<BorrowRecord>() };
        var book = new BookRecord { Id = 1, Title = "Book1", Author = "A", Pages = 100 };

        context.Users.AddRange(user1, user2);
        context.Books.Add(book);

        var now = DateTimeOffset.UtcNow;
        context.BorrowRecords.AddRange(
            new BorrowRecord { Id = 1, BookRecordId = 1, UserRecordId = 1, BookRecord = book, UserRecord = user1, BorrowedAt = now.AddDays(-2) },
            new BorrowRecord { Id = 2, BookRecordId = 1, UserRecordId = 1, BookRecord = book, UserRecord = user1, BorrowedAt = now.AddDays(-1) },
            new BorrowRecord { Id = 3, BookRecordId = 1, UserRecordId = 2, BookRecord = book, UserRecord = user2, BorrowedAt = now }
        );
        context.SaveChanges();

        var service = new UserServiceImpl(context);

        var req = new UserBorrowRequest
        {
            StartDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(now.AddDays(-5).UtcDateTime),
            EndDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(now.UtcDateTime),
            Limit = 2
        };

        var result = await service.GetUsersWithMostBorrows(req, GetMockContext());

        Assert.Equal("Users with most borrows retrieved successfully.", result.Message);
        Assert.Equal(2, result.UserBorrowDetails.Count);
        Assert.Equal(1, result.UserBorrowDetails[0].UserId); // User1 has more borrows
        Assert.Equal(2, result.UserBorrowDetails[1].UserId);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetBooksBorrowedByUser_ReturnsUserDoesNotExist()
    {
        var context = GetInMemoryContext();
        var service = new UserServiceImpl(context);

        var req = new UserBorrowedBooksRequest
        {
            UserId = 99,
            StartDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-10).ToUniversalTime()),
            EndDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow.ToUniversalTime())
        };

        var result = await service.GetBooksBorrowedByUser(req, GetMockContext());

        Assert.Contains("User does not exist", result.Message);
        Assert.Empty(result.UserBookDetail);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetBooksBorrowedByUser_ReturnsNoBooksForUser()
    {
        var context = GetInMemoryContext();
        var user = new UserRecord { Id = 1, FullName = "User1", Email = "u1@mail.com", BorrowRecords = new List<BorrowRecord>() };
        context.Users.Add(user);
        context.SaveChanges();

        var service = new UserServiceImpl(context);

        var req = new UserBorrowedBooksRequest
        {
            UserId = 1,
            StartDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow.AddDays(-10).ToUniversalTime()),
            EndDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow.ToUniversalTime())
        };

        var result = await service.GetBooksBorrowedByUser(req, GetMockContext());

        Assert.Equal("No books found for the specified user and date range.", result.Message);
        Assert.Empty(result.UserBookDetail);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task GetBooksBorrowedByUser_ReturnsBooks()
    {
        var context = GetInMemoryContext();
        var user = new UserRecord { Id = 1, FullName = "User1", Email = "u1@mail.com", BorrowRecords = [] };
        var book = new BookRecord { Id = 1, Title = "Book1", Author = "A", Pages = 100 };
        context.Users.Add(user);
        context.Books.Add(book);

        var now = DateTimeOffset.UtcNow;
        context.BorrowRecords.Add(
            new BorrowRecord
            {
                Id = 1,
                BookRecordId = 1,
                UserRecordId = 1,
                BookRecord = book,
                UserRecord = user,
                BorrowedAt = now
            }
        );
        context.SaveChanges();

        var service = new UserServiceImpl(context);

        var req = new UserBorrowedBooksRequest
        {
            UserId = 1,
            StartDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(now.AddDays(-1).UtcDateTime),
            EndDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(now.AddDays(1).UtcDateTime)
        };

        var result = await service.GetBooksBorrowedByUser(req, GetMockContext());

        Assert.Equal("Books retrieved successfully.", result.Message);
        Assert.Single(result.UserBookDetail);
        Assert.Equal(1, result.UserBookDetail[0].BookId);
        Assert.Equal("Book1", result.UserBookDetail[0].BookTitle);
        Assert.Equal("A", result.UserBookDetail[0].BookAuthor);
        Assert.Equal("User1", result.UserBookDetail[0].UserName);
    }
}