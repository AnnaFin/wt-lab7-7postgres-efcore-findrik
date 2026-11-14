using Microsoft.EntityFrameworkCore;
using wt_lab7_7postgres_efcore_findrik.Models;
using wt_lab7_7postgres_efcore_findrik.Data;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

if (!string.IsNullOrWhiteSpace(databaseUrl))
{
    var uri = new Uri(databaseUrl);
    var userInfo = uri.UserInfo.Split(':', 2);
    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
}

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "IT Tasks API (PostgreSQL)", Version = "v1" });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Minimal API с PostgreSQL работает!");

app.MapGet("/api/tasks", async (string? search, AppDbContext db) =>
{
    var query = db.Tasks.AsQueryable();
    if (!string.IsNullOrWhiteSpace(search))
    {
        query = query.Where(x => x.Title.ToLower().Contains(search.ToLower()));
    }
    var tasks = await query.ToListAsync();
    return Results.Ok(tasks);
});

/// 1. Фильтрация (Задачи с оценкой больше 20 часов)
app.MapGet("/api/tasks/long", async (AppDbContext db) =>
{
    var tasks = await db.Tasks.Where(x => x.EstimatedHours > 20).ToListAsync();
    return Results.Ok(tasks);
});

// 2. Сортировка (По убыванию часов)
app.MapGet("/api/tasks/sorted", async (AppDbContext db) =>
{
    var tasks = await db.Tasks.OrderByDescending(x => x.EstimatedHours).ToListAsync();
    return Results.Ok(tasks);
});

// 3. Выборка с условием (Только выполненные, сортировка по названию)
app.MapGet("/api/tasks/completed", async (AppDbContext db) =>
{
    var tasks = await db.Tasks.Where(x => x.IsCompleted).OrderBy(x => x.Title).ToListAsync();
    return Results.Ok(tasks);
});

// GET: По ID
app.MapGet("/api/tasks/{id}", async (int id, AppDbContext db) =>
{
    var task = await db.Tasks.FirstOrDefaultAsync(x => x.Id == id);
    if (task == null) return Results.NotFound(new { Message = "Задача не найдена" });
    return Results.Ok(task);
});

// POST: Создание
app.MapPost("/api/tasks", async (ItTask newTask, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(newTask.Title))
        return Results.BadRequest(new { Message = "Название обязательно" });

    newTask.CreatedAt = DateTime.UtcNow;
    db.Tasks.Add(newTask);
    await db.SaveChangesAsync();
    
    return Results.Created($"/api/tasks/{newTask.Id}", newTask);
});

// PUT: Изменение
app.MapPut("/api/tasks/{id}", async (int id, ItTask updatedTask, AppDbContext db) =>
{
    var task = await db.Tasks.FirstOrDefaultAsync(x => x.Id == id);
    if (task == null) return Results.NotFound();

    task.Title = updatedTask.Title;
    task.ProjectName = updatedTask.ProjectName;
    task.Priority = updatedTask.Priority;
    task.EstimatedHours = updatedTask.EstimatedHours;
    task.IsCompleted = updatedTask.IsCompleted;

    await db.SaveChangesAsync();
    return Results.Ok(task);
});

// DELETE: Удаление
app.MapDelete("/api/tasks/{id}", async (int id, AppDbContext db) =>
{
    var task = await db.Tasks.FirstOrDefaultAsync(x => x.Id == id);
    if (task == null) return Results.NotFound();

    db.Tasks.Remove(task);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
app.Run($"http://0.0.0.0:{port}");