using DigitalProject.Models;
using DigitalProject.Response;

namespace DigitalProject.Security
{
    public interface IJwtHelper 
    {
        AuthResponse GenerateToken(User user);
    }
}
