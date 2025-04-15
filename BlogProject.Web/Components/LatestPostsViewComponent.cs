using BlogProject.Business.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace BlogProject.Web.Components
{
    public class LatestPostsViewComponent : ViewComponent
    {
        private readonly IBlogService _blogService;

        public LatestPostsViewComponent(IBlogService blogService)
        {
            _blogService = blogService;
        }

        public async Task<IViewComponentResult> InvokeAsync(int count = 5)
        {
            var blogs = await _blogService.GetAllAsync();
            var latestBlogs = blogs
                .Where(b => b.IsActive)
                .OrderByDescending(b => b.PublishDate)
                .Take(count)
                .ToList();
            return View(latestBlogs);
        }
    }
}
