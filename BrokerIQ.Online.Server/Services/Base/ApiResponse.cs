using System.Net;

namespace BrokerIQ.Online.Server.Services.Base;

public class ApiResponse<T>
{
    public HttpStatusCode StatusCode { get; set; }

    public T Data { get; set; }

    public ApiResponse(HttpStatusCode statusCode, T data)
    {
        StatusCode = statusCode;

        Data = IsSuccess ? data : default;
    }

    public ApiResponse(HttpStatusCode statusCode)
    {
        StatusCode = statusCode;

        Data = default;
    }

    public bool IsSuccess => ((int)StatusCode) >= 200 && ((int)StatusCode) < 300;

    public override string ToString()
    {
        return $"ApiResponse(StatusCode={StatusCode}, Data={Data})";
    }
}