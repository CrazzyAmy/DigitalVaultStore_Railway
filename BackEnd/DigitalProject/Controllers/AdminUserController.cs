// Controllers/AdminUserController.cs
using DigitalProject.Interface.User;
using DigitalProject.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalProject.Controllers
{
    [ApiController]
    [Route("api/admin/user")]
    [Authorize(Policy = "CanManageUser")]
    public class AdminUserController : BaseController
    {
        private readonly IUserService _userService;

        public AdminUserController(IUserService userService)
        {
            _userService = userService;
        }

        // GET /api/admin/user
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PagedRequest request)
        {
            var result = await _userService.GetAllAsync(request);
            return Ok(result);
        }

        // GET /api/admin/user/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _userService.GetByIdAsync(id);
            return Ok(user);
        }

        // PUT /api/admin/user/{id}/deactivate
        [HttpPut("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            await _userService.DeactivateAsync(id);
            return Ok(new { message = "帳號已停用" });
        }

        // PUT /api/admin/user/{id}/activate
        [HttpPut("{id}/activate")]
        public async Task<IActionResult> Activate(Guid id)
        {
            await _userService.ActivateAsync(id);
            return Ok(new { message = "帳號已啟用" });
        }

        // PUT /api/admin/user/{id}/role
        [HttpPut("{id}/role")]
        public async Task<IActionResult> UpdateRole(
            Guid id, [FromBody] UpdateUserRoleRequest request)
        {
            await _userService.UpdateRoleAsync(id, request);
            return Ok(new { message = "角色已更新" });
        }
    }
}