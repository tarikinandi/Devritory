namespace BlogProject.Web.Models
{
    public class ReCaptchaResponse
    {
        public bool success { get; set; }
        public string challenge_ts { get; set; }
        public string hostname { get; set; }
        public string[] error_codes { get; set; }
    }
}