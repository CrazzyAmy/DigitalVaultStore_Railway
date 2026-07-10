// Middleware/GlobalExceptionMiddleware.cs
using DigitalProject.Response;
using DigitalProject.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;

namespace DigitalProject.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly JsonSerializerOptions _jsonOptions;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IWebHostEnvironment env,
            IOptions<JsonOptions> jsonOptions)
        {
            _next = next;
            _logger = logger;
            _env = env;
            _jsonOptions = jsonOptions.Value.JsonSerializerOptions;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發生未處理的例外: {Message}", ex.ToString());
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            // 根據例外類型決定狀態碼和訊息
            var (statusCode, message) = exception switch
            {
                AppException ex => (ex.StatusCode, ex.Message),
                UnauthorizedAccessException => (401, "未授權，請先登入"),
                KeyNotFoundException => (404, "找不到資源"),
                ArgumentException ex => (400, ex.Message),
                _ => (500, "伺服器發生錯誤，請稍後再試")
            };

            context.Response.StatusCode = statusCode;

            var errorResponse = new ErrorResponse(
                message: message,
                path: context.Request.Path.ToString(),
                statusCode: statusCode
            );

            // 開發環境才顯示詳細錯誤
            if (_env.IsDevelopment() && statusCode == 500)
            {
                errorResponse.Details = $"{exception.Message}\n\nStackTrace:\n{exception.StackTrace}";
            }

            var jsonResponse = JsonSerializer.Serialize(errorResponse, _jsonOptions);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}