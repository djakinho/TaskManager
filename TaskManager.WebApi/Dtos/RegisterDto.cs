using System.ComponentModel.DataAnnotations;

namespace TaskManager.WebApi.Dtos;

public class RegisterDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = default!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = default!;
}
