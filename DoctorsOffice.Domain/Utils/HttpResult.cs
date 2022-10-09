using FluentValidation.Results;
using Microsoft.AspNetCore.Http;

namespace DoctorsOffice.Domain.Utils;

public class HttpResult<TValue> : Result<HttpResult<TValue>, TValue>
{
    public IDictionary<string, string[]>? ValidationErrors;
    public int StatusCode { get; set; } = StatusCodes.Status200OK;
    public bool HasValidationErrors => ValidationErrors != null;

    public HttpResult<TValue> WithStatusCode(int statusCode)
    {
        StatusCode = statusCode;
        return this;
    }

    public HttpResult<TValue> WithValidationErrors(List<ValidationFailure> validationFailures)
    {
        ValidationErrors = validationFailures
            .GroupBy(x => x.PropertyName, s => s.ErrorMessage)
            .ToDictionary(g => g.Key, g => g.ToArray());
        StatusCode = StatusCodes.Status400BadRequest;
        return this;
    }

    public HttpResult<TValue> WithValidationErrors(IDictionary<string, string[]>? validationErrors)
    {
        ValidationErrors = validationErrors;
        StatusCode = StatusCodes.Status400BadRequest;
        return this;
    }
}