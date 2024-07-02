using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wiki.Models;
using Page = Wiki.Models.Page;

namespace Wiki.Pages;

public class Add_Article : PageModel
{
    private readonly IAntiforgery _antiforgery;
    private readonly Models.Wiki _wiki;

    public Add_Article(IAntiforgery antiforgery, Models.Wiki wiki)
    {
        _antiforgery = antiforgery;
        _wiki = wiki;
    }
    
    // [BindProperty]
    // public string ArticleName { get; set; }
    //
    // [BindProperty]
    // public string Content { get; set; }
    //
    // [BindProperty]
    // [DataType(DataType.Upload)]
    // public IFormFileCollection Attachment { get; set; }

    [BindProperty]
    public string __RequestVerificationToken { get; set; }

    

    public void OnGet()
    {
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
        __RequestVerificationToken = tokens.RequestToken;
    }
    
    public async Task<IActionResult> OnPostAsync([FromForm] ArticleInputModel formInput)
    {
        if (!ModelState.IsValid)
        {
            return Page(); // Return partial view on error
        }

        List<string> attachmentPaths = new List<string>();
        if (formInput.Attachment != null)
        {
            foreach (var file in formInput.Attachment)
            {
                var filePath = Path.Combine("wwwroot/uploads", file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                attachmentPaths.Add(filePath);
            }
        }
        string filePaths = string.Join(";", attachmentPaths);

        var input = new PageInput(
            null,
            formInput.ArticleName, // Name
            formInput.Content, // Content
            filePaths // Files
        );
        
        var (success, page, exception) = _wiki.SavePage(input);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, exception?.Message);
            return Page(); 
        }

        return new JsonResult(new { success = true, message = "Article added successfully" });
    }
}