using BlogProject.Business.Abstract;
using BlogProject.Entity;
using BlogProject.Web.HtmlHelpers;
using BlogProject.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace BlogProject.Web.Controllers
{
    public class BlogController : Controller
    {
        private readonly IBlogService _blogService;
        private readonly ICategoryService _categoryService;
        private readonly ICommentService _commentService;
        private readonly IUserService _userService;

        public BlogController(IBlogService blogService , ICategoryService categoryService, ICommentService commentService, IUserService userService)
        {
            _blogService = blogService;
            _categoryService = categoryService;
            _commentService = commentService;
            _userService = userService;
        }

        public async Task<IActionResult> Index(string? query, int? categoryId, DateTime? startDate, DateTime? endDate, int page = 1, int pageSize = 3)
        {
            var allBlogs = await _blogService.GetAllWithCategoriesAsync();

            var activeBlogs = allBlogs
                .Where(b => b.IsActive);

            if (!string.IsNullOrWhiteSpace(query))
                activeBlogs = activeBlogs.Where(b => b.Title.Contains(query, StringComparison.OrdinalIgnoreCase) || b.Content.Contains(query, StringComparison.OrdinalIgnoreCase));

            if (categoryId.HasValue)
                activeBlogs = activeBlogs.Where(b => b.Categories.Any(c => c.Id == categoryId));

            if (startDate.HasValue)
                activeBlogs = activeBlogs.Where(b => b.PublishDate >= startDate);

            if (endDate.HasValue)
                activeBlogs = activeBlogs.Where(b => b.PublishDate <= endDate);

            var filteredBlogs = activeBlogs.OrderByDescending(b => b.PublishDate).ToList();

            var pagedBlogs = filteredBlogs
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)filteredBlogs.Count / pageSize);
            ViewBag.SelectedCategory = null;
            ViewBag.CategoryId = categoryId;
            ViewBag.Query = query;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.Categories = await _categoryService.GetAllAsync();

            return View(pagedBlogs);
        }



        [HttpGet("Blog/{slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            var blog = await _blogService.GetBlogBySlugAsync(slug);
            if (blog == null)
            {
                return NotFound();
            }

            blog.Comments = await _commentService.GetCommentsByBlogIdAsync(blog.Id);

            string plainText = Regex.Replace(blog.Content, "<.*?>", string.Empty);
            ViewData["MetaDescription"] = plainText.Length > 150
                ? plainText.Substring(0, 150) + "..."
                : plainText;

            return View(blog);
        }

        [HttpGet("Blog/Category/{id}")]
        public async Task<IActionResult> Category(int id, int page = 1, int pageSize = 3)
        {
            var allBlogs = await _blogService.GetByCategoryIdAsync(id);
            var activeBlogs = allBlogs.Where(b => b.IsActive)
                .OrderByDescending(b => b.PublishDate)
                .ToList();

            var pagedBlogs = activeBlogs
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var category = await _categoryService.GetByIdAsync(id);

            ViewBag.SelectedCategory = category?.Name ?? "Kategori";
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)activeBlogs.Count / pageSize);
            ViewBag.CategoryId = id;
            ViewBag.Query = "";
            ViewBag.StartDate = null;
            ViewBag.EndDate = null;

            ViewBag.Categories = await _categoryService.GetAllAsync(); 

            return View("Index", pagedBlogs);
        }


        [HttpPost("Blog/AddComment")]
        public async Task<IActionResult> AddComment(int blogId, string content)
        {
            if (!string.IsNullOrWhiteSpace(content) && User.Identity?.IsAuthenticated == true)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var user = await _userService.GetByIdAsync(userId);

                var comment = new Comment
                {
                    BlogId = blogId,
                    Content = content,
                    CommentDate = DateTime.Now,
                    UserId = userId
                };

                await _commentService.CreateAsync(comment);

                return Json(new
                {
                    Id = comment.Id,
                    Content = comment.Content,
                    CommentDate = comment.CommentDate,
                    User = new
                    {
                        user.UserName,
                        user.ImagePath
                    },
                    IsOwner = true,
                    IsAdmin = User.IsInRole("Admin")
                });
            }

            return Unauthorized();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null)
                return NotFound();

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var isAdmin = User.IsInRole("Admin");

            if (comment.UserId != currentUserId && !isAdmin)
                return Unauthorized();

            await _commentService.DeleteAsync(comment);

            return Ok(); 
        }


        [HttpPost]
        public async Task<IActionResult> EditComment(int id, string content)
        {
            var comment = await _commentService.GetByIdAsync(id);
            if (comment == null) return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var isAdmin = User.IsInRole("Admin");

            if (comment.UserId != userId && !isAdmin)
                return Unauthorized();

            comment.Content = content;
            await _commentService.UpdateAsync(comment);

            return Ok();
        }


        [HttpGet("Blog/Create")]
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryService.GetAllAsync();
            ViewBag.Categories = new MultiSelectList(categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(BlogCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _categoryService.GetAllAsync();
                ViewBag.Categories = new MultiSelectList(categories, "Id", "Name", model.SelectedCategoryIds);
                return View(model);
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var newBlog = new Blog
            {
                Title = model.Title,
                Content = model.Content,
                PublishDate = DateTime.Now,
                ViewCount = 0,
                ImagePath = model.Image != null ? SaveImage(model.Image) : "default-blog.jpg",
                UserId = userId,
                Slug = SlugHelper.GenerateSlug(model.Title),
                IsActive = false 
            };

            newBlog.Categories = new List<Category>();
            foreach (var categoryId in model.SelectedCategoryIds)
            {
                var category = await _categoryService.GetByIdAsync(categoryId);
                if (category != null)
                    newBlog.Categories.Add(category);
            }

            await _blogService.AddAsync(newBlog);

            return RedirectToAction("Index");
        }

        private string SaveImage(IFormFile file)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);
            using var stream = new FileStream(path, FileMode.Create);
            file.CopyTo(stream);
            return fileName;
        }

        [HttpGet("Blog/Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var blog = await _blogService.GetBlogWithUserAndCategoriesAsync(id);
            if (blog == null) return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var isAdmin = User.IsInRole("Admin");

            if (blog.UserId != userId && !isAdmin)
                return Unauthorized();

            ViewBag.Categories = await _categoryService.GetAllAsync();
            return View(blog);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Blog model, IFormFile? imageFile, int[] selectedCategoryIds)
        {
            var existingBlog = await _blogService.GetBlogWithUserAndCategoriesAsync(model.Id);
            if (existingBlog == null) return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var isAdmin = User.IsInRole("Admin");

            if (existingBlog.UserId != userId && !isAdmin)
                return Unauthorized();

            existingBlog.Title = model.Title;
            existingBlog.Content = model.Content;
            existingBlog.PublishDate = DateTime.Now;

            if (imageFile != null)
            {
                var imagePath = Path.Combine("wwwroot/img", imageFile.FileName);
                using var stream = new FileStream(imagePath, FileMode.Create);
                await imageFile.CopyToAsync(stream);
                existingBlog.ImagePath = imageFile.FileName;
            }

            var allCategories = await _categoryService.GetAllAsync();
            existingBlog.Categories = allCategories.Where(c => selectedCategoryIds.Contains(c.Id)).ToList();

            await _blogService.UpdateAsync(existingBlog);

            return RedirectToAction("Details", new { slug = existingBlog.Slug });
        }

        [HttpGet("Blog/MyBlogs")]
        public async Task<IActionResult> MyBlogs()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var isAdmin = User.IsInRole("Admin");

            var allBlogs = await _blogService.GetAllWithCategoriesAsync();

            var filtered = isAdmin
                ? allBlogs.OrderByDescending(b => b.PublishDate).ToList()
                : allBlogs.Where(b => b.UserId == userId).OrderByDescending(b => b.PublishDate).ToList();

            return View(filtered);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var blog = await _blogService.GetByIdAsync(id);
            if (blog == null) return NotFound();

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var isAdmin = User.IsInRole("Admin");

            if (blog.UserId != userId && !isAdmin)
                return Unauthorized();

            _blogService.Delete(blog);
            return RedirectToAction("MyBlogs");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var blog = await _blogService.GetByIdAsync(id);
            if (blog == null) return NotFound();

            if (!User.IsInRole("Admin"))
                return Unauthorized();

            blog.IsActive = !blog.IsActive;
            await _blogService.UpdateAsync(blog);
            return Redirect(Request.Headers["Referer"].ToString());
        }

        [HttpGet("/Blog/Search")]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Json(new List<object>());

            var allBlogs = await _blogService.GetAllWithCategoriesAsync();

            var filtered = allBlogs
                .Where(b => b.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                            b.Content.Contains(query, StringComparison.OrdinalIgnoreCase))
                .Select(b => new
                {
                    title = b.Title,
                    slug = b.Slug,
                    image = b.ImagePath,
                    categories = b.Categories.Select(c => c.Name)
                })
                .Take(8)
                .ToList();

            return Json(filtered);
        }

        [HttpGet("/Blog/GetLatestComments")]
        public async Task<IActionResult> GetLatestComments()
        {
            var comments = await _commentService.GetAllWithBlogAndUserAsync();

            var latest = comments
                .OrderByDescending(c => c.CommentDate)
                .Take(5)
                .Select(c => new
                {
                    content = c.Content,
                    date = c.CommentDate.ToString("dd MMM yyyy HH:mm"),
                    blogTitle = c.Blog?.Title,
                    blogSlug = c.Blog?.Slug,
                    userImage = c.User?.ImagePath ?? "default.png",
                    username = c.User?.UserName
                }).ToList();

            return Json(latest);
        }

        [HttpPost]
        public async Task<IActionResult> AddReplyComment(CommentViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.Content) && User.Identity?.IsAuthenticated == true)
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var comment = new Comment
                {
                    BlogId = model.BlogId,
                    Content = model.Content,
                    ParentCommentId = model.ParentCommentId,
                    CommentDate = DateTime.Now,
                    UserId = userId
                };

                await _commentService.AddCommentAsync(comment);

                var blog = await _blogService.GetByIdAsync(model.BlogId);
                return RedirectToAction("Details", "Blog", new { slug = blog.Slug });
            }

            return Unauthorized();
        }

    }
}
