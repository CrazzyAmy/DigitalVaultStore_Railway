// Controllers/UserController.cs
using DigitalProject.Exceptions;
using DigitalProject.Interface.User;
using DigitalProject.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalProject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // PUT /api/user/displayName
        [HttpPut("displayName")]
        public async Task<IActionResult> UpdateDisplayName(
            [FromBody] UpdateDisplayNameRequest request)
        {
            var userId = GetUserId()!.Value;
            await _userService.UpdateDisplayNameAsync(userId, request);
            return Ok(new { message = "顯示名稱更新成功" });
        }

        // PUT /api/user/password
        [HttpPut("password")]
        public async Task<IActionResult> UpdatePassword(
            [FromBody] UpdatePasswordRequest request)
        {
            var userId = GetUserId()!.Value;
            await _userService.UpdatePasswordAsync(userId, request);
            return Ok(new { message = "密碼修改成功" });
        }

        // GET /api/user/purchases
        [HttpGet("purchases")]
        public async Task<IActionResult> GetPurchases()
        {
            var userId = GetUserId()!.Value;
            var purchases = await _userService.GetPurchasesAsync(userId);
            return Ok(purchases);
        }

        // PUT /api/user/avatar
        [HttpPut("avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new AppException("請選擇圖片檔案", 400);

            var userId = GetUserId()!.Value;
            var avatarUrl = await _userService.UploadAvatarAsync(userId, file);
            return Ok(new { avatarUrl });
        }
    }
}