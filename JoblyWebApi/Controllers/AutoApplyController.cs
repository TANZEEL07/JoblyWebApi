using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AutoApplyController : ControllerBase
{
    [HttpPost("naukri")]
    public IActionResult Apply()
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Normally you fetch from DB
        var filters = new JobFilterRepository().GetByUserId(userId).FirstOrDefault();
        if (filters == null) return BadRequest("No filters found");

        // Dummy login (replace with actual DB stored login)
        string email = "sayedawesali190@gmail.com";
        string password = "awes@123";

        var engine = new NaukriApplyEngine(email, password, userId);
        engine.Run(filters.Role, filters.Location, filters.Skills);

        return Ok("Naukri auto-apply completed");
    }
}
