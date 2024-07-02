using Microsoft.AspNetCore.Mvc.RazorPages;
using Wiki.Models;

public class IndexModel : PageModel
{
    public string HomePageName { get; set; }

    public IndexModel(WikiConfig config)
    {
        HomePageName = config.HomePageName;
    }
}