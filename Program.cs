using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// The call to AddEndpointsApiExplorer shown is required only for minimal APIs.
builder.Services.AddEndpointsApiExplorer();

// Register the Swagger generator, defining 1 or more Swagger documents
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Todo API",
        Description = "An ASP.NET Core Web API for managing todos",
        TermsOfService = new Uri("https://example.com/terms"),
        Contact = new OpenApiContact
        {
            Name = "Kenan Hancer",
            Email = "kenanhancer@gmail.com",
            Url = new Uri("https://kenanhancer.com")
        },
        License = new OpenApiLicense
        {
            Name = "Todo License",
            Url = new Uri("https://example.com/license")
        }
    });
});

builder.Services.AddDbContext<TodoDb>(opt => opt.UseInMemoryDatabase("TodoList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

var app = builder.Build();

// Configure the HTTP request pipeline:
if (app.Environment.IsDevelopment())
{
    // Enable middleware to serve generated Swagger as a JSON endpoint.
    app.UseSwagger();

    // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
    app.UseSwaggerUI(options =>
    {
        // To serve the Swagger UI at the app's root (https://localhost:<port>/), set the RoutePrefix property to an empty string:
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}

// Endpoints:

app.MapGet("/todoitems", async (TodoDb db) =>
{
    return await db.Todos.ToListAsync();
})
.WithName("ListTodos")
.WithDescription("Fetches the todo list")
.WithSummary("Fetches the todo list")
.WithTags("Get Operations")
.WithOpenApi();

app.MapGet("/todoitems/complete", async (TodoDb db) =>
{
    return await db.Todos.Where(t => t.IsComplete).ToListAsync();
})
.WithName("ListCompletedTodos")
.WithDescription("Fetches the completed todo list")
.WithSummary("Fetches the completed todo list")
.WithTags("Get Operations")
.WithOpenApi();

app.MapGet("/todoitems/{id}", async (int id, TodoDb db) =>
{
    return await db.Todos.FindAsync(id) is Todo todo ? Results.Ok(todo) : Results.NotFound();
})
.WithOpenApi(operation =>
    new OpenApiOperation(operation)
    {
        OperationId = "GetTodoById",
        Description = "Get todo by id",
        Summary = "Get todo by id",
        Tags = new List<OpenApiTag>() { new OpenApiTag() { Name = "Get Operations" } },
        Parameters = new List<OpenApiParameter>()
        {
            new OpenApiParameter(operation.Parameters[0])
            {
                Description = "The ID associated with the created Todo"
            }
        }
    }
);

app.MapPost("/todoitems", async (Todo todo, TodoDb db) =>
{
    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    return Results.Created($"/todoitems/{todo.Id}", todo);
})
.WithName("AddTodoItem")
.WithDescription("Add todo item")
.WithSummary("Add todo item")
.WithTags("Set Operations")
.WithOpenApi();

app.MapPut("/todoitems/{id}", async (int id, Todo inputTodo, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.Name = inputTodo.Name;
    todo.IsComplete = inputTodo.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
})
.WithName("ReplaceTodoItem")
.WithDescription("Replace Todo Item")
.WithSummary("Replace Todo Item")
.WithTags("Set Operations")
.WithOpenApi();

app.MapDelete("/todoitems/{id}", async (int id, TodoDb db) =>
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();

        return Results.Ok(todo);
    }

    return Results.NotFound();
})
.WithName("DeleteTodoItemById")
.WithDescription("Delete Todo Item by Id")
.WithSummary("Delete Todo Item by Id")
.WithTags("Set Operations")
.WithOpenApi();

app.Run();

class Todo
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public bool IsComplete { get; set; }
}

class TodoDb : DbContext
{
    public TodoDb(DbContextOptions<TodoDb> options) : base(options) { }

    public DbSet<Todo> Todos => Set<Todo>();
}