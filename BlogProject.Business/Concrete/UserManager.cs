using BlogProject.Business.Abstract;
using BlogProject.Business.Security;
using BlogProject.DataAccess.Abstract;
using BlogProject.Entity;
using Microsoft.AspNetCore.Identity;

namespace BlogProject.Business.Concrete
{
    public class UserManager : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly PasswordService _passwords;

        public UserManager(IUserRepository userRepository, PasswordService? passwords = null)
        {
            _userRepository = userRepository;
            _passwords = passwords ?? new PasswordService(new PasswordHasher<User>());
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<User> GetByIdAsync(int id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _userRepository.GetByEmailAsync(email);
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _userRepository.GetByUsernameAsync(username);
        }

        public async Task<bool> CheckUserCredentialsAsync(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            return user != null && await VerifyPasswordAsync(user, password);
        }

        public async Task<bool> VerifyPasswordAsync(User user, string password)
        {
            var result = _passwords.Verify(user, password);
            if (result == PasswordCheck.Failed) return false;

            if (result == PasswordCheck.SuccessRehashNeeded)
            {
                // Legacy plain text (or an old hash format): store a fresh hash now that we know the password.
                SetPassword(user, password);
                _userRepository.Update(user);
                await _userRepository.SaveAsync();
            }
            return true;
        }

        public void SetPassword(User user, string plainPassword)
        {
            user.Password = _passwords.Hash(user, plainPassword);
        }

        public string CreatePasswordResetToken(User user, TimeSpan lifetime)
        {
            var token = ResetTokens.Generate();
            user.PasswordResetToken = ResetTokens.Hash(token);
            user.PasswordResetTokenExpires = DateTime.UtcNow.Add(lifetime);
            return token;
        }

        public bool IsPasswordResetTokenValid(User user, string token) =>
            ResetTokens.Matches(user.PasswordResetToken, token) &&
            user.PasswordResetTokenExpires is { } expires && expires > DateTime.UtcNow;

        public async Task<int> MigrateLegacyPasswordsAsync()
        {
            var changed = 0;
            foreach (var user in await _userRepository.GetAllAsync())
            {
                if (string.IsNullOrEmpty(user.Password) || PasswordService.LooksHashed(user.Password)) continue;
                SetPassword(user, user.Password);
                _userRepository.Update(user);
                changed++;
            }
            if (changed > 0) await _userRepository.SaveAsync();
            return changed;
        }

        public async Task CreateAsync(User user)
        {
            EnsureHashed(user);
            await _userRepository.AddAsync(user);
        }

        public async Task UpdateAsync(User user)
        {
            EnsureHashed(user);
            _userRepository.Update(user);
            await _userRepository.SaveAsync();
        }

        public async Task DeleteAsync(User user)
        {
            _userRepository.Delete(user);
            await _userRepository.SaveAsync();
        }

        // Safety net: a plain-text value must never reach the database through this service.
        private void EnsureHashed(User user)
        {
            if (!string.IsNullOrEmpty(user.Password) && !PasswordService.LooksHashed(user.Password))
                SetPassword(user, user.Password);
        }
    }
}
