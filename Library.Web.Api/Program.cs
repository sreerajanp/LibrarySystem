using Library.Book;
using Library.Borrow;
using Library.User;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Add Swagger generator
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Library API",
        Version = "v1",
        Description = "Library Web API"
    });
});

// Register the gRPC client for BookService
builder.Services.AddGrpcClient<BookService.BookServiceClient>(o =>
{
    o.Address = new Uri("https://localhost:7009"); 
});

// Register the gRPC client for UserService
builder.Services.AddGrpcClient<UserService.UserServiceClient>(o =>
{
    o.Address = new Uri("https://localhost:7111"); 
});

// Register the gRPC client for BorrowService
builder.Services.AddGrpcClient<BorrowService.BorrowServiceClient>(o =>
{
    o.Address = new Uri("https://localhost:7288"); 
});


var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(); // default at /swagger
}

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
