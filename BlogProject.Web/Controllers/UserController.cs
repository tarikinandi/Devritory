using BlogProject.Business.Abstract;
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

        public UserController(IUserService userService, IConfiguration configuration, IEmailService emailService)
        {
            _userService = userService;
            _configuration = configuration;
            _emailService = emailService;
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
                Password = model.Password, 
                ImagePath = model.Image != null ? SaveImage(model.Image) : "default.png"
            };

            await _userService.CreateAsync(newUser);

            return RedirectToAction("Login");
        }

        private string SaveImage(IFormFile image)
        {
            var fileName = Path.GetFileName(image.FileName);
            var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                image.CopyTo(stream);
            }
            return fileName;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userService.GetByEmailAsync(model.Email);
            if (user == null || user.Password != model.Password)
            {
                ModelState.AddModelError("", "E-posta veya şifre hatalı.");
                return View(model);
            }
    
            var token = GenerateJwtToken(user);

            Response.Cookies.Append("blogToken", token, new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.Now.AddHours(2)
            });

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
                expires: DateTime.Now.AddHours(2),
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

            var user = await _userService.GetByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "E-posta sistemde bulunamadı.");
                return View(model);
            }

            var token = Guid.NewGuid().ToString();
            user.PasswordResetToken = token;
            user.PasswordResetTokenExpires = DateTime.Now.AddHours(1);

            await _userService.UpdateAsync(user);

            var resetLink = Url.Action("ResetPassword", "User", new { token = token, email = model.Email }, Request.Scheme);
            await _emailService.SendAsync(user.Email, "Şifre Yenileme Bağlantısı", $"Şifrenizi yenilemek için <a href='{resetLink}'>buraya tıklayın</a>.");

            ViewBag.Message = "Şifre yenileme bağlantısı e-posta adresinize gönderildi.";
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
            if (user == null || user.PasswordResetToken != model.Token || user.PasswordResetTokenExpires < DateTime.Now)
            {
                ModelState.AddModelError("", "Geçersiz veya süresi dolmuş bağlantı.");
                return View(model);
            }

            user.Password = model.NewPassword; 
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpires = null;

            await _userService.UpdateAsync(user);

            ViewBag.Success = "Şifreniz başarıyla güncellendi. Giriş yapabilirsiniz.";
            return View("ResetPasswordSuccess");
        }

    }
}
