using EduScoring.Common.Extensions;

Console.OutputEncoding = System.Text.Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Tự động map tất cả endpoints trong assembly — không cần gọi tay từng cái
app.MapAllEndpoints();

await app.RunAsync();