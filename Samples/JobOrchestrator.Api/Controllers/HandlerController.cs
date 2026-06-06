using Microsoft.AspNetCore.Mvc;

namespace JobOrchestrator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HandlerController : ControllerBase
{
    private readonly ILogger<HandlerController> _logger;

    public HandlerController(ILogger<HandlerController> logger)
    {
        _logger = logger;
    }

    [HttpGet("execute")]
    public IActionResult Execute()
    {
        _logger.LogInformation("Handler executed via controller");
        Console.WriteLine("[HandlerController] Processing request...");
        
        return Ok(new 
        { 
            message = "Handler processed successfully",
            timestamp = DateTime.UtcNow,
            status = "success"
        });
    }
}
