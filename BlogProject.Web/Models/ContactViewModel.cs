using System.ComponentModel.DataAnnotations;

namespace BlogProject.Web.Models
{
    public class ContactViewModel
    {
        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, StringLength(150)]
        public string Subject { get; set; }

        [Required, StringLength(1000)]
        public string Message { get; set; }
    }
}
