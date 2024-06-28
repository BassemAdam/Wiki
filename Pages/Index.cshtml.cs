using Microsoft.AspNetCore.Mvc.RazorPages;

public class IndexModel : PageModel
{
    public string HomePageName { get; set; }

    public IndexModel(WikiConfig config)
    {
        HomePageName = config.HomePageName;
    }
}