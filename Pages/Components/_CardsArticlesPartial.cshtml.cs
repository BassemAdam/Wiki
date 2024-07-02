using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace Wiki.Pages.Components
{
    public class _CardsArticlesPartial : PageModel
    {
        private readonly Models.Wiki _wiki;
        public IEnumerable<Models.Page> Pages { get; set; }

        public _CardsArticlesPartial(Models.Wiki wiki)
        {
            _wiki = wiki;
            Pages = new List<Models.Page>();
        }
        public void OnGet()
        {
            OnGetRecentlyModifiedArticles();
        }

        public IActionResult OnGetRecentlyModifiedArticles()
        {
            var pages = _wiki.GetAllPagesDetails()
            .OrderByDescending(x => x.LastModifiedUtc)
            .Take(10); // Adjust the number of articles as needed

            if (pages != null)
            {
                foreach (Models.Page page in pages)
                {
                    Console.WriteLine(page.Name);
                }
            }
            else
            {
                Console.WriteLine("No pages found");
            }
           
            Pages = pages; // Populate the Model.Pages property

            return Page();
        }

        
    }
}

  