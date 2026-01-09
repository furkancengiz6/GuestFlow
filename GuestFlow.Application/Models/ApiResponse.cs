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
        // Keep only SuccessResponse to avoid name collision with property 'Success'

        public static ApiResponse<T> Fail(string message, int statusCode = 400, object? errors = null)
            => new ApiResponse<T> { Success = false, Message = message, Errors = errors, StatusCode = statusCode, Timestamp = DateTime.UtcNow };
    }

    public class ApiResponse : ApiResponse<object>
    {
    }
}

