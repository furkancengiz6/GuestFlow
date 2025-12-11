using System;

namespace GuestFlow.Api.Models
{
    /// <summary>
    /// Standart API yanıt formatı
    /// </summary>
    /// <typeparam name="T">Yanıt verisi tipi</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// İşlemin başarılı olup olmadığı
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Yanıt mesajı
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Yanıt verisi
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Hata detayları (sadece hata durumunda)
        /// </summary>
        public object? Errors { get; set; }

        /// <summary>
        /// HTTP durum kodu
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// İşlem zamanı
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Başarılı yanıt oluşturur
        /// </summary>
        public static ApiResponse<T> SuccessResponse(T? data = default, string message = "İşlem başarılı.")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data,
                StatusCode = 200,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Hata yanıtı oluşturur
        /// </summary>
        public static ApiResponse<T> ErrorResponse(string message, int statusCode = 400, object? errors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default,
                Errors = errors,
                StatusCode = statusCode,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Bulunamadı yanıtı oluşturur
        /// </summary>
        public static ApiResponse<T> NotFoundResponse(string message = "Kayıt bulunamadı.")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default,
                StatusCode = 404,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Yetkisiz erişim yanıtı oluşturur
        /// </summary>
        public static ApiResponse<T> UnauthorizedResponse(string message = "Yetkisiz erişim.")
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Data = default,
                StatusCode = 401,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Veri olmayan API yanıtı için
    /// </summary>
    public class ApiResponse : ApiResponse<object>
    {
        /// <summary>
        /// Başarılı yanıt oluşturur
        /// </summary>
        public static ApiResponse SuccessResponse(string message = "İşlem başarılı.")
        {
            return new ApiResponse
            {
                Success = true,
                Message = message,
                Data = null,
                StatusCode = 200,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Hata yanıtı oluşturur
        /// </summary>
        public static new ApiResponse ErrorResponse(string message, int statusCode = 400, object? errors = null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                Data = null,
                Errors = errors,
                StatusCode = statusCode,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Bulunamadı yanıtı oluşturur
        /// </summary>
        public static new ApiResponse NotFoundResponse(string message = "Kayıt bulunamadı.")
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                Data = null,
                StatusCode = 404,
                Timestamp = DateTime.UtcNow
            };
        }

        /// <summary>
        /// Yetkisiz erişim yanıtı oluşturur
        /// </summary>
        public static new ApiResponse UnauthorizedResponse(string message = "Yetkisiz erişim.")
        {
            return new ApiResponse
            {
                Success = false,
                Message = message,
                Data = null,
                StatusCode = 401,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}

