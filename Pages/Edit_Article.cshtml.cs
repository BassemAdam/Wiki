using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wiki.Models;

namespace Wiki.Pages
{
    public class Edit_Article : PageModel
    {
        private readonly IAntiforgery _antiforgery;
        private readonly Wiki.Models.Wiki _wiki;

        public Edit_Article(IAntiforgery antiforgery, Wiki.Models.Wiki wiki)
        {
            _antiforgery = antiforgery;
            _wiki = wiki;
        }

        [BindProperty]
        public int Id { get; set; }

        [BindProperty]
        public string __RequestVerificationToken { get; set; }

        [BindProperty]
        public ArticleInputModel FormInput { get; set; }

        public IActionResult OnGet(int id)
        {
            Id = id;
            var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
            __RequestVerificationToken = tokens.RequestToken;

            // Load the article from the database
            var article = _wiki.GetPageById(id);

            if (article != null)
            {
                FormInput = new ArticleInputModel
                {
                    ArticleName = article.Name,
                    Content = article.Content
                };
                return Page();
            }
            else
            {
                return NotFound();
            }
        }
    }
}