using BlogProject.Entity;
using System.ComponentModel.DataAnnotations;

namespace BlogProject.Web.Models
{
    public class BlogEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Başlık zorunludur.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "İçerik boş olamaz.")]
        public string Content { get; set; }

        public string? ImagePath { get; set; }

        public List<int> SelectedCategoryIds { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
    }
}
