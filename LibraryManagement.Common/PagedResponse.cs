using System.Net;

namespace LibraryManagement.Common;

public class PagedResponse<T> : Response<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    public PagedResponse() : base() { }

    public PagedResponse(T data, int nPageNumber, int nPageSize, int ntotalRecords, int ntotalPages, bool bIsSuccess, string sMessage, HttpStatusCode statusCode, string[] arrErrorMessages)
        : base(data, bIsSuccess, sMessage, arrErrorMessages, statusCode)
    {
        PageNumber = nPageNumber;
        PageSize = nPageSize;
        TotalRecords = ntotalRecords;
        TotalPages = ntotalPages;
    }
}
