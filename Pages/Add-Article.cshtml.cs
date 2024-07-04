using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Threading.Tasks;
using Wiki.Models;
using Page = Wiki.Models.Page;

namespace Wiki.Pages;

public class Add_Article : PageModel
{
    private readonly IAntiforgery _antiforgery;
    private readonly Models.Wiki _wiki;
    private readonly IWebHostEnvironment _env;

    public Add_Article(IAntiforgery antiforgery, Models.Wiki wiki, IWebHostEnvironment env)
    {
        _antiforgery = antiforgery;
        _wiki = wiki;
        _env = env;
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

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Extract image URLs from content
        var imageUrls = new List<string>();
        var regex = new Regex("<img[^>]+src=\"(.*?)\"[^>]*>", RegexOptions.IgnoreCase);
        var matches = regex.Matches(FormInput.Content);
        foreach (Match match in matches)
        {
            imageUrls.Add(match.Groups[1].Value);
        }

        var input = new PageInput(
            null,
            FormInput.ArticleName,
            FormInput.Content,
            string.Join(";", imageUrls)
        );

        var (success, page, exception) = _wiki.SavePage(input);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, exception?.Message);
            return Page();
        }

        return new JsonResult(new { success = true, message = "Article added successfully" });
    }

    public async Task<IActionResult> OnPostUploadImageAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Upload a file");

        var uploads = Path.Combine(_env.WebRootPath, "uploads");
        if (!Directory.Exists(uploads))
            Directory.CreateDirectory(uploads);

        var filePath = Path.Combine(uploads, Guid.NewGuid().ToString() + Path.GetExtension(file.FileName));
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var fileUrl = $"/uploads/{Path.GetFileName(filePath)}";
        return new JsonResult(new { location = fileUrl });
    }
}

public class ArticleInputModel
{
    [Required]
    public string ArticleName { get; set; }

    [Required]
    public string Content { get; set; }

    public IFormFileCollection Attachment { get; set; }
}
