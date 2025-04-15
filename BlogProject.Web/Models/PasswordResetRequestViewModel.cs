using System.ComponentModel.DataAnnotations;

namespace BlogProject.Web.Models
{
    public class PasswordResetRequestViewModel
    {
        [Required(ErrorMessage = "E-posta adresi gereklidir.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string Email { get; set; }
    }
}
