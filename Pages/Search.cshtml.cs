using Microsoft.AspNetCore.Mvc.RazorPages;
using WikiPage = Wiki.Models.Page; // Alias for Wiki.Models.Page
using System.Collections.Generic;
using System.Linq;

namespace Wiki.Pages
{
    public class SearchModel : PageModel
    {
        private readonly Models.Wiki _wiki;

        public SearchModel(Models.Wiki wiki)
        {
            _wiki = wiki;
        }

        public IEnumerable<WikiPage> SearchResults { get; private set; } // Use the alias here

        public void OnGet(string query)
        {
            SearchResults = _wiki.GetAllPagesDetails()
                .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                            p.Content.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}