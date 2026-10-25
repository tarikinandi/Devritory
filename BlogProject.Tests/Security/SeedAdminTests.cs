using BlogProject.DataAccess.Concrete.Context;
using BlogProject.DataAccess.Concrete.Seed;
using BlogProject.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BlogProject.Tests.Security
{
    public class SeedAdminTests
    {
        private static AppDbContext NewContext() =>
            new(new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);

        private static IConfiguration Config(params (string Key, string? Value)[] values) =>
            new ConfigurationBuilder()
                .AddInMemoryCollection(values.Select(v => new KeyValuePair<string, string?>(v.Key, v.Value)))
                .Build();

        private static readonly PasswordHasher<User> Hasher = new();
        private const string Pw = "a-long-test-password";

        [Fact]
        public void NotConfigured_CreatesNoUser()
        {
            using var ctx = NewContext();

            var created = SeedData.SeedAdmin(ctx, Config(), Hasher);

            Assert.False(created);
            Assert.Empty(ctx.Users);
        }

        [Fact]
        public void Configured_CreatesAdmin_WithHashedPassword()
        {
            using var ctx = NewContext();

            var created = SeedData.SeedAdmin(ctx, Config(
                ("SeedAdmin:Email", "boss@example.test"), ("SeedAdmin:UserName", "boss"), ("SeedAdmin:Password", Pw)), Hasher);

            Assert.True(created);
            var admin = Assert.Single(ctx.Users);
            Assert.Equal("Admin", admin.Role);
            Assert.Equal("boss@example.test", admin.Email);
            Assert.NotEqual(Pw, admin.Password);
            Assert.Equal(PasswordVerificationResult.Success, Hasher.VerifyHashedPassword(admin, admin.Password, Pw));
        }

        [Fact]
        public void RunningTwice_IsIdempotent_AndDoesNotOverwriteExistingUser()
        {
            using var ctx = NewContext();
            var cfg = Config(("SeedAdmin:Email", "boss@example.test"), ("SeedAdmin:UserName", "boss"), ("SeedAdmin:Password", Pw));

            SeedData.SeedAdmin(ctx, cfg, Hasher);
            var hashAfterFirst = ctx.Users.Single().Password;
            var second = SeedData.SeedAdmin(ctx, cfg, Hasher);

            Assert.False(second);
            Assert.Equal(hashAfterFirst, ctx.Users.Single().Password);
        }

        [Theory]
        [InlineData("boss@example.test", "boss", null)]
        [InlineData("boss@example.test", null, Pw)]
        [InlineData(null, "boss", Pw)]
        [InlineData("boss@example.test", "boss", "short")]
        public void PartialOrWeakConfiguration_CreatesNothing(string? email, string? name, string? password)
        {
            using var ctx = NewContext();

            var created = SeedData.SeedAdmin(ctx, Config(
                ("SeedAdmin:Email", email), ("SeedAdmin:UserName", name), ("SeedAdmin:Password", password)), Hasher);

            Assert.False(created);
            Assert.Empty(ctx.Users);
        }

        [Fact]
        public void SeedingCategories_NeverCreatesUsers()
        {
            using var ctx = NewContext();

            SeedData.SeedCategories(ctx);

            Assert.NotEmpty(ctx.Categories);
            Assert.Empty(ctx.Users);
        }
    }
}
