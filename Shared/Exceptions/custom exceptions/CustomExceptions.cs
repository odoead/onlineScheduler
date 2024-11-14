namespace Shared.Exceptions.custom_exceptions
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string message = null) : base(message ?? "Bad Request") { }
    }

    public class NotFoundException : Exception
    {
        public NotFoundException(string message = null) : base(message ?? "Data Not Found") { }
    }

    public class ValidationException : Exception
    {
        public ValidationException(string message = null) : base(message ?? "Validation Error") { }
    }
}
