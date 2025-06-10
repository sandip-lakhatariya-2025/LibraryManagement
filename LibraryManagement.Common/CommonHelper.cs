using System.Net;

namespace LibraryManagement.Common;

public static class CommonHelper
{
    public static Response<T> CreateResponse<T> (T data,  HttpStatusCode httpStatuscode, bool bIsSuccess, string? sMessage = "", string[]? arrErrorMessages = null) {
        try
        {
            return new Response<T>(data, bIsSuccess, sMessage, arrErrorMessages, httpStatuscode);
        }
        catch (System.Exception)
        {
            throw;
        }
    }

    public static PagedResponse<T> CreatePagedResponse<T>(
        T data,
        int nPageNumber, 
        int nPageSize, 
        int ntotalRecords, 
        int ntotalPages,
        HttpStatusCode statusCode,
        bool bIsSuccess,
        string sMessage = "",
        string[]? arrErrorMessages = null)
    {
        try
        {
            return new PagedResponse<T>(data, nPageNumber, nPageSize, ntotalRecords, ntotalPages, bIsSuccess, sMessage, statusCode, arrErrorMessages);
        }
        catch (System.Exception)
        {
            throw;
        }
    }
}
