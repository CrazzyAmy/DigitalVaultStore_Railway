// Request/UpdateUserRoleRequest.cs
using System.ComponentModel.DataAnnotations;

namespace DigitalProject.Request
{
    public class UpdateUserRoleRequest
    {
        [Required]
        public string RoleCode { get; set; } = null!;  // "admin" / "manager" / "support" / "user"
    }
}