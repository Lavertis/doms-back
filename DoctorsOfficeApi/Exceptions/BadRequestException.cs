namespace DoctorsOfficeApi.Exceptions;

public class BadRequestException : AppException
{
    public BadRequestException(string message) : base(message)
    {
    }
}