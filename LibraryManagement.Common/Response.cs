using System.Net;

namespace LibraryManagement.Common;

public class Response<T>
{
    public Response() {}

    public Response(T data, bool bIsSuccess, string sMessage, string[] arrErrorMessages, HttpStatusCode httpStatuscode) {
        Data = data;
        Succeeded = bIsSuccess;
        Message = sMessage;
        Errors = arrErrorMessages;
        StatusCode = httpStatuscode;
    }

    public T Data { get; set; }
    public bool Succeeded { get; set; }
    public string Message { get; set; }
    public string[] Errors { get; set; }
    public HttpStatusCode StatusCode { get; set; }
}
