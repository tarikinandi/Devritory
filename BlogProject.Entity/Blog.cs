using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogProject.Entity
{
    public class Blog
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime PublishDate { get; set; }
        public int ViewCount { get; set; } = 0;
        public string? ImagePath { get; set; }
        public bool IsActive { get; set; } = false;
        public int UserId { get; set; }
        public string Slug { get; set; }

        public User User { get; set; }
        public int CategoryId { get; set; }
        public ICollection<Category> Categories { get; set; } = new List<Category>();

        public ICollection<Comment> Comments { get; set; }
    }
}
