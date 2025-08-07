using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Library.Shared.Data.Migrations
{
    /// <inheritdoc />
    public partial class seed_sample_data : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Insert 10 books
            migrationBuilder.InsertData(
                table: "Books",
                columns: ["Id", "Title", "Author", "Pages", "TotalCopies"],
                values: new object[,]
                {
                    { 1, "C# in Depth", "Jon Skeet", 528, 5 },
                    { 2, "ASP.NET Core in Action", "Andrew Lock", 420, 4 },
                    { 3, "gRPC: Up and Running", "Kasun Indrasiri", 300, 3 },
                    { 4, "Entity Framework Core in Action", "Jon P Smith", 370, 4 },
                    { 5, "Clean Code", "Robert C. Martin", 464, 6 },
                    { 6, "Microservices Patterns", "Chris Richardson", 280, 3 },
                    { 7, "xUnit Test Patterns", "Gerard Meszaros", 500, 2 },
                    { 8, "Swagger and OpenAPI", "Josh Ponelat", 200, 3 },
                    { 9, "Building Microservices with .NET Core", "Gaurav Aroraa", 350, 4 },
                    { 10, "The Pragmatic Programmer", "Andrew Hunt & David Thomas", 352, 5 }
                });

            // Insert 10 users
            migrationBuilder.InsertData(
                table: "Users",
                columns: ["Id", "FullName", "Email"],
                values: new object[,]
                {
                    { 1, "Alice Johnson", "alice@example.com" },
                    { 2, "Bob Smith", "bob@example.com" },
                    { 3, "Charlie Brown", "charlie@example.com" },
                    { 4, "Diana Prince", "diana@example.com" },
                    { 5, "Ethan Hunt", "ethan@example.com" },
                    { 6, "Fiona Glenanne", "fiona@example.com" },
                    { 7, "George Costanza", "george@example.com" },
                    { 8, "Hannah Baker", "hannah@example.com" },
                    { 9, "Ian Fleming", "ian@example.com" },
                    { 10, "Julia Roberts", "julia@example.com" }
                });

            // Insert 15 borrow records (some with ReturnedAt = null to simulate current borrows)
            migrationBuilder.InsertData(
                table: "BorrowRecords",
                columns: ["Id", "BookRecordId", "UserRecordId", "BorrowedAt", "ReturnedAt"],
                values: new object[,]
                {
                    { 1, 1, 1, DateTime.Now.AddDays(-30), DateTime.Now.AddDays(-5) },
                    { 2, 1, 2, DateTime.Now.AddDays(-5), null },
                    { 3, 2, 3,  DateTime.Now.AddDays(-35), DateTime.Now.AddDays(-2) },
                    { 4, 3, 4,  DateTime.Now.AddDays(-20), DateTime.Now.AddDays(-8) },
                    { 5, 4, 5, DateTime.Now.AddDays(-10), null },
                    { 6, 5, 6,  DateTime.Now.AddDays(-50), DateTime.Now.AddDays(-15) },
                    { 7, 6, 7, DateTime.Now.AddDays(-30), null },
                    { 8, 7, 8,  DateTime.Now.AddDays(-40), DateTime.Now.AddDays(-8) },
                    { 9, 8, 9, DateTime.Now.AddDays(-30), DateTime.Now.AddDays(-5) },
                    { 10, 9, 10, DateTime.Now.AddDays(-20), null },
                    { 11, 10, 1, DateTime.Now.AddDays(-80), DateTime.Now.AddDays(-10) },
                    { 12, 3, 2,  DateTime.Now.AddDays(-40), DateTime.Now.AddDays(-20) },
                    { 13, 4, 3, DateTime.Now.AddDays(-35), null },
                    { 14, 2, 4, DateTime.Now.AddDays(-30), DateTime.Now.AddDays(-5) },
                    { 15, 5, 5, DateTime.Now.AddDays(-3), null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Delete BorrowRecords first because of FK constraints
            for (int id = 1; id <= 15; id++)
            {
                migrationBuilder.DeleteData("BorrowRecords", "Id", id);
            }

            // Delete Users
            for (int id = 1; id <= 10; id++)
            {
                migrationBuilder.DeleteData("Users", "Id", id);
            }

            // Delete Books
            for (int id = 1; id <= 10; id++)
            {
                migrationBuilder.DeleteData("Books", "Id", id);
            }
        }
    }
}