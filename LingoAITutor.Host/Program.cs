using LingoAITutor.Host.Endpoints;
using LingoAITutor.Host.Entities;
using LingoAITutor.Host.Infrastructure;
using LingoAITutor.Host.Services;
using LingoAITutor.Host.Services.Common;
using LingoAITutor.Host.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OpenAI_API;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration().CreateLogger();
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services));

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
//builder.Services.AddDbContext<LingoDbContext>(options =>
    //options.UseSqlServer(connectionString));

builder.Services.AddDbContext<LingoDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<UserIdHepler>();
builder.Services.AddTransient<VocabluaryImport>();
builder.Services.AddTransient<Words100Import>();
builder.Services.AddTransient<NamesExcluding>();
builder.Services.AddTransient<IrregularImport>();
builder.Services.AddSingleton(new OpenAIAPI("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"));
builder.Services.AddTransient<TranslationExerciseAnaliser>();
builder.Services.AddTransient<TranslationExerciseGenerator>();
builder.Services.AddTransient<VocabularySizeCalculation>();
builder.Services.AddTransient<VocabularyMapGenerator>();
builder.Services.AddSingleton<AllWords>();
builder.Services.AddSingleton<IrregularVerbs>();
builder.Services.AddTransient<ChatProgressor>();

builder.Services.AddCors();

builder.Services.AddAuthentication().AddJwtBearer(options => {
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
       builder.WithOrigins("http://localhost:3000", "http://185.229.227.166")
              .AllowAnyMethod()
              .AllowAnyHeader());

app.UseAuthentication();
app.UseAuthorization();

VocabularyMapEndpoints.AddEndpoints(app);
VocabularyTrainingEndpoints.AddEndpoints(app);
Auth.AddEndpoints(app);
ChatEndpoints.AddEndpoints(app);
MessagesEndpoints.AddEndpoints(app);

using (var scope = app.Services.CreateScope())
{
    new DBSeeder(scope).Seed();
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

app.MapGet("/api/weatherforecast", () =>
{

    Log.Logger.Information("--------------------------- weatherforecast request");
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


internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}