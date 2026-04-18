using EduScoring.Common.Extensions;
using EduScoring.Common.Messaging;
using EduScoring.Features.Auth.Features.Login;
using EduScoring.Features.Auth.Features.Register;
using EduScoring.Features.Exams;
using EduScoring.Features.Exams.Features.CreateExam;
using EduScoring.Features.Exams.Features.DeleteExam;
using EduScoring.Features.Exams.Features.GetExamDetail;
using EduScoring.Features.Exams.Features.RestoreExam;
using EduScoring.Features.Exams.Features.UpdateExam;
using EduScoring.Features.Submissions;
using EduScoring.Features.Submissions.Features.CreateSubmission;
using EduScoring.Features.Submissions.Features.GetSubmissionDetail;
using EduScoring.Features.Submissions.Features.TriggerAiEvaluation;
using EduScoring.Features.Submissions.Features.UploadSubmissionImages;
using EduScoring.Features.Submissions.Features.ReviewAiEvaluation;
using EduScoring.Features.Submissions.Features.OverrideScore;
using EduScoring.Features.Submissions.Features.CreateHumanEvaluation;
using EduScoring.Features.Submissions.Features.FinalizeSubmission;
using EduScoring.Features.Submissions.Services;
using EduScoring.Features.System;
using EduScoring.Features.Users;
Console.OutputEncoding = System.Text.Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1.SERVICES
// ==========================================
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();
builder.Services.AddScoped<RegisterCommandHandler>();
// ĐĂNG KÝ SERVICES CỦA MODULE SUBMISSIONS
//builder.Services.AddScoped<IAiScoringService, AiScoringService>(); // (Tạm thời map Interface vào 1 class implement, nếu sếp chưa code class implement thì tạm comment dòng này lại)

// ĐĂNG KÝ CÁC HANDLER CỦA PHASE 1
builder.Services.AddScoped<EduScoring.Features.Submissions.Features.CreateSubmission.CreateSubmissionCommandHandler>();
builder.Services.AddScoped<EduScoring.Features.Submissions.Features.UploadSubmissionImages.UploadSubmissionImagesCommandHandler>();
builder.Services.AddScoped<EduScoring.Features.Submissions.Features.TriggerAiEvaluation.TriggerAiEvaluationCommandHandler>();
builder.Services.AddScoped<EduScoring.Features.Submissions.Features.GetSubmissionDetail.GetSubmissionDetailQueryHandler>();
builder.Services.AddScoped<EduScoring.Features.Submissions.Features.ReviewAiEvaluation.ReviewAiEvaluationCommandHandler>();
builder.Services.AddScoped<EduScoring.Features.Submissions.Features.OverrideScore.OverrideScoreCommandHandler>();
builder.Services.AddScoped<EduScoring.Features.Submissions.Features.CreateHumanEvaluation.CreateHumanEvaluationCommandHandler>();
builder.Services.AddScoped<EduScoring.Features.Submissions.Features.FinalizeSubmission.FinalizeSubmissionCommandHandler>();
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
app.MapUpdateExamEndpoint();
app.MapDeleteExamEndpoint();
app.MapGetExamDetailEndpoint();
app.MapRestoreExamEndpoint();

//Submissions
app.MapCreateSubmissionEndpoint();
app.MapUploadSubmissionImagesEndpoint();
app.MapTriggerAiEvaluationEndpoint();
app.MapGetSubmissionDetailEndpoint();
app.MapReviewAiEvaluationEndpoint();
app.MapOverrideScoreEndpoint();
app.MapCreateHumanEvaluationEndpoint();
app.MapFinalizeSubmissionEndpoint();

// ==========================================
// 4. TEST ENDPOINTS
// ==========================================
app.MapTestEndpoints();


await app.RunAsync();