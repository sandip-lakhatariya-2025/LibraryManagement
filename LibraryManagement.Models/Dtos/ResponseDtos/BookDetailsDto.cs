namespace LibraryManagement.Models.Dtos.ResponseDtos;

public class BookDetailsDto : BookResultDto
{
    public PublisherResultDto? PublisherDetails { get; set; }

    public ReaderDetailsDto? ReaderDetails { get; set; }
}
