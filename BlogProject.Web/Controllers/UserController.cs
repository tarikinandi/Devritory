using BlogProject.Business.Abstract;
using BlogProject.Business.Security;
using BlogProject.Entity;
using BlogProject.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlogProject.Web.Controllers
{
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly LoginThrottle _throttle;

        public UserController(IUserService userService, IConfiguration configuration, IEmailService emailService, LoginThrottle throttle)
        {
            _userService = userService;
            _configuration = configuration;
            _emailService = emailService;
            _throttle = throttle;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var existingUser = await _userService.GetByEmailAsync(model.Email)
                            ?? await _userService.GetByUsernameAsync(model.UserName);

            if (existingUser != null)
            {
                ModelState.AddModelError("", "Bu e-posta veya kullanıcı adı zaten alınmış.");
                return View(model);
            }

            var newUser = new User
            {
                UserName = model.UserName,
                Email = model.Email,
                ImagePath = model.Image != null ? SaveImage(model.Image) : "default.png"
            };
            _userService.SetPassword(newUser, model.Password);

            await _userService.CreateAsync(newUser);

            return RedirectToAction("Login");
        }

        private string SaveImage(IFormFile image)
        {
            // Server-chosen name and an image-extension allow-list: no overwriting of existing
            // files and no uploaded scripts or HTML.
            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (extension is not (".jpg" or ".jpeg" or ".png" or ".gif" or ".webp"))
                return "default.png";

            var fileName = Guid.NewGuid().ToString("N") + extension;
            var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                image.CopyTo(stream);
            }
            return fileName;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
                return View(model);

            var throttleKey = LoginThrottle.Key(model.Email, HttpContext.Connection.RemoteIpAddress?.ToString());
            if (_throttle.IsLocked(throttleKey))
            {
                ModelState.AddModelError("", "Çok fazla başarısız deneme. Lütfen biraz sonra tekrar deneyin.");
                return View(model);
            }

            // Same message whether the e-mail exists or not, so the form does not reveal accounts.
            var user = await _userService.GetByEmailAsync(model.Email);
            if (user == null || !await _userService.VerifyPasswordAsync(user, model.Password))
            {
                _throttle.RegisterFailure(throttleKey);
                ModelState.AddModelError("", "E-posta veya şifre hatalı.");
                return View(model);
            }

            _throttle.Reset(throttleKey);
            var token = GenerateJwtToken(user);

            Response.Cookies.Append("blogToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddHours(2)
            });

            // Local paths only: an absolute or protocol-relative returnUrl would be an open redirect.
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return LocalRedirect(returnUrl);

            return RedirectToAction("Index", "Blog");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("blogToken");
            return RedirectToAction("Login");
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
             {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("ImagePath", user.ImagePath ?? "default.png"),
                new Claim(ClaimTypes.Role, user.Role ?? "User")
            };

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(PasswordResetRequestViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Identical answer for known and unknown e-mails (no account enumeration).
            var user = await _userService.GetByEmailAsync(model.Email);
            if (user != null)
            {
                var token = _userService.CreatePasswordResetToken(user, TimeSpan.FromHours(1));
                await _userService.UpdateAsync(user);

                var resetLink = Url.Action("ResetPassword", "User", new { token = token, email = model.Email }, Request.Scheme);
                await _emailService.SendAsync(user.Email, "Şifre Yenileme Bağlantısı", $"Şifrenizi yenilemek için <a href='{resetLink}'>buraya tıklayın</a>.");
            }

            ViewBag.Message = "Bu e-posta adresine kayıtlı bir hesap varsa, şifre yenileme bağlantısı gönderildi.";
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            return View(new ResetPasswordViewModel { Email = email, Token = token });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userService.GetByEmailAsync(model.Email);
            if (user == null || !_userService.IsPasswordResetTokenValid(user, model.Token))
            {
                ModelState.AddModelError("", "Geçersiz veya süresi dolmuş bağlantı.");
                return View(model);
            }

            _userService.SetPassword(user, model.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpires = null;

            await _userService.UpdateAsync(user);

            ViewBag.Success = "Şifreniz başarıyla güncellendi. Giriş yapabilirsiniz.";
            return View("ResetPasswordSuccess");
        }

    }
}
