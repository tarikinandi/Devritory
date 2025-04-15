using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.RegularExpressions;

namespace BlogProject.Web.HtmlHelpers
{
    public static class HtmlTruncateExtensions
    {
        public static string TruncateHtml(this string html, int maxLength, bool stripTags = true)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;

            if (stripTags)
                html = System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);

            html = System.Net.WebUtility.HtmlDecode(html);

            return html.Length <= maxLength ? html : html.Substring(0, maxLength) + "...";
        }
    }
}
