using FluentValidation;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// The call to AddEndpointsApiExplorer shown is required only for minimal APIs.
builder.Services.AddEndpointsApiExplorer();

// Register the Swagger generator, defining 1 or more Swagger documents
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("user", new OpenApiInfo
    {
        Version = "1.0",
        Title = "User API",
        Description = "API endpoints for managing users",
        Contact = new OpenApiContact
        {
            Name = "Kenan Hancer",
            Email = "kenanhancer@gmail.com",
            Url = new Uri("https://kenanhancer.com")
        },
        License = new OpenApiLicense
        {
            Name = "User License",
            Url = new Uri("https://example.com/license")
        }
    });

    options.SwaggerDoc("todo", new OpenApiInfo
    {
        Version = "1.0",
        Title = "Todo API",
        Description = "API endpoints for managing todos",
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
builder.Services.AddSingleton<IDatabaseInitializer, TodoDbInitializer>();
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();


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
        options.SwaggerEndpoint("/swagger/todo/swagger.json", "Todo API");
        options.SwaggerEndpoint("/swagger/user/swagger.json", "User API");
        options.RoutePrefix = string.Empty;
    });

    IDatabaseInitializer? dbInitializer = app.Services.GetService<IDatabaseInitializer>();

    dbInitializer?.Initialize();
}

// Endpoints:

RouteGroupBuilder todosRoute = app.MapGroup("/todos");

todosRoute.MapGet("/", GetAllTodos)
    .WithName("GetAllTodos")
    .WithDescription("Returns a list of all todo items.")
    .WithSummary("Retrieve all todo items")
    .WithTags("Todo Read Operations")
    .WithGroupName("todo")
    .WithOpenApi();

todosRoute.MapGet("/completed", GetCompletedTodos)
    .WithName("GetCompletedTodos")
    .WithDescription("Returns a list of completed todo items.")
    .WithSummary("Retrieve completed todo items")
    .WithTags(tags: "Todo Read Operations")
    .WithGroupName("todo")
    .WithOpenApi();

todosRoute.MapGet("/active", GetActiveTodos)
    .WithName("GetActiveTodos")
    .WithDescription("Returns a list of active todo items.")
    .WithSummary("Retrieve active todo items")
    .WithTags("Todo Read Operations")
    .WithGroupName("todo")
    .WithOpenApi();

todosRoute.MapGet("/{id:guid}", GetTodo)
    .WithGroupName("todo")
    .WithOpenApi(operation =>
        new OpenApiOperation(operation)
        {
            OperationId = "GetTodoById",
            Description = "Returns todo item with the specified ID.",
            Summary = "Retrieve a specific todo item",
            Tags = new List<OpenApiTag>() { new OpenApiTag() { Name = "Todo Read Operations" } },
            Parameters = new List<OpenApiParameter>()
            {
                new OpenApiParameter(operation.Parameters[0])
                {
                    Description = "The ID associated with the created Todo"
                }
            }
        }
    );

todosRoute.MapPost("/", CreateTodo)
    .WithName("AddTodoItem")
    .WithDescription("Add todo item")
    .WithSummary("Add todo item")
    .WithTags("Todo Write Operations")
    .WithGroupName("todo")
    .WithOpenApi();

todosRoute.MapPut("/{id:guid}", ReplaceTodo)
    .WithName("ReplaceTodoItem")
    .WithDescription("Updates todo item with the specified ID.")
    .WithSummary("Replace Todo Item")
    .WithTags("Todo Write Operations")
    .WithGroupName("todo")
    .WithOpenApi();

todosRoute.MapPatch("/{id:guid}/status", UpdateTodoStatus)
    .WithName("UpdateTodoStatus")
    .WithDescription("Updates todo status with the specified ID.")
    .WithSummary("Update Todo Status")
    .WithTags("Todo Write Operations")
    .WithGroupName("todo")
    .WithOpenApi();

todosRoute.MapPatch("/{id:guid}/complete", MarkTodoAsCompleted)
    .WithName("MarkTodoAsComplete")
    .WithDescription("Marks a specific todo item as complete.")
    .WithSummary("Mark a specific todo item as complete")
    .WithTags("Todo Write Operations")
    .WithGroupName("todo")
    .WithOpenApi();

todosRoute.MapPatch("/{id}/active", MarkTodoAsActive)
    .WithName("MarkTodoAsActive")
    .WithDescription("Marks a specific todo item as active (not complete).")
    .WithSummary("Mark a specific todo item as active")
    .WithTags("Todo Write Operations")
    .WithGroupName("todo")
    .WithOpenApi();

todosRoute.MapDelete("/{id:guid}", DeleteTodo)
    .WithName("DeleteTodo")
    .WithDescription("Deletes todo item with the specified ID.")
    .WithSummary("Delete a specific todo item")
    .WithTags("Todo Write Operations")
    .WithGroupName("todo")
    .WithOpenApi();

app.Run();

static async Task<IResult> GetAllTodos(TodoDb db)
{
    return TypedResults.Ok(await db.Todos.ToArrayAsync());
}

static async Task<IResult> GetCompletedTodos(TodoDb db)
{
    return TypedResults.Ok(await db.Todos.Where(t => t.IsCompleted).ToListAsync());
}

static async Task<IResult> GetActiveTodos(TodoDb db)
{
    return TypedResults.Ok(await db.Todos.Where(t => !t.IsCompleted).ToListAsync());
}

static async Task<IResult> GetTodo([FromRoute] Guid id, TodoDb db)
{
    return await db.Todos.FindAsync(id)
        is Todo todo
            ? TypedResults.Ok(todo)
            : TypedResults.NotFound();
}

static async Task<IResult> CreateTodo([FromBody] Todo todo, TodoDb db)
{
    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    return TypedResults.Created($"/todos/{todo.Id}", todo);
}

static async Task<IResult> ReplaceTodo([FromRoute] Guid id, [FromBody] Todo inputTodo, TodoDb db)
{
    Todo? todo = await db.Todos.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.Name = inputTodo.Name;
    todo.IsCompleted = inputTodo.IsCompleted;

    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}

static async Task<IResult> UpdateTodoStatus([FromRoute] Guid id, [FromBody] TodoStatusUpdate todoStatusUpdate, TodoDb db)
{
    if (todoStatusUpdate == null)
    {
        return TypedResults.BadRequest();
    }

    Todo? todo = await db.Todos.FindAsync(id);

    if (todo == null)
    {
        return TypedResults.NotFound();
    }

    todo.IsCompleted = todoStatusUpdate.IsCompleted;
    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}

static async Task<IResult> MarkTodoAsCompleted([FromRoute] Guid id, TodoDb db)
{
    var todo = await db.Todos.FindAsync(id);

    if (todo == null)
    {
        return TypedResults.NotFound();
    }

    todo.IsCompleted = true;
    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}

static async Task<IResult> MarkTodoAsActive([FromRoute]Guid id, TodoDb db)
{
    var todo = await db.Todos.FindAsync(id);
    if (todo == null)
    {
        return Results.NotFound();
    }

    todo.IsCompleted = false;
    await db.SaveChangesAsync();

    return Results.NoContent();
}

static async Task<IResult> DeleteTodo([FromRoute] Guid id, TodoDb db)
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();

        return TypedResults.Ok(todo);
    }

    return TypedResults.NotFound();
}

class Todo
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
}

class User
{
    public Guid Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
}

record TodoCreateRequest(string Name);

record TodoResponse(Guid Id, string Name, bool IsCompleted, string UserFullName);

record TodoStatusUpdate(bool IsCompleted);

class TodoDb : DbContext
{
    public TodoDb(DbContextOptions<TodoDb> options) : base(options)
    {
    }

    public DbSet<Todo> Todos => Set<Todo>();
    public DbSet<User> Users => Set<User>();
}

class TodoValidator : AbstractValidator<Todo>
{
    public TodoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name must not be empty.");
        RuleFor(x => x.IsCompleted).Equal(false).WithMessage("IsCompleted must be false when creating a Todo item.");
    }
}

interface IDateTimeProvider
{
    DateTime GetCurrentDateTime();
}

interface IDatabaseInitializer
{
    void Initialize();
}

class DateTimeProvider : IDateTimeProvider
{
    public DateTime GetCurrentDateTime() => DateTime.UtcNow;
}

class TodoDbInitializer : IDatabaseInitializer
{
    private readonly IServiceProvider _serviceProvider;

    public TodoDbInitializer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Initialize()
    {
        using (IServiceScope scope = _serviceProvider.CreateScope())
        {
            TodoDb dbContext = scope.ServiceProvider.GetRequiredService<TodoDb>();

            // Check if the database has been created
            if (dbContext.Database.EnsureCreated())
            {
                IDateTimeProvider? dateTimeProvider = scope.ServiceProvider.GetService<IDateTimeProvider>();

                SeedData(dbContext, dateTimeProvider);
            }
        }
    }

    private void SeedData(TodoDb db, IDateTimeProvider? dateTimeProvider)
    {
        DateTime now = dateTimeProvider?.GetCurrentDateTime() ?? DateTime.UtcNow;

        // Perform data seeding logic here
        List<User> users = new List<User>
        {
            new User { Id = Guid.Parse("00000000-0000-0000-0000-000000000000"), FullName = "John Doe", Email = "john@example.com" },
            new User { Id = Guid.Parse("00000000-0000-0000-0000-000000000001"), FullName = "Jane Smith", Email = "jane@example.com" },
            new User { Id = Guid.Parse("00000000-0000-0000-0000-000000000002"), FullName = "Alice Johnson", Email = "alice@example.com" },
            new User { Id = Guid.Parse("00000000-0000-0000-0000-000000000003"), FullName = "Bob Wilson", Email = "bob@example.com" },
            new User { Id = Guid.Parse("00000000-0000-0000-0000-000000000004"), FullName = "Eva Davis", Email = "eva@example.com" },
            new User { Id = Guid.Parse("00000000-0000-0000-0000-000000000005"), FullName = "Michael Thompson", Email = "michael@example.com" },
            new User { Id = Guid.Parse("00000000-0000-0000-0000-000000000006"), FullName = "Sophia White", Email = "sophia@example.com" },
            new User { Id = Guid.Parse("00000000-0000-0000-0000-000000000007"), FullName = "William Anderson", Email = "william@example.com" },
            new User { Id = Guid.Parse("00000000-0000-0000-0000-000000000008"), FullName = "Olivia Davis", Email = "olivia@example.com" },
            new User { Id = Guid.Parse("00000000-0000-0000-0000-000000000009"), FullName = "James Wilson", Email = "james@example.com" },
            new User { Id = Guid.Parse("00000000-0000-0000-0000-000000000010"), FullName = "Emily Taylor", Email = "emily@example.com" }
        };

        Guid johnDoe = Guid.Parse("00000000-0000-0000-0000-000000000000");
        Guid janeSmith = Guid.Parse("00000000-0000-0000-0000-000000000001");

        List<Todo> todos = new List<Todo>
        {
            new Todo { Id = Guid.Parse("10000000-0000-0000-0000-000000000000"), Name = "Buy groceries", IsCompleted = false, CreatedAt = now, UserId = johnDoe},
            new Todo { Id = Guid.Parse("10000000-0000-0000-0000-000000000001"), Name = "Finish project report", IsCompleted = true, UserId = janeSmith },
            new Todo { Id = Guid.Parse("10000000-0000-0000-0000-000000000002"), Name = "Book doctor appointment", IsCompleted = false, UserId = janeSmith },
            new Todo { Id = Guid.Parse("10000000-0000-0000-0000-000000000003"), Name = "Walk the dog", IsCompleted = true, UserId = johnDoe },
            new Todo { Id = Guid.Parse("10000000-0000-0000-0000-000000000004"), Name = "Prepare for meeting", IsCompleted = false, UserId = johnDoe },
            new Todo { Id = Guid.Parse("10000000-0000-0000-0000-000000000005"), Name = "Pick up kids from school", IsCompleted = true, UserId = janeSmith },
            new Todo { Id = Guid.Parse("10000000-0000-0000-0000-000000000006"), Name = "Call the bank", IsCompleted = false, UserId = johnDoe },
            new Todo { Id = Guid.Parse("10000000-0000-0000-0000-000000000007"), Name = "Study for the exam", IsCompleted = true, UserId = johnDoe },
            new Todo { Id = Guid.Parse("10000000-0000-0000-0000-000000000008"), Name = "Clean the house", IsCompleted = false, UserId = janeSmith },
            new Todo { Id = Guid.Parse("10000000-0000-0000-0000-000000000009"), Name = "Pay the bills", IsCompleted = true, UserId = janeSmith }
        };

        db.Users.AddRange(users);
        db.Todos.AddRange(todos);
        db.SaveChanges();
    }
}