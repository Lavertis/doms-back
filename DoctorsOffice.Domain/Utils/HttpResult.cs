using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Domain.Utils;

public class HttpResult<TValue> : Result<HttpResult<TValue>, TValue>
{
    public int StatusCode { get; set; } = StatusCodes.Status200OK;
    public string? ErrorField { get; set; }
    public bool HasFieldError => !string.IsNullOrEmpty(ErrorField);

    public HttpResult<TValue> WithStatusCode(int statusCode)
    {
        StatusCode = statusCode;
        return this;
    }

    public HttpResult<TValue> WithFieldError(string fieldName, Error error)
    {
        ErrorField = fieldName;
        Error = error;
        return this;
    }
}