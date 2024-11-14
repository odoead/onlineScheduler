namespace Shared.Exceptions
{
    public class ApiErrorResponse
    {
        public int StatusCode { get; set; } = 500;
        public string Message { get; set; } = "";
        public string Details { get; set; } = "";
    }
}
