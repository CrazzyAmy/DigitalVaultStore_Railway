// UpdatePasswordRequest 補上驗證
using System.ComponentModel.DataAnnotations;

public class UpdatePasswordRequest
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required]
    [MinLength(8, ErrorMessage = "新密碼至少 8 個字元")]
    public string NewPassword { get; set; } = string.Empty;
}