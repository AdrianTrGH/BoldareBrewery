namespace BoldareBrewery.Application.Common
{
    public class Error
    {
        public string Code { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public string? Details { get; init; }

        public static Error None => new();
        public static Error NotFound(string message = "Resource not found") =>
            new() { Code = "NotFound", Message = message };
        public static Error ValidationFailure(string message) =>
            new() { Code = "ValidationFailure", Message = message };
        public static Error InternalServerError(string message = "Internal server error") =>
            new() { Code = "InternalServerError", Message = message };
        public static Error ExternalServiceFailure(string message) =>
            new() { Code = "ExternalServiceFailure", Message = message };

        public static Error Custom(string code, string message, string? details = null) =>
            new() { Code = code, Message = message, Details = details };
    }
}
