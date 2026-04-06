using EduScoring.Common.Storage;
using EduScoring.Data;
using EduScoring.Data.Entities;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<CloudinaryService>();

builder.Services.AddControllers();
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
//TEST SUBMISION IMAGE ENTITY
app.MapPost("/create-test-submission", async (AppDbContext db) =>
{
    var submission = new Submission
    {
        ExamId = 1, // nhớ phải tồn tại
        StudentId = null
    };

    db.Submissions.Add(submission);
    await db.SaveChangesAsync();

    return Results.Ok(submission.Id);
});
// TEST UPLOAD IMAGE TO CLOUDINARY
app.MapPost("/test-upload", async (
    IFormFile file,
    CloudinaryService cloudService,
    AppDbContext db) =>
{
    if (file == null || file.Length == 0)
        return Results.BadRequest("File không hợp lệ!");

    var url = await cloudService.UploadImageAsync(file);

    if (string.IsNullOrEmpty(url))
        return Results.BadRequest("Upload thất bại!");

    // Fetch an existing submission from the database to link the image to
    var submission = await db.Submissions.FirstOrDefaultAsync();

    if (submission == null)
        return Results.BadRequest("Không tìm thấy Submission nào trong Database để liên kết ảnh!");

    var imgEntity = new SubmissionImage
    {
        ImageUrl = url,
        PageNumber = 1,
        SubmissionId = submission.Id
    };

    db.SubmissionImages.Add(imgEntity);
    await db.SaveChangesAsync();

    return Results.Ok(new { Message = "OK 🚀", URL = url });

}).DisableAntiforgery();

await app.RunAsync();
