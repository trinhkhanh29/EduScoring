using EduScoring.Data;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Thêm API Test Kết Nối Database
app.MapGet("/test-db", async (EduScoring.Data.AppDbContext dbContext) =>
{
    try
    {
        // Thử kết nối đến Supabase
        bool canConnect = await dbContext.Database.CanConnectAsync();
        if (canConnect)
        {
            return Results.Ok(new { status = "Thành công!", message = "Đã kết nối mượt mà tới Supabase PostgreSQL 🚀" });
        }
        else
        {
            return Results.StatusCode(500);
        }
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, title: "Kết nối thất bại ❌");
    }
});

await app.RunAsync();
