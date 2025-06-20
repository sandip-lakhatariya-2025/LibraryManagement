namespace LibraryManagement.Models.Dtos.ResponseDtos;

public class CustomerResultDto
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string Email { get; set; } = null!;
}
