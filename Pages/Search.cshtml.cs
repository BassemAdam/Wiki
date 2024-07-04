using Microsoft.AspNetCore.Mvc.RazorPages;
using WikiPage = Wiki.Models.Page; 
using System.Text.RegularExpressions;

namespace Wiki.Pages
{
    public class SearchModel : PageModel
    {
        private readonly Models.Wiki _wiki;

        public SearchModel(Models.Wiki wiki)
        {
            _wiki = wiki;
        }

        public IEnumerable<WikiPage> SearchResults { get; private set; } 

       public void OnGet(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                SearchResults = _wiki.GetAllPagesDetails().ToList();
            }
            else
            {
                SearchResults = _wiki.GetAllPagesDetails()
                    .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
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