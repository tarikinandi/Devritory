using System.Text.RegularExpressions;

namespace BlogProject.Web.HtmlHelpers
{
    public static class SlugHelper
    {
        public static string GenerateSlug(string text)
        {
            text = text.ToLowerInvariant().Trim();
            text = Regex.Replace(text, @"\s+", "-"); 
            text = Regex.Replace(text, @"[^a-z0-9\-]", ""); 
            text = Regex.Replace(text, @"-+", "-"); 
            return text;
        }
    }
}
