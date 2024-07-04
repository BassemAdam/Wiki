using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Wiki.Pages.Shared;

public class _ArticleForm : PageModel
{
    private readonly IAntiforgery _antiforgery;

    public _ArticleForm(IAntiforgery antiforgery, Models.Wiki wiki, IWebHostEnvironment env)
    {
        _antiforgery = antiforgery;
    }
    
    [BindProperty]
    public string __RequestVerificationToken { get; set; }

    [BindProperty]
    public ArticleInputModel FormInput { get; set; }

    public void OnGet()
    {
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
        __RequestVerificationToken = tokens.RequestToken;
    }
}


