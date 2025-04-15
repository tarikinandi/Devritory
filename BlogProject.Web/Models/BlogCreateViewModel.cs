using System.ComponentModel.DataAnnotations;

namespace BlogProject.Web.Models
{
    public class BlogCreateViewModel
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public IFormFile? Image { get; set; }

        [Required]
        public List<int> SelectedCategoryIds { get; set; }
    }
}
