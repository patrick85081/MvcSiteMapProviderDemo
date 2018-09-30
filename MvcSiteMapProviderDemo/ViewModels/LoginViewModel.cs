using System.ComponentModel.DataAnnotations;

namespace MvcSiteMapProviderDemo.ViewModels
{
    public class LoginViewModel
    {
        [Display(Name = "信箱")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Display(Name = "密碼")]
        public string Password { get; set; }
    }
}