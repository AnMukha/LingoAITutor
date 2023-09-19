using LingoAITutor.Host.Endpoints;
using LingoAITutor.Host.Infrastructure;
using LingoAITutor.Host.Utilities;
using Microsoft.EntityFrameworkCore;
using OpenAI_API;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<LingoDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddTransient<VocabluaryImport>();
builder.Services.AddSingleton(new OpenAIAPI("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"));

builder.Services.AddCors();

var app = builder.Build();

app.UseCors(builder =>
       builder.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader());

VocabularyMapEndpoints.AddEndpoints(app);
VocabularyTrainingEndpoints.AddEndpoints(app);

using (var scope = app.Services.CreateScope())
{
    SeedDatabase(scope);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

static void SeedDatabase(IServiceScope scope)
{
    var context = scope.ServiceProvider.GetRequiredService<LingoDbContext>();
    if (!context.Words.Any())
    {
        var vocabluaryImport = scope.ServiceProvider.GetRequiredService<VocabluaryImport>();
        var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
        var path = Path.Combine(env.ContentRootPath, "Txt");
        vocabluaryImport.Import(Path.Combine(path, "cambridge.txt"), Path.Combine(path, "COCA 5000.txt"), Path.Combine(path, "10000.txt"));
    }
}


internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

