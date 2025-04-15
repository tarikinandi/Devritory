using Microsoft.AspNetCore.Mvc;

namespace BlogProject.Web.ViewComponents
{
    public class BreadcrumbViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(List<(string Text, string? Url)> items)
        {
            return View("Default", items);
        }
    }
}
