using EduScoring.Application.UseCases;
using EduScoring.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// ── Infrastructure (repositories + LLM service) ─────────────────────────────
builder.Services.AddInfrastructure(builder.Configuration);

// ── Application use cases ────────────────────────────────────────────────────
builder.Services.AddScoped<EvaluateEssayUseCase>();
builder.Services.AddScoped<ManageGradingCriteriaUseCase>();

// ── API / MVC ────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "EduScoring API",
        Version = "v1",
        Description = "Automated essay scoring and feedback system powered by Large Language Models."
    });

    // Include XML comments in Swagger
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// ── HTTP pipeline ────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EduScoring API v1"));
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
