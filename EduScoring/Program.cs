using EduScoring.Common.Extensions;
using EduScoring.Features.Auth;
using EduScoring.Features.Exams;
using EduScoring.Features.Submissions;
using EduScoring.Features.System;
using EduScoring.Features.Users;
using EduScoring.Common.Messaging;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1.SERVICES
// ==========================================
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();

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
// 3. FEATURE ENDPOINTS
// ==========================================
app.MapGetUsersEndpoint();
app.MapCreateExamEndpoint();
app.MapRegisterEndpoint();
app.MapLoginEndpoint();
//Exam
app.MapSubmitExamEndpoint();
app.MapUpdateExamEndpoint();
app.MapDeleteExamEndpoint();
app.MapGetExamDetailEndpoint();

// ==========================================
// 4. TEST ENDPOINTS
// ==========================================
app.MapTestEndpoints();

await app.RunAsync();