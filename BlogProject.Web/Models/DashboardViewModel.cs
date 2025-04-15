using BlogProject.Entity;
using BlogProject.Web.DTOs;

namespace BlogProject.Web.Models
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalBlogs { get; set; }
        public int TotalComments { get; set; }
        public int TotalCategories { get; set; }
        public int TotalViewCount { get; set; }

        public List<Blog> LatestBlogs { get; set; }
        public List<Comment> LatestComments { get; set; }
        public List<User> LatestUsers { get; set; }

        public List<CategoryBlogCountDto> CategoryBlogCounts { get; set; } 
        public List<BlogViewCountDto> TopViewedBlogs { get; set; } 
        public List<DailyBlogCountDto> DailyBlogCounts { get; set; } 
    }

}
