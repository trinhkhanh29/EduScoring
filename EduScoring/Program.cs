using EduScoring.Common.Extensions;
using EduScoring.Features.Auth;
using EduScoring.Features.Exams;
using EduScoring.Features.System;
using EduScoring.Features.Users;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1.SERVICES
// ==========================================
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

// ==========================================
// 2. MIDDLEWARE PIPELINE
// ==========================================
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ==========================================
// 3. FEATURE ENDPOINTS (VSA)
// ==========================================
app.MapGetUsersEndpoint();
app.MapCreateExamEndpoint();
app.MapRegisterEndpoint();
app.MapLoginEndpoint();

// ==========================================
// 4. TEST ENDPOINTS
// ==========================================
app.MapTestEndpoints();

await app.RunAsync();