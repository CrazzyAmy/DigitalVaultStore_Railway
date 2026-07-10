using System.ComponentModel.DataAnnotations;

public class UpdateDisplayNameRequest
{
    [Required]
    [MaxLength(100, ErrorMessage = "顯示名稱最多 100 字")]
    public string DisplayName { get; set; } = string.Empty;
}
