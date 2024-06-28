using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
    
    [BindProperty]
    public string ArticleName { get; set; }

    [BindProperty]
    public string Content { get; set; }

    [BindProperty]
    [DataType(DataType.Upload)]
    public IFormFileCollection Attachment { get; set; }

    [BindProperty]
    public string __RequestVerificationToken { get; set; }

    public void OnGet()
    {
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
        __RequestVerificationToken = tokens.RequestToken;
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page(); // Return partial view on error
        }

       
        List<IFormFile> attachments = new List<IFormFile>();
        if (Attachment != null)
        {
            foreach (var file in Attachment)
            {
                var filePath = Path.Combine("wwwroot/uploads", file.FileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                attachments.Add(file);
            }
        }
        FormFileCollection fileCollection = new FormFileCollection();
        foreach (var file in attachments)
        {
            fileCollection.Add(file);
        }
        var input = new PageInput(
            null,
            ArticleName, // Name
            Content, // Content
            fileCollection // Files
        );
        var (success, page, exception) = _wiki.SavePage(input);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, exception?.Message);
            return Page(); 
        }

        //return RedirectToPage("/Index");
        return new JsonResult(new { success = true, message = "Article added successfully" });

        
    }
}