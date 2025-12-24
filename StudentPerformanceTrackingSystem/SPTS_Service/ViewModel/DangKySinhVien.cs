using System.ComponentModel.DataAnnotations;

namespace SPTS_Service.ViewModel
{
    public class DangKySinhVien
    {
            [Required]
            public string FullName { get; set; } = null!;

            [Required, EmailAddress]
            public string Email { get; set; } = null!;
            [Required]

            public string? Major { get; set; }
            public int? CohortYear { get; set; }

            [Required, DataType(DataType.Password)]
            public string Password { get; set; } = null!;

            [Required, DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
            public string ConfirmPassword { get; set; } = null!;

    }
}
