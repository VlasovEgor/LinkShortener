using LinkShortener.Services;
using Microsoft.AspNetCore.Mvc;

namespace LinkShortener.Controllers;

[ApiController]
[Route("api/links")]
public class LinksController : ControllerBase
{   
    private readonly LinksService _linksService;
    
    public LinksController(LinksService linksService)
    {
        _linksService = linksService;    
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromHeader(Name = "Idempotency-Key")] string? idempotencyKey, CreateLinkRequest request, CancellationToken cancellationToken)
    {
        LinkServiceResponse createTask = await _linksService.TryCreateLink(request.Url, idempotencyKey, cancellationToken);
        
        switch (createTask.Status)
        {   
            case GenerateStatus.Success:
                return Created($"api/links/{createTask.Code}", createTask);
            case GenerateStatus.InvalidUrl:
                return Problem(
                    statusCode: StatusCodes.Status400BadRequest,
                    title: "Invalid URL",
                    detail: "The URL must be absolute and use the HTTP or HTTPS scheme.");
            case GenerateStatus.GenerationFailed:
                return Problem(
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Code generation failed",
                    detail: "A unique short code could not be generated. Please try again later.");
            case GenerateStatus.Returned:
                return Ok(createTask);
        }

        return Problem();
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetStatistic(string code, CancellationToken cancellationToken)
    {   
        Link? statistics = await _linksService.TryGetStatistics(code, cancellationToken);
        if (statistics == null)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Link not found",
                detail: $"No link with code '{code}' was found.");
        }
        
        return Ok(statistics);
    }

    [HttpGet("/{code}")]
    public async Task<IActionResult> Get(string code, CancellationToken cancellationToken)
    {
        string? link = await _linksService.TryGetLink(code, cancellationToken);
        if (string.IsNullOrEmpty(link))
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Link not found",
                detail: $"No link with code '{code}' was found.");
        }

        await _linksService.IncreaseClickCount(code, cancellationToken);
        return Redirect(link);
    }

    [HttpDelete("{code}")]
    public async Task<IActionResult> Delete(string code, CancellationToken cancellationToken)
    {   
        bool hasDeleted = await _linksService.DeleteLink(code, cancellationToken);
        if (!hasDeleted)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Link not found",
                detail: $"No link with code '{code}' was found.");
        }

        return NoContent();
    }
}