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
    public IActionResult Post(CreateLinkRequest request, CancellationToken cancellationToken)
    {
        if (!_linksService.TryCreateLink(request.Url, cancellationToken, out string code, out GenerateStatus result))
        {
            switch (result)
            {
                case GenerateStatus.InvalidUrl:
                    return BadRequest();
                case GenerateStatus.GenerationFailed:
                    return Problem();
            }
            
            return Problem();
        }
        
        Console.WriteLine(code);
        return Created();
    }

    [HttpGet("{code}")]
    public IActionResult GetStatistic(string code)
    {
        Console.WriteLine(code);
        Console.WriteLine(DateTime.Now);
        return Ok();
    }

    [HttpGet("/{code}")]
    public IActionResult Get(string code)
    {
        Console.WriteLine("code: " + code);
        
        if(!_linksService.TryGetLink(code, out string originalUrl))
        {
            return NotFound();
        }
        Console.WriteLine("originalUrl:  " + originalUrl);
        return Redirect(originalUrl);
    }

    [HttpDelete("{code}")]
    public IActionResult Delete(string code)
    {
        if (_linksService.DeleteLink(code))
            return NoContent();
        
        return NotFound();
    }
}