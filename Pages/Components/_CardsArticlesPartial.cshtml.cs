using System.Text.RegularExpressions;
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
            OnGetPopularArticles();
        }
        public IActionResult OnGetPopularArticles()
        {
            var pages = _wiki.GetAllPagesDetails()
                .OrderByDescending(x => x.NumOfVisits)
                .Take(8); 

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

            Pages = pages; 

            return Page();
        }
        
        public IActionResult OnGetRecentlyModifiedArticles()
        {
            var pages = _wiki.GetAllPagesDetails()
            .OrderByDescending(x => x.LastModifiedUtc)
            .Take(8); 

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
           
            Pages = pages; 

            return Page();
        }
        public string TruncateContent(string content, int maxLength = 300)
        {
            if (string.IsNullOrEmpty(content)) return content;
            
            if (content.Contains("<"))
            {
                return TruncateHtml(content, maxLength);
            }
            else
            {
                return TruncatePlainText(content, maxLength);
            }
        }

        private string TruncatePlainText(string content, int maxLength)
        {
            if (content.Length <= maxLength) return content;

            return content.Substring(0, maxLength) + "...";
        }

        private string TruncateHtml(string html, int maxLength)
        {
            if (html.Length <= maxLength) return html;
            
            var truncatedHtml = html.Substring(0, maxLength) + "...";
            return CloseOpenTags(truncatedHtml);
        }

        private string CloseOpenTags(string html)
        {
            var tags = new Stack<string>();
            var regex = new Regex(@"<[^>]+>");

            foreach (Match match in regex.Matches(html))
            {
                if (match.Value.StartsWith("</"))
                {
                    if (tags.Count > 0) tags.Pop();
                }
                else if (!match.Value.EndsWith("/>"))
                {
                    var tag = match.Value.Substring(1, match.Value.Length - 2).Split(' ')[0];
                    tags.Push(tag);
                }
            }

            while (tags.Count > 0)
            {
                html += $"</{tags.Pop()}>";
            }

            return html;
        }
    }
}

  