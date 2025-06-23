using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly JwtService _jwt;
    private readonly UserRepository _repo = new UserRepository();

    public AuthController(IConfiguration config)
    {
        _jwt = new JwtService(config["Jwt:Key"]);
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] User user)
    {
        var existing = _repo.GetByEmail(user.Email);
        if (existing != null)
            return BadRequest("Email already exists");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        _repo.Register(user);
        return Ok("User Registered");
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        var existing = _repo.GetByEmail(request.Email);
        if (existing == null || !BCrypt.Net.BCrypt.Verify(request.Password, existing.PasswordHash))
            return Unauthorized("Invalid credentials");

        string token = _jwt.GenerateToken(existing);
        return Ok(new { Token = token });
    }

}
