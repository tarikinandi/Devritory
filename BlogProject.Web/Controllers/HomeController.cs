using BlogProject.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using System.Text.Json;

namespace BlogProject.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _config;

        public HomeController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Contact(ContactViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var recaptchaResponse = Request.Form["g-recaptcha-response"];
            var secretKey = _config["GoogleReCaptcha:SecretKey"];

            using var httpClient = new HttpClient();
            var googleVerifyUrl = $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={recaptchaResponse}";
            var verificationResponse = await httpClient.GetStringAsync(googleVerifyUrl);
            var captchaResult = JsonSerializer.Deserialize<ReCaptchaResponse>(verificationResponse);

            if (captchaResult is null || !captchaResult.success)
            {
                ModelState.AddModelError("", "Lütfen reCAPTCHA doğrulamasını tamamlayınız.");
                return View(model);
            }

            var emailSettings = _config.GetSection("EmailSettings");

            var mail = new MailMessage
            {
                From = new MailAddress(emailSettings["Username"]!, "İletişim Formu"),
                Subject = model.Subject,
                Body = $"Gönderen: {model.Name} ({model.Email})\n\nMesaj:\n{model.Message}",
                IsBodyHtml = false
            };
            mail.To.Add(emailSettings["ToEmail"]!);

            using var smtp = new SmtpClient
            {
                Host = emailSettings["Host"],
                Port = int.Parse(emailSettings["Port"]!),
                EnableSsl = bool.Parse(emailSettings["EnableSSL"]!),
                Credentials = new NetworkCredential(emailSettings["Username"], emailSettings["Password"])
            };

            await smtp.SendMailAsync(mail);

            ViewBag.Success = true;
            ModelState.Clear();
            return View();
        }

        [HttpGet]
        public IActionResult About() => View();

        public IActionResult Privacy() => View();

        public IActionResult Policy() => View();

        public IActionResult FAQ() => View();
       
    }
}
