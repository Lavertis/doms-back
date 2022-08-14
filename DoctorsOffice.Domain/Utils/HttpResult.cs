using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Domain.Utils;

public class HttpResult<TValue> : Result<HttpResult<TValue>, TValue>
{
    public int StatusCode { get; set; } = StatusCodes.Status200OK;

    public HttpResult<TValue> WithStatusCode(int statusCode)
    {
        StatusCode = statusCode;
        return this;
    }
}