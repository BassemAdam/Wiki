using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Wiki.Pages;

public class Home : PageModel
{
    private readonly Models.Wiki _wiki;
    public Home(Models.Wiki wiki)
    {
        _wiki = wiki;
    }
    public void OnGet()
    {
        
    }
  
}