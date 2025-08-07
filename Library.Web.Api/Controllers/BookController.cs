using Library.Book;
using Microsoft.AspNetCore.Mvc;

namespace Library.Web.Api.Controllers
{
    /// <summary>
    /// Provides endpoints to retrieve book data and availability.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BookController : ControllerBase
    {
        private readonly BookService.BookServiceClient _grpcClient;

        public BookController(BookService.BookServiceClient grpcClient)
        {
            _grpcClient = grpcClient;
        }

        /// <summary>
        /// Health Check endpoint to verify the API is running.
        /// </summary>
        [HttpGet("ping")]
        public IActionResult HealthCheck()
        {
            return Ok("pong");
        }

        /// <summary>
        /// What are the most borrowed books?
        /// </summary>
        /// <param name="limit">The number of books to return (e.g., top 5, 10, etc.).</param>
        /// <returns>A list of the most borrowed books.</returns>
        /// <response code="200">Returns the list of most borrowed books.</response>
        [HttpGet("most-borrowed/{limit}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMostBorrowedBooks(int limit)
        {
            var request = new TopNRequest { Limit = limit };
            var response = await _grpcClient.GetMostBorrowedBooksAsync(request);

            return Ok(response);
        }

        /// <summary>
        /// How many copies of a specific book are borrowed vs available?.
        /// </summary>
        /// <param name="id">The ID of the book to check availability for.</param>
        /// <returns>Availability details including total borrowed and available copies.</returns>
        /// <response code="200">Returns the availability information for the book.</response>
        [HttpGet("availability/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetBookAvailability(int id)
        {
            var request = new IdRequest { Id = id };
            var response = await _grpcClient.GetBookAvailabilityAsync(request);

            return Ok(response);
        }
    }
}
