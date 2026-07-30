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
    public async Task<IActionResult> Post(CreateLinkRequest request, CancellationToken cancellationToken)
    {
        LinkServiceResponse createTask = await _linksService.TryCreateLink(request.Url, cancellationToken);
        
        switch (createTask.Status)
        {   
            case GenerateStatus.Success:
                return Created();
            case GenerateStatus.InvalidUrl:
                return BadRequest();
            case GenerateStatus.GenerationFailed:
                return Problem();
        }

        return Problem();
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetStatistic(string code, CancellationToken cancellationToken)
    {   
        Link? statistics = await _linksService.TryGetStatistics(code, cancellationToken);
        if(statistics == null)
            return NotFound();
        
        return Ok(statistics);
    }

    [HttpGet("/{code}")]
    public async Task<IActionResult> Get(string code, CancellationToken cancellationToken)
    {
        string? link = await _linksService.TryGetLink(code, cancellationToken);
        if(string.IsNullOrEmpty(link))
            return NotFound();

        await _linksService.IncreaseClickCount(code, cancellationToken);
        return Redirect(link);
    }

    [HttpDelete("{code}")]
    public async Task<IActionResult> Delete(string code, CancellationToken cancellationToken)
    {   
        bool hasDeleted = await _linksService.DeleteLink(code, cancellationToken);
        return hasDeleted ? NoContent() : NotFound();
    }
}