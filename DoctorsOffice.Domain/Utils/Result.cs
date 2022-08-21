namespace DoctorsOffice.Domain.Utils;

public abstract class Result<TResult, TValue> where TResult : Result<TResult, TValue>
{
    public Error? Error;
    public TValue? Value;

    public bool IsSuccess => Error is null;
    public bool IsFailed => Error is not null;

    public TResult WithValue(TValue value)
    {
        Value = value;
        return (TResult) this;
    }

    public TResult WithError(Error? error)
    {
        Error = error;
        return (TResult) this;
    }
}