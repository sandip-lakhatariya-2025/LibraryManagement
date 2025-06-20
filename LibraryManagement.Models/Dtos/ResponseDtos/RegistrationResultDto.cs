namespace LibraryManagement.Models.Dtos.ResponseDtos;

public class RegistrationResultDto
{
    public long Id { get; set; }
    public string Email { get; set; } = null!;
    public string Firstname { get; set; } = null!;
    public string? Lastname { get; set; }
    public string Password { get; set; } = null!;
    public string Role { get; set; } = null!;
}
