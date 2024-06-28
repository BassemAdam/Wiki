using Ganss.Xss;
using LiteDB;
using Markdig;
namespace Wiki.Models;

public class Wiki
{
    private readonly LiteDatabase _db;
    private readonly ILiteCollection<Page> _pages;
    private readonly HtmlSanitizer _sanitizer;
    private readonly MarkdownPipeline _pipeline;

    public Wiki()
    {
        _db = new LiteDatabase(@"wiki.db");
        _pages = _db.GetCollection<Page>("pages");
        _pages.EnsureIndex(x => x.Name);
        _sanitizer = new HtmlSanitizer();
        _pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
    }

    public Page? GetPage(string name) => _pages.FindOne(x => x.Name == name);

    public IEnumerable<string> GetAllPages() => _pages.FindAll().Select(x => x.Name);

    public (bool, Page?, Exception?) SavePage(PageInput input)
    {
        try
        {
            var htmlContent = Markdown.ToHtml(input.Content, _pipeline);
            var sanitizedContent = _sanitizer.Sanitize(htmlContent);
            Page page;
            if (input.Id.HasValue)
            {
                page = _pages.FindById(input.Id.Value);
                if (page == null) throw new Exception("Page not found");
                page.Content = sanitizedContent;
                page.LastModifiedUtc = DateTime.UtcNow;
                _pages.Update(page);
            }
            else
            {
                page = new Page
                {
                    Name = input.Name,
                    Content = sanitizedContent,
                    LastModifiedUtc = DateTime.UtcNow
                };
                _pages.Insert(page);
            }
            return (true, page, null);
        }
        catch (Exception ex)
        {
            return (false, null, ex);
        }
    }

    public (bool, Exception?) DeletePage(int id, string homePageName)
    {
        try
        {
            var page = _pages.FindById(id);
            if (page == null) throw new Exception("Page not found");
            if (page.Name == homePageName) throw new Exception("Cannot delete home page");
            _pages.Delete(id);
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex);
        }
    }

    public (bool, Page?, Exception?) DeleteAttachment(int pageId, string fileId)
    {
        try
        {
            var page = _pages.FindById(pageId);
            if (page == null) throw new Exception("Page not found");
            page.Attachments.Remove(fileId);
            _pages.Update(page);
            return (true, page, null);
        }
        catch (Exception ex)
        {
            return (false, null, ex);
        }
    }
}


