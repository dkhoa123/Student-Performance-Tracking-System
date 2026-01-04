using System.ComponentModel.DataAnnotations;


namespace SPTS_Service.ViewModel
{
    public class DangNhapModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";

        public bool RememberMe { get; set; }

    }
}
