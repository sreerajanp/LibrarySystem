using Library.User;
using Microsoft.AspNetCore.Mvc;

namespace Library.Web.Api.Controllers
{
    /// <summary>
    /// Provides user-related analytics such as most active borrowers and user borrowing history.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UserController(UserService.UserServiceClient grpcClient) : ControllerBase
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
        /// Which users have borrowed the most books within a given time frame?
        /// </summary>
        /// <param name="startDate">The start date of the borrowing period (UTC).</param>
        /// <param name="endDate">The end date of the borrowing period (UTC).</param>
        /// <param name="limit">The maximum number of users to return. Default is 5.</param>
        /// <returns>A list of users with the highest borrow count in the specified range.</returns>
        /// <response code="200">Returns the list of most active borrowers.</response>
        [HttpGet("most-borrowed-users")]
        public async Task<IActionResult> GetMostBorrowedUsers([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] int limit = 5)
        {
            var request = new UserBorrowRequest
            {
                StartDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.SpecifyKind(startDate, DateTimeKind.Utc)),
                EndDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.SpecifyKind(endDate, DateTimeKind.Utc)),
                Limit = limit
            };

            var response = await grpcClient.GetUsersWithMostBorrowsAsync(request);
            return Ok(response);
        }

        /// <summary>
        /// What books has a particular user borrowed during a specified period?
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="startDate">The start date of the borrowing period (UTC).</param>
        /// <param name="endDate">The end date of the borrowing period (UTC).</param>
        /// <returns>A list of books the user borrowed during the period.</returns>
        /// <response code="200">Returns the list of borrowed books for the user.</response>
        [HttpGet("user-borrowed-books/{userId}")]
        public async Task<IActionResult> GetBooksBorrowedByUser(int userId, [FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            var request = new UserBorrowedBooksRequest
            {
                UserId = userId,
                StartDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.SpecifyKind(startDate, DateTimeKind.Utc)),
                EndDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.SpecifyKind(endDate, DateTimeKind.Utc))
            };

            var response = await grpcClient.GetBooksBorrowedByUserAsync(request);
            return Ok(response);
        }
    }
}
