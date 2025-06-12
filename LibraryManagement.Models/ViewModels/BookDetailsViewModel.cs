namespace LibraryManagement.Models.ViewModels;

public class BookDetailsViewModel : BooksViewModel
{
    public PublisherViewModel? PublisherDetails { get; set; }

    public ReaderDetailsViewModel? ReaderDetails { get; set; }
}
