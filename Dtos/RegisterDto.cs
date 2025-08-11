using System.ComponentModel.DataAnnotations;

namespace JwtAuthDotnetEight.Dtos
{
    public class RegisterDto
    {
    [Required, MinLength(3)]
    public required string Username { get; set; }
    [Required, EmailAddress]
    public required string Email { get; set; }
    [Required, MinLength(6)]
    public required string Password { get; set; }
        public string Role { get; set; } = "User";
    }
}
