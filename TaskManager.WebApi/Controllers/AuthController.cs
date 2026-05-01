using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Application;
using TaskManager.Domain;
using TaskManager.WebApi.Dtos;

namespace TaskManager.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email
            };

            await _authService.RegisterAsync(user, dto.Password);
            return StatusCode(StatusCodes.Status201Created);
        }
        catch (ValidationException ex)
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                { string.Empty, new[] { ex.Message } }
            }));
        }
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var token = await _authService.LoginAsync(dto.Email, dto.Password);
            return Ok(new { token });
        }
        catch (ValidationException ex)
        {
            return ValidationProblem(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                { string.Empty, new[] { ex.Message } }
            }));
        }
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var name = User.FindFirstValue(ClaimTypes.Name);
        var email = User.FindFirstValue(ClaimTypes.Email);

        if (id is null || name is null || email is null)
        {
            return Unauthorized();
        }

        return Ok(new { id, name, email });
    }
}
