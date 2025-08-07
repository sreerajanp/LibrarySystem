using Grpc.Core;
using Library.Book.Service;
using Library.Book.Service.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Library.User.Service.Services
{
    public class UserServiceImpl(LibraryContext _context) : UserService.UserServiceBase
    {
        public override async Task<UserBorrowResponse> GetUsersWithMostBorrows(UserBorrowRequest request, ServerCallContext context)
        {
            var response = new UserBorrowResponse();

            var startDate = request.StartDate.ToDateTime();
            var endDate = request.EndDate.ToDateTime();

            var borrowedRecords = await _context.BorrowRecords
                .Include(b => b.BookRecord)
                .Include(b => b.UserRecord).ToListAsync();

            var borrowedRecordFilter = borrowedRecords.Where(r => r.BorrowedAt >= startDate && r.BorrowedAt <= endDate).ToList();

            var borrowDetails = borrowedRecordFilter.GroupBy(b => b.UserRecordId)
                                .OrderByDescending(g => g.Count())
                                .Take(request.Limit).ToList();
            if (borrowDetails.Count == 0)
            {
                response.Message = "No borrow records found for the specified date range.";
                return response;
            }

            foreach (var borrowDetail in borrowDetails)
            {
                var userRecord = borrowDetail.FirstOrDefault()?.UserRecord;

                if (userRecord == null) { continue; }

                response.UserBorrowDetails.Add(new UserBorrowDetails()
                {
                    UserId = userRecord.Id,
                    UserName = userRecord.FullName,
                    BorrowCount = borrowDetail.Count()
                });
            }
            response.Message = "Users with most borrows retrieved successfully.";

            return response;
        }

        public override async Task<UserBorrowedBooksResponse> GetBooksBorrowedByUser(UserBorrowedBooksRequest request, ServerCallContext context)
        {
            var userRecord = await _context.Users.Where(r => r.Id == request.UserId).FirstOrDefaultAsync();
            var response = new UserBorrowedBooksResponse();
            if (userRecord == null)
            {
                response.Message = $"User does not exist.Invalid UserId :{request.UserId}";
                return response;
            }

            var startDate = request.StartDate.ToDateTime();
            var endDate = request.EndDate.ToDateTime();

            var borrowedRecords = await _context.BorrowRecords
                .Include(b => b.BookRecord)
                .Include(b => b.UserRecord).ToListAsync();


            var borrowedRecordFilter = borrowedRecords
                .Where(r => r.UserRecordId == request.UserId
                    && r.BorrowedAt >= request.StartDate.ToDateTime()
                    && r.BorrowedAt <= request.EndDate.ToDateTime())
                .Select(r => r.BookRecord)
                .Distinct()
                .ToList();

            if (borrowedRecordFilter.Count == 0)
            {
                response.Message = "No books found for the specified user and date range.";
                return response;
            }
            foreach (var bookRecord in borrowedRecordFilter)
            {
                if (bookRecord == null) { continue; }

                response.UserBookDetail.Add(new UserBookDetail()
                {
                    BookAuthor = bookRecord.Author,
                    BookId = bookRecord.Id,
                    BookTitle = bookRecord.Title,
                    UserName = userRecord.FullName
                });
            }

            response.Message = "Books retrieved successfully.";
            return response;
        }

    }
}
