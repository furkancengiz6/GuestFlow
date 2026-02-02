using System;

namespace GuestFlow.Application
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public object? Errors { get; set; }
        public int StatusCode { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResponse<T> SuccessResponse(T? data = default, string message = "Success")
        {
            return new ApiResponse<T> { Success = true, Message = message, Data = data, StatusCode = 200, Timestamp = DateTime.UtcNow };
        }

        public static ApiResponse<T> ErrorResponse(string message, int statusCode = 400, object? errors = null)
        {
            return new ApiResponse<T> { Success = false, Message = message, StatusCode = statusCode, Errors = errors, Timestamp = DateTime.UtcNow };
        }

        public static ApiResponse<T> NotFoundResponse(string message = "Resource not found")
        {
            return new ApiResponse<T> { Success = false, Message = message, StatusCode = 404, Timestamp = DateTime.UtcNow };
        }

        public static ApiResponse<T> UnauthorizedResponse(string message = "Unauthorized access")
        {
            return new ApiResponse<T> { Success = false, Message = message, StatusCode = 401, Timestamp = DateTime.UtcNow };
        }

        public static ApiResponse<T> Fail(string message, int statusCode = 400, object? errors = null)
            => ErrorResponse(message, statusCode, errors);
    }

    public class ApiResponse : ApiResponse<object>
    {
    }
}

