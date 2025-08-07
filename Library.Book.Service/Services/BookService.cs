using Grpc.Core;
using Library.Book.Service.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Net;
using static Library.Book.BookService;

namespace Library.Book.Service.Services
{
    public class BookServiceImpl(LibraryContext _context) : BookServiceBase
    {
        public override async Task<BooksResponse> GetMostBorrowedBooks(TopNRequest request, ServerCallContext context)
        {
            // Validation: Check if the limit is greater than zero
            if (request.Limit <= 0)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Limit must be greater than zero."));
            }

            // Validation: Check if any books exist in the system at all
            var totalBooks = await _context.Books.CountAsync();
            if (totalBooks == 0)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, "No books available in the system."));
            }

            var topBorrowedBooks = await _context.BorrowRecords
                .Include(b => b.BookRecord)
                .Where(b => b.BookRecord != null)
                .GroupBy(b => b.BookRecordId)
                .OrderByDescending(g => g.Count())
                .Take(request.Limit)
                .Select(g => new
                {
                    Book = g.First().BookRecord,
                    TotalBorrows = g.Count()
                })
                .ToListAsync();

            var response = new BooksResponse();

            // Validation: No borrow records available for any books
            if (topBorrowedBooks.Count == 0)
            {
                response.Message = "No borrow data available.";
                return response;
            }

            foreach (var book in topBorrowedBooks)
            {
                if (book.Book == null) { continue; }
                response.BookDetails.Add(new BookDetail
                {
                    Id = book.Book.Id,
                    Title = book.Book.Title,
                    Author = book.Book.Author,
                    TotalBorrows = book.TotalBorrows
                });
            }
            response.Message = "Top borrowed books retrieved successfully.";
            return response;
        }

        public override async Task<BookAvailabilityResponse> GetBookAvailability(IdRequest request, ServerCallContext context)
        {
            if (request.Id <= 0)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Book ID must be greater than zero."));
            }

            var book = await _context.Books.FindAsync(request.Id) ?? throw new RpcException(new Status(StatusCode.NotFound, $"Book with ID {request.Id} not found."));
            if (book.TotalCopies <= 0)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, $"Book with ID {request.Id} has no available copies configured."));
            }

            var borrowedCount = await _context.BorrowRecords.CountAsync(r => r.BookRecordId == request.Id && r.ReturnedAt == null);

            if (borrowedCount > book.TotalCopies)
            {
                // Log this as a potential data inconsistency
                throw new RpcException(new Status(StatusCode.Internal, $"Inconsistent data: borrowed copies ({borrowedCount}) exceed total copies ({book.TotalCopies}) for book ID {request.Id}."));
            }

            var availableCopies = book.TotalCopies - borrowedCount;

            return new BookAvailabilityResponse
            {
                Id = book.Id,
                BorrowedCopies = borrowedCount,
                AvailableCopies = availableCopies,
                Message = "Book availability retrieved successfully."
            };
        }
    }
}
