# Library Management System

   Library System is a microservice-based app that helps library staff manage books, borrowers, and loans easily. The services talk to each other using gRPC for fast and efficient communication, while WebAPI endpoints let users access the system. This setup helps to support smart library decisions.

## Architecture Diagram

```
    +----------------+       +---------------------+       +------------------+       +-------------------+
    |  Web API Layer |  -->  |  gRPC Service Layer |  -->  | Shared           |  -->  | Refer             |
    | (Book, User,   |       | (BookService,       |       | Database SQLite  |       | Project Structure |
    |  Borrow APIs)  |       |  UserService,       |       | proto files      |       | for more details  |
    |                |       |  BorrowService)     |       |                  |       |                   |
    +----------------+       +---------------------+       +------------------+       +-------------------+

 Flow Description

    1. API Layer – Handles HTTP requests from clients and converts them into gRPC calls.
    2. gRPC Service Layer – Contains core business logic, processes requests, and queries the database.
    3. Shared Database – SQLite database used by all services for consistent data access.

```
## Technologies Used
    ✔️ C# and .NET 9: Primary language and framework for all projects.  
    ✔️ gRPC (Grpc.AspNetCore): For internal microservice communication.  
    ✔️ Entity Framework Core 9 (Microsoft.EntityFrameworkCore, Microsoft.EntityFrameworkCore.Sqlite, Microsoft.EntityFrameworkCore.Design): ORM for data access and migrations.  
    ✔️ SQLite: Database engine for persistent storage.  
    ✔️ xUnit (xunit, xunit.runner.visualstudio): Unit and integration testing framework.  
    ✔️ Moq: Mocking library for tests.  
    ✔️ coverlet.collector: Code coverage tool for tests.  
    ✔️ Microsoft.NET.Test.Sdk: Test runner for .NET projects.  
    ✔️ Swagger: For API documentation (implied by documentation, check for Swashbuckle or similar in API project).  
    ✔️ HttpClient: For HTTP interactions.  
    ✔️ Postman: For API testing (external tool).  
    ✔️ Microservice Architecture: Modular design with separate services for Book, User, and Borrower.  
 
 ## Assumptions
    - The system is designed to be modular and scalable, allowing for easy addition of new features or services.
    - Each service is independent and can be deployed separately.
    - The database schema is designed to support the core functionalities of the library system.
    - The API endpoints are designed to be intuitive and follow RESTful principles.
    - The division between endpoints and gRPC services was made based on our own design choices to ensure a clear separation of responsibilities.
    - The logic and calculations rely on certain assumptions, like estimating reading speed using the borrow and return timestamps.

 ## Get the code from GitHub

    👉 Clone or download the project repository.
    👉 Open the solution file `LibrarySystem.sln` using **Visual Studio 2022**.
    👉 Ensure you have the required .NET SDK installed (version 9.0 or higher).

  ## How to run the project

    👉 Build the solution and explore the project structure and source code.
    👉 Set up the multiple startup project in solution properties to `Library.Book.Service, Library.User.Service, Library.Borrower.Service, Library.Web.Api`.
    👉 Run the solution to start the gRPC services and API.
    👉 Use Swagger/Postman/HTTP client(Library.Web.Api.http) to interact with the API endpoints.
    👉 Optionally, you can run the included test projects to validate the functionality.
    👉 For database migrations, use the provided EF Core commands to create and seed the database.
    👉 To run the tests, use the provided commands to execute unit and integration tests.
    👉 Refer to the architecture diagram and project structure for a better understanding of the system.
    👉 Run the included test projects for validation.

   ## How to start the service using Command Line

        ```
        gRPC Service: Navigate to respective project path Library.Book.Service, Library.User.Service, Library.Borrower.Service and run the following commands:
            dotnet clean
            dotnet build
            dotnet run

        Web API: Navigate to respective project path Library.Web.API and run the following commands:

            cd Library.Web.API
            dotnet clean
            dotnet build
            dotnet run
        ```

## Test the API using HttpClient

- Open the **`Library.Web.API`** project in Visual Studio.
- Locate and open the `Library.Web.Api.http` file.
- Send HTTP requests to test the endpoints using the built-in HTTP file support or use Postman.

    <details>
      <summary>Book Health check</summary>
         '''

         Request: GET {{Library.API_HostAddress}}/api/book/ping
         Response:"pong"
    </details>
    <details>
      <summary>What are the most borrowed books?</summary>

       ```
         Request: {{Library.Web.Api_HostAddress}}/api/Book/most-borrowed/2/
         Response:
            {
              "message": "Top borrowed books retrieved successfully.",
              "bookDetails": [
                {
                  "id": 1,
                  "title": "C# in Depth",
                  "author": "Jon Skeet",
                  "totalBorrows": 2
                },
                {
                  "id": 2,
                  "title": "ASP.NET Core in Action",
                  "author": "Andrew Lock",
                  "totalBorrows": 2
                }
              ]
            }
      ```
    </details>
    <details>
      <summary>How many copies of a specific book are borrowed vs available?</summary>

       ```
         Request: {{Library.Web.Api_HostAddress}}/api/Book/availability/2/
         Response:
            {
              "id": 2,
              "borrowedCopies": 0,
              "availableCopies": 4,
              "message": "Book availability retrieved successfully."
            }
      ```
    </details>
    <details>
      <summary>Borrower Health check</summary>
         '''
 
         Request: GET {{Library.Web.Api_HostAddress}}/api/Borrower/ping/
         Response:"pong"
    </details>
    <details>
      <summary>What other books were borrowed by individuals who borrowed a particular book?</summary>

       ```
         Request: GET {{Library.Web.Api_HostAddress}}/api/Borrower/related-books/2/
         Response:
            {
              "message": "Related books retrieved successfully.",
              "relatedBooks": [
                {
                  "bookId": 4,
                  "title": "Entity Framework Core in Action",
                  "author": "Jon P Smith",
                  "commonBorrowerCount": 1
                },
                {
                  "bookId": 3,
                  "title": "gRPC: Up and Running",
                  "author": "Kasun Indrasiri",
                  "commonBorrowerCount": 1
                }
              ]
            }
      ```
    </details>
    <details>
      <summary>Estimate the reading rate (pages/day) for a book based on borrow and return times</summary>

       ```
         Request: {{Library.Web.Api_HostAddress}}/api/Borrower/estimate-reading-rate/2/
         Response:
            {
              "bookId": 2,
              "title": "ASP.NET Core in Action",
              "averagePagesPerDay": 14.48,
              "borrowCount": 2,
              "message": "Reading rate estimated successfully."
            }
      ```
    </details>
    <details>
      <summary>User Health check</summary>
         '''
 
         Request: GET {{Library.Web.Api_HostAddress}}/api/User/ping/
         Response:"pong"
    </details>
    <details>
      <summary>Which users have borrowed the most books within a given time frame?</summary>

       ```
         Request: GET {{Library.Web.Api_HostAddress}}/api/User/most-borrowed-users?startDate=2025-07-08%2014%3A28%3A19.1638368%2B02%3A00&endDate=2025-08-08%2014%3A28%3A19.1638368%2B02%3A00&limit=2
         Response:
            {
              "message": "Users with most borrows retrieved successfully.",
              "userBorrowDetails": [
                {
                  "userId": 4,
                  "userName": "Diana Prince",
                  "borrowCount": 2
                },
                {
                  "userId": 5,
                  "userName": "Ethan Hunt",
                  "borrowCount": 2
                }
              ]
            }
      ```
    </details>
    <details>
      <summary>What books has a particular user borrowed during a specified period?</summary>

       ```
         Request: GET {{Library.Web.Api_HostAddress}}/api/User/user-borrowed-books/2?startDate=2025-07-08%2014%3A28%3A19.1638368%2B02%3A00&endDate=2025-08-08%2014%3A28%3A19.1638368%2B02%3A00
         Response:
            {
              "message": "Books retrieved successfully.",
              "userBookDetail": [
                {
                  "userName": "Bob Smith",
                  "bookId": 1,
                  "bookTitle": "C# in Depth",
                  "bookAuthor": "Jon Skeet"
                }
              ]
            }
      ```
    </details>


# EF Migrations Commands
    '''
        dotnet tool install --global dotnet-ef
        dotnet ef migrations add InitialCreate --project Library.Shared.Data --startup-project Library.Book.Service
        dotnet ef migrations add seed_sample_data --project Library.Shared.Data --startup-project Library.Book.Service
        dotnet ef database update   --project Library.Shared.Data   --startup-project Library.Book.Service


# Commands to run Tests
    dotnet test --filter "Category=Unit"
    dotnet test --filter "Category=Integration"


# Project Structure

```
│LibrarySystem/
│
├── Library.Web.Api/                   # API Layer HTTP endpoints (Handles HTTP interactions.)
│   └── Controllers/                   # HTTP Controllers for Book, User, and Borrower
│   └── Protos/                        # gRPC Client Protobuf definitions for Book, User, and Borrower services
│   └── appsettings.json               # Configuration file
│   └── Program.cs                     # Main entry point for the API
│   └── library.web.api.http           # HTTP client for Book, User, and Borrower services
│   └── README.md                      # API documentation and usage instructions
│
├── Library.Book.Service/              # gRPC Service Layer (Microservice - Encapsulates Book Service business logic and database operations)
│   └── Services/                      # gRPC Services implementation for Book operations
│   └── Protos/                        # gRPC server Protobuf definitions for Book services
│   └── Program.cs                     # Main entry point for the Book Service
│
├── Library.User.Service/              # gRPC Service Layer (Microservice - Encapsulates User Service business logic and database operations)
│   └── Services/                      # gRPC Services implementation for User operations
│   └── Protos/                        # gRPC server Protobuf definitions for User services
│   └── Program.cs                     # Main entry point for the User Service
│
├── Library.Borrower.Service/          # gRPC Service Layer (Microservice - Encapsulates Borrower Service business logic and database operations)
│   └── Services/                      # gRPC Services implementation for Borrower operations
│   └── Protos/                        # gRPC server Protobuf definitions for Borrower services
│   └── Program.cs                     # Main entry point for the Borrower Service
│
├── Library.Shared.Data/               # Shared entities and context + SQLite DB
│   └── Entities/                      # Shared entities for Book, User, and Borrower
│   └── Context/                       # Shared DbContext for EF Core
│   └── Database/                      # Database file (SQLite) library.db
│   └── Migrations/                    # EF Core migrations and seed data
│
├── Library.Shared/                    # Shared entities and context + SQLite DB
│   └── Protos/                        # gRPC Proto definitions for Book, User, and Borrower services
│
├── Library.Book.Service.Tests/        # Book service tests
│   └── BookServiceTests.cs            # Unit and integration tests for Book service
│
├── Library.User.Service.Tests/        # User service tests
│   └── UserServiceTests.cs            # Unit and integration tests for User service
│
├── Library.Borrower.Service.Tests/    # Borrower service tests
    └── BorrowerServiceTests.cs        # Unit and integration tests for Borrower service

```
