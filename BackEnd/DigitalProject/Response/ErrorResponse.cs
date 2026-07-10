namespace DigitalProject.Response
{
    
        public class ErrorResponse
        {
            public int StatusCode { get; set; }
            public string Message { get; set; } = null!;
            public string Path { get; set; } = null!;
            public DateTime Timestamp { get; set; } = DateTime.UtcNow;
            public string? Details { get; set; }  // 只有開發環境才填

            public ErrorResponse(string message, string path, int statusCode)
            {
                Message = message;
                Path = path;
                StatusCode = statusCode;
            }
        }
    }

