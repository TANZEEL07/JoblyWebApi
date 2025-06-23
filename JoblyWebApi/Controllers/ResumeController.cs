using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class ResumeController : ControllerBase
{
    private readonly ResumeRepository _repo = new ResumeRepository();

    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] ResumeUploadModel model)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        if (model.File == null || model.File.Length == 0)
            return BadRequest("Invalid file");

        string folder = Path.Combine("wwwroot", "resumes", userId.ToString());
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        string filePath = Path.Combine(folder, model.File.FileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await model.File.CopyToAsync(stream);
        }

        new ResumeRepository().Save(userId, model.File.FileName);
        return Ok("Resume uploaded");
    }

}
