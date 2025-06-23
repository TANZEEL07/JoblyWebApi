using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class JobFilterController : ControllerBase
{
    [HttpPost("save")]
    public IActionResult Save([FromBody] UserJobFilter model)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        new JobFilterRepository().Save(userId, model.Role, model.Location, model.Skills);
        return Ok("Job filters saved");
    }

    [HttpGet("my")]
    public IActionResult Get()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var filters = new JobFilterRepository().GetByUserId(userId);
        return Ok(filters);
    }
}
