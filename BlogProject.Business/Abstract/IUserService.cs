using BlogProject.Entity;

namespace BlogProject.Business.Abstract
{
    public interface IUserService
    {
        Task<List<User>> GetAllAsync();
        Task<User> GetByIdAsync(int id);
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByUsernameAsync(string username);

        /// <summary>True when the password matches. A legacy plain-text or outdated hash is upgraded and saved.</summary>
        Task<bool> CheckUserCredentialsAsync(string email, string password);

        /// <summary>Same check for an already loaded user (password change screen).</summary>
        Task<bool> VerifyPasswordAsync(User user, string password);

        /// <summary>Hashes <paramref name="plainPassword"/> into <c>user.Password</c>. Does not save.</summary>
        void SetPassword(User user, string plainPassword);

        /// <summary>Creates a reset token, stores only its hash on the user and returns the raw token. Does not save.</summary>
        string CreatePasswordResetToken(User user, TimeSpan lifetime);

        bool IsPasswordResetTokenValid(User user, string token);

        /// <summary>Hashes every row that still holds a plain-text password. Returns the number of rows changed.</summary>
        Task<int> MigrateLegacyPasswordsAsync();

        Task CreateAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(User user);
    }
}
