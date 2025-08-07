using Grpc.Core;
using Library.Book.Service.Data.Context;
using Library.Borrow;
using Microsoft.EntityFrameworkCore;
using static Library.Borrow.BorrowService;

namespace Library.Borrower.Service.Services
{
    public class BorrowerServiceImpl(LibraryContext _context) : BorrowServiceBase
    {
        public override async Task<RelatedBooksResponse> GetRelatedBooks(BookIdRequest request, ServerCallContext context)
        {
            var response = new RelatedBooksResponse();

            // Step 1: Get user IDs who borrowed the specified book
            var userIds = await _context.BorrowRecords
                .Where(r => r.BookRecordId == request.BookId)
                .Select(r => r.UserRecordId)
                .Distinct()
                .ToListAsync();

            if (userIds.Count == 0)
            {
                response.Message = "No users have borrowed the specified book.";
                return response;
            }

            // Step 2: Find other books borrowed by these users (excluding the original book)
            var relatedBooks = await _context.BorrowRecords
                .Where(r => userIds.Contains(r.UserRecordId) && r.BookRecordId != request.BookId)
                .Include(r => r.BookRecord)
                .ToListAsync();

            // Step 3: Group and count how many users borrowed each related book
            var grouped = relatedBooks
                .GroupBy(r => r.BookRecordId)
                .Select(g => new
                {
                    Book = g.First().BookRecord,
                    Count = g.Select(x => x.UserRecordId).Distinct().Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            foreach (var item in grouped)
            {
                if (item.Book == null) continue;

                response.RelatedBooks.Add(new RelatedBook
                {
                    BookId = item.Book.Id,
                    Title = item.Book.Title,
                    Author = item.Book.Author,
                    CommonBorrowerCount = item.Count
                });
            }

            response.Message = "Related books retrieved successfully.";
            return response;
        }

        public override async Task<ReadingRateResponse> EstimateReadingRate(BookIdRequest request, ServerCallContext context)
        {
            var book = await _context.Books
                .FirstOrDefaultAsync(b => b.Id == request.BookId);

            if (book == null)
            {
                return new ReadingRateResponse
                {
                    BookId = request.BookId,
                    Message = "Book not found."
                };
            }

            if (book.Pages <= 0)
            {
                return new ReadingRateResponse
                {
                    BookId = book.Id,
                    Title = book.Title,
                    Message = "Page count not available for this book."
                };
            }

            // Get completed borrow records (those that have been returned)
            var completedBorrows = await _context.BorrowRecords
                .Where(r => r.BookRecordId == request.BookId && r.ReturnedAt.HasValue)
                .ToListAsync();

            if (!completedBorrows.Any())
            {
                return new ReadingRateResponse
                {
                    BookId = book.Id,
                    Title = book.Title,
                    Message = "No completed borrow records found for this book."
                };
            }

            double totalDays = 0;
            int count = 0;

            foreach (var record in completedBorrows)
            {
                if (!record.ReturnedAt.HasValue) { continue; }
                var duration = (record.ReturnedAt.Value - record.BorrowedAt).TotalDays;
                if (duration > 0)
                {
                    totalDays += duration;
                    count++;
                }
            }

            if (count == 0 || totalDays == 0)
            {
                return new ReadingRateResponse
                {
                    BookId = book.Id,
                    Title = book.Title,
                    Message = "Could not estimate reading rate due to insufficient data."
                };
            }

            double avgDaysPerBorrow = totalDays / count;
            double avgPagesPerDay = book.Pages / avgDaysPerBorrow;

            return new ReadingRateResponse
            {
                BookId = book.Id,
                Title = book.Title,
                AveragePagesPerDay = Math.Round(avgPagesPerDay, 2),
                BorrowCount = count,
                Message = "Reading rate estimated successfully."
            };
        }
    }
}
