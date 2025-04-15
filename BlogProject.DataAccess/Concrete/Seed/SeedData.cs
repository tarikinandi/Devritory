using BlogProject.DataAccess.Concrete.Context;
using BlogProject.Entity;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace BlogProject.DataAccess.Concrete.Seed
{
    public static class SeedData
    {
        public static void Initialize(IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            context.Database.Migrate();

            if (!context.Categories.Any())
            {
                var cat1 = new Category { Name = "Yazılım" };
                var cat2 = new Category { Name = "Teknoloji" };
                var cat3 = new Category { Name = "Kariyer" };
                context.Categories.AddRange(cat1, cat2, cat3);
                context.SaveChanges();
            }

            if (!context.Users.Any())
            {
                var user1 = new User { UserName = "admin", Email = "admin@blog.com", Password = "123456", ImagePath = "default.png" , Role="Admin"};
                var user2 = new User { UserName = "ahmet", Email = "ahmet@example.com", Password = "123456", ImagePath = "default.png" };
                context.Users.AddRange(user1, user2);
                context.SaveChanges();
            }

            if (!context.Blogs.Any())
            {
                var user1 = context.Users.First();
                var user2 = context.Users.Skip(1).First();
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
            Categories = new List<Category> { categories[0], categories[1] },
            Comments = new List<Comment>
            {
                new Comment { Content = "Çok faydalı içerik.", CommentDate = DateTime.Now, UserId = user2.Id }
            }
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
