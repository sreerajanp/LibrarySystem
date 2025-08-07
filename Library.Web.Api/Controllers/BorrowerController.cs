using Library.Borrow;
using Microsoft.AspNetCore.Mvc;
using static Library.Borrow.BorrowService;

namespace Library.Web.Api.Controllers
{
    /// <summary>
    /// Handles borrowing-related operations such as finding related books and estimating reading rates.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class BorrowerController(BorrowServiceClient _borrowerService) : ControllerBase
    {
        /// <summary>
        /// Health Check endpoint to verify the API is running.
        /// </summary>
        [HttpGet("ping")]
        public IActionResult HealthCheck()
        {
            return Ok("pong");
        }

        /// <summary>
        /// What other books were borrowed by individuals who borrowed a particular book?
        /// </summary>
        /// <param name="bookId">The ID of the book to find related books for.</param>
        /// <returns>A list of related books if found; otherwise, a NotFound result.</returns>
        /// <response code="200">Returns the list of related books.</response>
        /// <response code="404">If no related books are found.</response>
        [HttpGet("related-books/{bookId}")]
        public async Task<IActionResult> GetRelatedBooks(int bookId)
        {
            var request = new BookIdRequest { BookId = bookId };
            var response = await _borrowerService.GetRelatedBooksAsync(request);

            if (response.RelatedBooks.Count == 0)
                return NotFound(response.Message);

            return Ok(response);
        }

        /// <summary>
        /// Estimate the reading rate (pages/day) for a book based on borrow and return times
        /// </summary>
        /// <param name="bookId">The ID of the book to estimate reading rate for.</param>
        /// <returns>Estimated reading duration and borrow statistics.</returns>
        /// <response code="200">Returns the estimated reading rate for the book.</response>
        /// <response code="404">If the book has no borrow records or estimation cannot be made.</response>
        [HttpGet("estimate-reading-rate/{bookId}")]
        public async Task<IActionResult> EstimateReadingRate(int bookId)
        {
            var request = new BookIdRequest { BookId = bookId };
            var response = await _borrowerService.EstimateReadingRateAsync(request);

            // If message is not empty and BorrowCount is 0, treat as NotFound or bad request
            if (!string.IsNullOrEmpty(response.Message) && response.BorrowCount == 0)
                return NotFound(response.Message);

            return Ok(response);
        }
    }

}
