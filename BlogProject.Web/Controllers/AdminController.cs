using BlogProject.Business.Abstract;
using BlogProject.Entity;
using BlogProject.Web.DTOs;
using BlogProject.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace BlogProject.Web.Controllers
{
    [Authorize(Roles = "Admin")] 
    [Route("Admin")]
    public class AdminController : Controller
    {
        private readonly IBlogService _blogService;
        private readonly ICommentService _commentService;
        private readonly ICategoryService _categoryService;
        private readonly IUserService _userService;
        public AdminController(IBlogService blogService , ICommentService commentService, ICategoryService categoryService, IUserService userService)
        {
            _blogService = blogService;
            _commentService = commentService;
            _categoryService = categoryService;
            _userService = userService;
        }

        [HttpGet("Blogs")]
        public async Task<IActionResult> Blogs()
        {
            var blogs = await _blogService.GetAllWithCategoriesAsync();
            return View(blogs.OrderByDescending(b => b.PublishDate).ToList());
        }

        [HttpGet("Comments")]
        public async Task<IActionResult> Comments()
        {
            var comments = await _commentService.GetAllWithBlogAndUserAsync();
            return View(comments.OrderByDescending(c => c.CommentDate).ToList());
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null) return NotFound();

            await _commentService.DeleteAsync(comment);
            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpGet("Categories")]
        public async Task<IActionResult> Categories()
        {
            var categories = await _categoryService.GetAllAsync();
            return View(categories.OrderByDescending(c => c.Id).ToList());
        }


        [HttpPost("CreateCategoryAjax")]
        public async Task<IActionResult> CreateCategoryAjax(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                await _categoryService.CreateAsync(new Category { Name = name });
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }


        [HttpPost("EditCategoryAjax")]
        public async Task<IActionResult> EditCategoryAjax(int id, string name)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category != null && !string.IsNullOrWhiteSpace(name))
            {
                category.Name = name;
                await _categoryService.UpdateAsync(category);
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpPost("DeleteCategoryAjax")]
        public async Task<IActionResult> DeleteCategoryAjax(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category != null)
            {
               await _categoryService.DeleteAsync(category);
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        [HttpGet("Users")]
        public async Task<IActionResult> Users()
        {
            var users = await _userService.GetAllAsync();
            return View(users.OrderBy(u => u.UserName).ToList());
        }

        [HttpPost("ToggleUserRoleAjax")]
        public async Task<IActionResult> ToggleUserRoleAjax(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
                return Json(new { success = false });

            user.Role = user.Role == "Admin" ? "User" : "Admin";
            await _userService.UpdateAsync(user);
            return Json(new { success = true, newRole = user.Role });
        }


        [HttpPost("DeleteUser")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userService.GetByIdAsync(id);
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (user == null || user.Id == currentUserId)
                return Unauthorized(); 

            await _userService.DeleteAsync(user);
            return RedirectToAction("Users");
        }

        [HttpGet("Dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            var users = await _userService.GetAllAsync();
            var blogs = await _blogService.GetAllWithCategoriesAsync();
            var comments = await _commentService.GetAllAsync();
            var categories = await _categoryService.GetAllAsync();

            var viewModel = new DashboardViewModel
            {
                TotalUsers = users.Count,
                TotalBlogs = blogs.Count,
                TotalComments = comments.Count,
                TotalCategories = categories.Count,
                TotalViewCount = blogs.Sum(b => b.ViewCount),

                LatestBlogs = blogs.OrderByDescending(b => b.PublishDate).Take(5).ToList(),
                LatestComments = comments.OrderByDescending(c => c.CommentDate).Take(5).ToList(),
                LatestUsers = users.OrderByDescending(u => u.Id).Take(5).ToList(),

                CategoryBlogCounts = blogs
                    .SelectMany(b => b.Categories)
                    .GroupBy(c => c.Name)
                    .Select(g => new CategoryBlogCountDto
                    {
                        CategoryName = g.Key,
                        BlogCount = g.Count()
                    }).ToList(),

                TopViewedBlogs = blogs
                    .OrderByDescending(b => b.ViewCount)
                    .Take(5)
                    .Select(b => new BlogViewCountDto
                    {
                        BlogTitle = b.Title,
                        ViewCount = b.ViewCount
                    }).ToList(),

                DailyBlogCounts = blogs
                    .GroupBy(b => b.PublishDate.Date)
                    .Select(g => new DailyBlogCountDto
                    {
                        Date = g.Key,
                        Count = g.Count()
                    }).OrderBy(d => d.Date).ToList()
            };

            return View(viewModel);
        }

    }
}
