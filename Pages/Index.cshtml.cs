using Microsoft.AspNetCore.Mvc.RazorPages;
using Wiki.Models;

public class IndexModel : PageModel
{
    public string SuccessMessage { get; private set; }

    public void OnGet()
    {
        SuccessMessage = HttpContext.Session.GetString("SuccessMessage");
        HttpContext.Session.Remove("SuccessMessage");
    }
}