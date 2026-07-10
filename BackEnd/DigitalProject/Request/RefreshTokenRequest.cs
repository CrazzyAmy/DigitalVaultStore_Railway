// Request/RefreshTokenRequest.cs
namespace DigitalProject.Request
{
    public class RefreshTokenRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
        public string OldLoginToken { get; set; } = string.Empty;
    }
}