using System.ComponentModel.DataAnnotations;

namespace BlogProject.Web.Models
{
    public class SettingsViewModel
    {
        [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "Ad Soyad en fazla 50 karakter olabilir.")]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; }

        [Display(Name = "Profil Resmi")]
        public IFormFile? ProfileImage { get; set; }

        public string? ExistingImagePath { get; set; }
    }
}
