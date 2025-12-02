
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 🔹 הגדרת CORS (אם כבר יש – לא חובה להוסיף שוב)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// 🔹 הגדרת DbContext
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("ToDoDB"),
        new MySqlServerVersion(new Version(8, 0, 36))
    ));

// 🔹 הוספת Swagger לשירותים
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Todo API",
        Version = "v1",
        Description = "API לניהול משימות"
    });
});

var app = builder.Build();

// 🔹 הפעלת Swagger בזמן ריצה
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API V1");
    });
}

app.UseCors("AllowAll");

// 🔹 ניתובי API
app.MapGet("/api/items", async (ToDoDbContext db) => await db.Items.ToListAsync());
app.MapPost("/api/items", async (ToDoDbContext db, Item newItem) =>
{
    db.Items.Add(newItem);
    await db.SaveChangesAsync();
    return Results.Created($"/api/items/{newItem.Id}", newItem);
});

app.MapPut("/api/items/{id}", async (ToDoDbContext db, int id, Item updatedItem) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound();

    if (string.IsNullOrEmpty(updatedItem.Name))
        updatedItem.Name = item.Name; // שומר על השם הקיים אם לא נשלח

    item.Name = updatedItem.Name;
    item.IsComplete = updatedItem.IsComplete;

    await db.SaveChangesAsync();
    return Results.Ok(item);
});

app.MapDelete("/api/items/{id}", async (ToDoDbContext db, int id) =>
{
    var item = await db.Items.FindAsync(id);
    if (item is null) return Results.NotFound();
    db.Items.Remove(item);
    await db.SaveChangesAsync();
    return Results.Ok();
});

app.Run();
