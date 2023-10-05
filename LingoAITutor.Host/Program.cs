using LingoAITutor.Host.Endpoints;
using LingoAITutor.Host.Entities;
using LingoAITutor.Host.Infrastructure;
using LingoAITutor.Host.Services;
using LingoAITutor.Host.Services.Common;
using LingoAITutor.Host.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenAI_API;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JWTToken_Auth_API",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<LingoDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddTransient<VocabluaryImport>();
builder.Services.AddSingleton(new OpenAIAPI("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"));
builder.Services.AddTransient<TranslationExerciseAnaliser>();
builder.Services.AddTransient<TranslationExerciseGenerator>();
builder.Services.AddTransient<VocabularySizeCalculation>();
builder.Services.AddTransient<VocabularyMapGenerator>();
builder.Services.AddSingleton<AllWords>();

builder.Services.AddCors();

builder.Services.AddAuthentication().AddJwtBearer(options => {
    //options.Audience = "https://localhost:5001/";
    //options.Authority = "https://localhost:5000/";
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("SECRET_KEY1SECRET_KEY1SECRET_KEY1SECRET_KEY1")),
        ValidateIssuer = false, // In a more advanced setup, this would be true.
        ValidateAudience = false, // Likewise here.
    };    
    options.MetadataAddress = string.Empty;
});
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors(builder =>
       builder.WithOrigins("http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader());

app.UseAuthentication();
app.UseAuthorization();

VocabularyMapEndpoints.AddEndpoints(app);
VocabularyTrainingEndpoints.AddEndpoints(app);
Auth.AddEndpoints(app);

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
    context.Words.Where(w => SpecialWords.GrammarWords.Contains(w.Text)).ExecuteDelete();

    if (!context.Users.Any())
    {
        context.Users.Add(new User() { Id = TranslationExerciseAnaliser.UserId, UserName = "test", Email = "test@test.com" });        
    }
    if (!context.UserProgresses.Any())
    {
        context.UserProgresses.Add(new UserProgress() { UserId = TranslationExerciseAnaliser.UserId });
        context.RangeProgresses.Add(new RangeProgress()
        {
            Id = Guid.NewGuid(),
            UserProgressId = TranslationExerciseAnaliser.UserId,
            StartPosition = 0,
            WordsCount = 500
        });
        context.RangeProgresses.Add(new RangeProgress()
        {
            Id = Guid.NewGuid(),
            UserProgressId = TranslationExerciseAnaliser.UserId,
            StartPosition = 500,
            WordsCount = 500
        });
        for (var i = 1; i < context.Words.Count() / 1000 + 1; i++)
        {
            context.RangeProgresses.Add(new RangeProgress()
            {
                Id = Guid.NewGuid(),
                UserProgressId = TranslationExerciseAnaliser.UserId,
                StartPosition = i * 1000,
                WordsCount = 1000
            });
        };
    }
    context.SaveChanges();
}

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}