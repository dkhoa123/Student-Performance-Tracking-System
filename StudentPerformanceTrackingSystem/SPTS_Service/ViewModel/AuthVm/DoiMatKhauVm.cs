using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPTS_Service.ViewModel.AuthVm
{
    public class DoiMatKhauVm
    {
        [Required] public string OldPassword { get; set; } = "";
        [Required, MinLength(6)] public string NewPassword { get; set; } = "";
        [Required] public string ConfirmPassword { get; set; } = "";
    }
}
