using Microsoft.AspNetCore.Mvc;

namespace SchoolSongManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PingController : ControllerBase
{
    private readonly ILogger<PingController> _logger;
    private readonly IConfiguration _configuration;

    public PingController(ILogger<PingController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("Ping endpoint called at {Time}", DateTime.UtcNow);
        
        return Ok(new
        {
            Message = "Pong! SchoolSongManager API is running",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0"
        });
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            Status = "Healthy",
            Service = "SchoolSongManager.Api",
            Timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("cors-config")]
    public IActionResult GetCorsConfig()
    {
        var origins = _configuration.GetSection("CORS:AllowedOrigins").Get<string[]>();
        
        return Ok(new
        {
            AllowedOrigins = origins ?? new[] { "No CORS origins configured" },
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            Timestamp = DateTime.UtcNow
        });
    }

    [HttpGet("discovery")]
    public IActionResult GetDiscovery()
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        
        return Ok(new
        {
            ApiBaseUrl = baseUrl,
            Endpoints = new
            {
                Ping = "/api/ping",
                Health = "/api/ping/health",
                CorsConfig = "/api/ping/cors-config"
            },
            Version = "1.0.0",
            Timestamp = DateTime.UtcNow
        });
    }
}