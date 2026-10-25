using BlogProject.Business.Concrete;
using BlogProject.Business.Security;
using BlogProject.DataAccess.Abstract;
using BlogProject.Entity;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace BlogProject.Tests.Security
{
    public class PasswordHashingTests
    {
        private readonly Mock<IUserRepository> _repo = new();
        private readonly UserManager _manager;

        public PasswordHashingTests()
        {
            _manager = new UserManager(_repo.Object, new PasswordService(new PasswordHasher<User>()));
        }

        [Fact]
        public void SetPassword_StoresHash_NotPlainText()
        {
            var user = new User { Email = "a@a.com" };

            _manager.SetPassword(user, "S3cret-pass!");

            Assert.NotEqual("S3cret-pass!", user.Password);
            Assert.DoesNotContain("S3cret-pass!", user.Password);
            Assert.True(PasswordService.LooksHashed(user.Password));
        }

        [Fact]
        public void SetPassword_SamePasswordTwice_GivesDifferentHashes()
        {
            var a = new User();
            var b = new User();
            _manager.SetPassword(a, "same-password-1");
            _manager.SetPassword(b, "same-password-1");
            Assert.NotEqual(a.Password, b.Password);
        }

        [Fact]
        public async Task VerifyPasswordAsync_HashedUser_AcceptsRightRejectsWrong_WithoutWriting()
        {
            var user = new User();
            _manager.SetPassword(user, "right-password");

            Assert.True(await _manager.VerifyPasswordAsync(user, "right-password"));
            Assert.False(await _manager.VerifyPasswordAsync(user, "wrong-password"));
            Assert.False(await _manager.VerifyPasswordAsync(user, ""));

            _repo.Verify(r => r.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task VerifyPasswordAsync_LegacyPlainText_RehashesAndSaves()
        {
            var user = new User { Email = "old@a.com", Password = "legacy-plain" };

            var ok = await _manager.VerifyPasswordAsync(user, "legacy-plain");

            Assert.True(ok);
            Assert.NotEqual("legacy-plain", user.Password);
            Assert.True(PasswordService.LooksHashed(user.Password));
            _repo.Verify(r => r.Update(user), Times.Once);
            _repo.Verify(r => r.SaveAsync(), Times.Once);

            // After the rehash the same password still works, and no second write happens.
            Assert.True(await _manager.VerifyPasswordAsync(user, "legacy-plain"));
            _repo.Verify(r => r.SaveAsync(), Times.Once);
        }

        [Fact]
        public async Task VerifyPasswordAsync_LegacyPlainText_WrongPassword_DoesNotTouchRow()
        {
            var user = new User { Password = "legacy-plain" };

            Assert.False(await _manager.VerifyPasswordAsync(user, "nope"));

            Assert.Equal("legacy-plain", user.Password);
            _repo.Verify(r => r.SaveAsync(), Times.Never);
        }

        [Fact]
        public async Task VerifyPasswordAsync_StoredHashIsNotAcceptedAsPassword()
        {
            var user = new User();
            _manager.SetPassword(user, "real-password");
            var leakedHash = user.Password;

            Assert.False(await _manager.VerifyPasswordAsync(user, leakedHash));
        }

        [Fact]
        public async Task CreateAsync_PlainTextPassword_IsHashedBeforeAdd()
        {
            User? saved = null;
            _repo.Setup(r => r.AddAsync(It.IsAny<User>())).Callback<User>(u => saved = u).Returns(Task.CompletedTask);

            await _manager.CreateAsync(new User { UserName = "n", Password = "typed-by-user" });

            Assert.NotNull(saved);
            Assert.NotEqual("typed-by-user", saved!.Password);
            Assert.True(PasswordService.LooksHashed(saved.Password));
        }

        [Fact]
        public async Task UpdateAsync_PlainTextPassword_IsHashedBeforeSave()
        {
            var user = new User { Id = 1, Password = "oops-plain" };

            await _manager.UpdateAsync(user);

            Assert.True(PasswordService.LooksHashed(user.Password));
        }

        [Fact]
        public async Task UpdateAsync_AlreadyHashed_IsNotHashedTwice()
        {
            var user = new User();
            _manager.SetPassword(user, "pw-123456789");
            var before = user.Password;

            await _manager.UpdateAsync(user);

            Assert.Equal(before, user.Password);
            Assert.True(await _manager.VerifyPasswordAsync(user, "pw-123456789"));
        }

        [Fact]
        public async Task MigrateLegacyPasswordsAsync_HashesOnlyPlainRows_AndIsIdempotent()
        {
            var hashed = new User { Id = 1 };
            _manager.SetPassword(hashed, "already-hashed");
            var hashedBefore = hashed.Password;
            var plain = new User { Id = 2, Password = "plain-one" };
            var users = new List<User> { hashed, plain };
            _repo.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

            var first = await _manager.MigrateLegacyPasswordsAsync();
            var second = await _manager.MigrateLegacyPasswordsAsync();

            Assert.Equal(1, first);
            Assert.Equal(0, second);
            Assert.Equal(hashedBefore, hashed.Password);
            Assert.True(PasswordService.LooksHashed(plain.Password));
            Assert.True(await _manager.VerifyPasswordAsync(plain, "plain-one"));
            Assert.All(users, u => Assert.True(PasswordService.LooksHashed(u.Password)));
        }

        [Theory]
        [InlineData("123456")]
        [InlineData("password")]
        [InlineData("AQAAAA-too-short")]
        [InlineData("")]
        public void LooksHashed_FalseForPlainText(string value)
        {
            Assert.False(PasswordService.LooksHashed(value));
        }

        [Fact]
        public void PasswordResetToken_StoresOnlyHash_AndValidatesOnce()
        {
            var user = new User();

            var token = _manager.CreatePasswordResetToken(user, TimeSpan.FromHours(1));

            Assert.NotEqual(token, user.PasswordResetToken);
            Assert.DoesNotContain(token, user.PasswordResetToken!);
            Assert.True(_manager.IsPasswordResetTokenValid(user, token));
            Assert.False(_manager.IsPasswordResetTokenValid(user, token + "x"));
            Assert.False(_manager.IsPasswordResetTokenValid(user, user.PasswordResetToken!));
        }

        [Fact]
        public void PasswordResetToken_Expired_IsRejected()
        {
            var user = new User();
            var token = _manager.CreatePasswordResetToken(user, TimeSpan.FromHours(1));
            user.PasswordResetTokenExpires = DateTime.UtcNow.AddMinutes(-1);

            Assert.False(_manager.IsPasswordResetTokenValid(user, token));
        }
    }
}
