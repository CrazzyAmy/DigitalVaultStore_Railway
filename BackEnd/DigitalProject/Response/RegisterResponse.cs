using DigitalProject.Domain;
using Microsoft.AspNetCore.Mvc;

namespace DigitalProject.Response
{
    public class RegisterResponse
    {
        public string Message { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
    }
}
