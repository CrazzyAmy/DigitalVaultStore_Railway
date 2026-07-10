using Microsoft.AspNetCore.Mvc;

namespace DigitalProject.Request
{
    public class RegisterRequest
    {
        public string Email { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
