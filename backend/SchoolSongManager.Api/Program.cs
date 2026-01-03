using SchoolSongManager.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS with configurable origins
var allowedOrigins = builder.Configuration.GetSection("CORS:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:4200", "https://localhost:4200", "http://localhost:5168", "https://localhost:7159" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Add repositories
builder.Services.AddSingleton<INurseryRhymeRepository, NurseryRhymeRepository>();
builder.Services.AddSingleton<IThemeRepository, ThemeRepository>();
builder.Services.AddSingleton<NurseryRhymeAssignmentRepository>();

// Add PDF generator service
builder.Services.AddSingleton<IPdfGeneratorService, PdfGeneratorService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use CORS - MUST be before UseHttpsRedirection to handle preflight requests
app.UseCors("AllowAngularApp");

// Only redirect to HTTPS in production to avoid CORS issues with HTTP in development
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
