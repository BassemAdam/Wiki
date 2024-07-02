using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wiki.Models;

namespace Wiki.Pages
{
    public class ArticleDetailModel : PageModel
    {
        private readonly Models.Wiki _wiki;

        public ArticleDetailModel(Models.Wiki wiki)
        {
            _wiki = wiki;
        }

        public Models.Page Article { get; set; }

        public IActionResult OnGet(int id)
        {
            Article = _wiki.GetAllPagesDetails().FirstOrDefault(p => p.Id == id);
            if (Article == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}