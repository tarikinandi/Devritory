using BlogProject.DataAccess.Concrete.Context;
using BlogProject.Entity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace BlogProject.DataAccess.Concrete.Seed
{
    public static class SeedData
    {
        public const int MinAdminPasswordLength = 10;

        public static void Initialize(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var sp = scope.ServiceProvider;
            var context = sp.GetRequiredService<AppDbContext>();

            context.Database.Migrate();

            SeedCategories(context);
            SeedAdmin(context,
                sp.GetRequiredService<IConfiguration>(),
                sp.GetRequiredService<IPasswordHasher<User>>(),
                sp.GetRequiredService<ILoggerFactory>().CreateLogger("SeedData"));
            SeedSampleBlogs(context);
        }

        public static void SeedCategories(AppDbContext context)
        {
            if (context.Categories.Any()) return;
            context.Categories.AddRange(
                new Category { Name = "Yazılım" },
                new Category { Name = "Teknoloji" },
                new Category { Name = "Kariyer" });
            context.SaveChanges();
        }

        /// <summary>
        /// Creates the initial admin only from configuration (SeedAdmin:Email, SeedAdmin:UserName,
        /// SeedAdmin:Password). Nothing is created when it is not configured, when the password is
        /// too short, or when a user with that e-mail already exists. The password is never logged.
        /// </summary>
        public static bool SeedAdmin(AppDbContext context, IConfiguration config, IPasswordHasher<User> hasher, ILogger? logger = null)
        {
            var email = config["SeedAdmin:Email"]?.Trim();
            var userName = config["SeedAdmin:UserName"]?.Trim();
            var password = config["SeedAdmin:Password"];

            if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(userName) && string.IsNullOrEmpty(password))
                return false;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(userName) || string.IsNullOrEmpty(password))
            {
                logger?.LogWarning("SeedAdmin is partly configured (Email, UserName and Password are all required); admin not created.");
                return false;
            }

            if (password.Length < MinAdminPasswordLength)
            {
                logger?.LogWarning("SeedAdmin:Password is shorter than {Min} characters; admin not created.", MinAdminPasswordLength);
                return false;
            }

            var lowerEmail = email.ToLower();
            var lowerName = userName.ToLower();
            if (context.Users.Any(u => u.Email.ToLower() == lowerEmail || u.UserName.ToLower() == lowerName))
                return false;

            var admin = new User { UserName = userName, Email = email, ImagePath = "default.png", Role = "Admin" };
            admin.Password = hasher.HashPassword(admin, password);
            context.Users.Add(admin);
            context.SaveChanges();
            logger?.LogInformation("Initial admin user created from configuration.");
            return true;
        }

        private static void SeedSampleBlogs(AppDbContext context)
        {
            if (context.Blogs.Any()) return;
            var author = context.Users.FirstOrDefault(u => u.Role == "Admin");
            if (author == null) return; // sample posts need an author; there is no built-in account any more

            {
                var user1 = author;
                var user2 = author;
                var categories = context.Categories.ToList();

                string GenerateSlug(string title)
                {
                    var slug = title.ToLowerInvariant();
                    slug = Regex.Replace(slug, @"[^a-z0-9\s-]", ""); 
                    slug = Regex.Replace(slug, @"\s+", " ").Trim(); 
                    slug = slug.Replace(" ", "-");
                    return slug;
                }

                var blogs = new List<Blog>
    {
        new Blog
        {
            Title = "ASP.NET Core Nedir?",
            Slug = GenerateSlug("ASP.NET Core Nedir?"),
            Content = "Modern web uygulamaları için framework.",
            PublishDate = DateTime.Now,
            ViewCount = 50,
            ImagePath = "blog1.jpg",
            UserId = user1.Id,
            IsActive = true,
            Categories = new List<Category> { categories[0], categories[1] }
        },
        new Blog
        {
            Title = "Entity Framework Core İlişkiler",
            Slug = GenerateSlug("Entity Framework Core İlişkiler"),
            Content = "One-to-many, many-to-many ilişkiler nasıl kurulur?",
            PublishDate = DateTime.Now.AddDays(-1),
            ViewCount = 30,
            ImagePath = "blog2.jpg",
            UserId = user2.Id,
            IsActive = true,
            Categories = new List<Category> { categories[0] }
        },
        new Blog
        {
            Title = "Yazılımcı Olmanın Zorlukları",
            Slug = GenerateSlug("Yazılımcı Olmanın Zorlukları"),
            Content = "Freelance mi, kurumsal mı? Tecrübelerim.",
            PublishDate = DateTime.Now.AddDays(-2),
            ViewCount = 80,
            ImagePath = "blog3.jpg",
            UserId = user1.Id,
            IsActive = true,
            Categories = new List<Category> { categories[2] }
        },
        new Blog
        {
            Title = "Dependency Injection Nedir?",
            Slug = GenerateSlug("Dependency Injection Nedir?"),
            Content = "ASP.NET Core'da bağımlılıkları nasıl çözümleyeceğiz?",
            PublishDate = DateTime.Now.AddDays(-3),
            ViewCount = 25,
            ImagePath = "blog4.jpg",
            UserId = user2.Id,
            IsActive = true,
            Categories = new List<Category> { categories[0], categories[2] }
        },
        new Blog
        {
            Title = "2025'te Web Geliştirici Olmak",
            Slug = GenerateSlug("2025'te Web Geliştirici Olmak"),
            Content = "Yeni teknolojiler, trendler, gelişmeler.",
            PublishDate = DateTime.Now.AddDays(-5),
            ViewCount = 120,
            ImagePath = "blog5.jpg",
            UserId = user1.Id,
            IsActive = true,
            Categories = new List<Category> { categories[1] }
        }
    };

                context.Blogs.AddRange(blogs);
                context.SaveChanges();
            }
        }
    }
}
