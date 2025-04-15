using BlogProject.Business.Abstract;
using BlogProject.Entity;
using BlogProject.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BlogProject.Web.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly IUserService _userService;

        public SettingsController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _userService.GetByIdAsync(userId);

            if (user == null)
                return NotFound();

            var model = new SettingsViewModel
            {
                FullName = user.UserName,
                Email = user.Email,
                ExistingImagePath = user.ImagePath
            };

            ViewBag.ChangePasswordModel = new ChangePasswordViewModel();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(SettingsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Eksik veya hatalı bilgiler var." });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
                return Json(new { success = false, message = "Kullanıcı bulunamadı." });

            user.UserName = model.FullName;
            user.Email = model.Email;

            if (model.ProfileImage != null)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(model.ProfileImage.FileName);
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", fileName);

                using var stream = new FileStream(path, FileMode.Create);
                await model.ProfileImage.CopyToAsync(stream);

                user.ImagePath = fileName;
            }

            await _userService.UpdateAsync(user);
            return Json(new { success = true, message = "Profil başarıyla güncellendi." });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword([FromForm] ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Geçersiz veri." });
            }

            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
                return Json(new { success = false, message = "Kullanıcı bulunamadı." });

            if (user.Password != model.CurrentPassword)
            {
                return Json(new { success = false, message = "Mevcut şifre yanlış." });
            }

            if (model.NewPassword != model.ConfirmNewPassword)
            {
                return Json(new { success = false, message = "Yeni şifreler uyuşmuyor." });
            }

            user.Password = model.NewPassword;
            await _userService.UpdateAsync(user);

            return Json(new { success = true, message = "Şifre başarıyla güncellendi." });
        }
    }
}
