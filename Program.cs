using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Wiki.Models;

#region App Setup

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAntiforgery()
    .AddMemoryCache()
    .AddDistributedMemoryCache()
    .AddSession(options => {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    })
    .AddSingleton<Wiki.Models.Wiki>()
    .AddRazorPages()
    .AddRazorRuntimeCompilation()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Program>())
    ;
    
builder.Logging.AddConsole().SetMinimumLevel(LogLevel.Warning);

var app = builder.Build();
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions {
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.WebRootPath, "uploads")),
    RequestPath = "/uploads"
});
app.UseAntiforgery();
app.UseSession();
app.MapRazorPages();

#endregion

#region EndPoints
app.MapGet("/", async context =>{
    context.Response.Redirect("/Index");
});

// app.MapGet("/edit/{pageName}", async context =>{
//     var wiki = context.RequestServices.GetRequiredService<Wiki.Models.Wiki>();
//     var pageName = context.Request.RouteValues["pageName"] as string;
//
//     var page = wiki.GetPage(pageName);
//     if (page == null)
//     {
//         context.Response.StatusCode = 404;
//         await context.Response.WriteAsync("Page not found");
//         return;
//     }
//
//     var tokens = context.RequestServices.GetRequiredService<IAntiforgery>().GetAndStoreTokens(context);
//     await context.Response.WriteAsync($@"
//         <form method='post' action='/edit'>
//             <input type='hidden' name='__RequestVerificationToken' value='{tokens.RequestToken}' />
//             <input type='hidden' name='Id' value='{page.Id}' />
//             <label for='Name'>Name</label>
//             <input type='text' name='Name' value='{page.Name}' />
//             <label for='Content'>Content</label>
//             <textarea name='Content'>{page.Content}</textarea>
//             <button type='submit'>Save</button>
//         </form>");
// });
//
// app.MapPost("/edit", async context =>{
//     var form = await context.Request.ReadFormAsync();
//     var input = PageInput.From(form);
//
//     var config = context.RequestServices.GetRequiredService<WikiConfig>();
//     var validator = new PageInputValidator(config);
//     var result = await validator.ValidateAsync(input);
//
//     if (!result.IsValid)
//     {
//         context.Response.StatusCode = 400;
//         await context.Response.WriteAsync(string.Join("\n", result.Errors.Select(e => e.ErrorMessage)));
//         return;
//     }
//
//     var wiki = context.RequestServices.GetRequiredService<Wiki.Models.Wiki>();
//     var (isOk, page, ex) = wiki.SavePage(input);
//     if (!isOk)
//     {
//         context.Response.StatusCode = 500;
//         await context.Response.WriteAsync("Problem in saving page");
//         return;
//     }
//
//     context.Response.Redirect($"/page/{page!.Name}");
// });
//
// app.MapGet("/page/{pageName}", async context =>{
//     var wiki = context.RequestServices.GetRequiredService<Wiki.Models.Wiki>();
//     var pageName = context.Request.RouteValues["pageName"] as string;
//
//     var page = wiki.GetPage(pageName);
//     if (page == null)
//     {
//         context.Response.StatusCode = 404;
//         await context.Response.WriteAsync("Page not found");
//         return;
//     }
//
//     await context.Response.WriteAsync($@"
//         <h1>{page.Name}</h1>
//         {page.Content}
//         <a href='/edit/{page.Name}'>Edit</a>
//         <form method='post' action='/delete-page'>
//             <input type='hidden' name='id' value='{page.Id}' />
//             <button type='submit'>Delete</button>
//         </form>");
// });
//
//
// app.MapPost("/delete-attachment", async context =>{
//     var form = await context.Request.ReadFormAsync();
//     var fileId = form["fileId"];
//     var pageId = int.Parse(form["pageId"]);
//
//     var wiki = context.RequestServices.GetRequiredService<Wiki.Models.Wiki>();
//     var (isOk, page, ex) = wiki.DeleteAttachment(pageId, fileId);
//     if (!isOk)
//     {
//         context.Response.StatusCode = 500;
//         await context.Response.WriteAsync("Problem deleting attachment");
//         return;
//     }
//
//     context.Response.Redirect($"/page/{page!.Name}");
// });
//
// app.MapGet("/htmx/side-panel", async context =>{
//     var wiki = context.RequestServices.GetRequiredService<Wiki.Models.Wiki>();
//     var allPages = wiki.GetAllPages();
//
//     await context.Response.WriteAsync("<ul>");
//     foreach (var pageName in allPages)
//     {
//         await context.Response.WriteAsync($@"<li><a href='/page/{pageName}' class='htmx-get'>{pageName}</a></li>");
//     }
//     await context.Response.WriteAsync("</ul>");
// });
//
// app.MapGet("/htmx/page-content/{pageName}", async context =>{
//     var wiki = context.RequestServices.GetRequiredService<Wiki.Models.Wiki>();
//     var pageName = context.Request.RouteValues["pageName"] as string;
//
//     var page = wiki.GetPage(pageName);
//     if (page == null)
//     {
//         context.Response.StatusCode = 404;
//         await context.Response.WriteAsync("Page not found");
//         return;
//     }
//
//     await context.Response.WriteAsync($@"
//         <h1>{page.Name}</h1>
//         {page.Content}");
// });

// app.MapPost("/htmx/new-page", async context =>{
//     var form = await context.Request.ReadFormAsync();
//     var pageName = form["pageName"];
//
//     var wiki = context.RequestServices.GetRequiredService<Wiki.Models.Wiki>();
//     var page = new PageInput(null, pageName, "", null);
//     var (isOk, p, ex) = wiki.SavePage(page);
//     if (!isOk)
//     {
//         context.Response.StatusCode = 500;
//         await context.Response.WriteAsync("Problem creating page");
//         return;
//     }
//
//     await context.Response.WriteAsync("Page created successfully.");
// });

app.MapPost("/delete-page/{id}", async context =>{
    var idStr = context.Request.RouteValues["id"] as string;
    if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out var id))
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Invalid or missing id parameter");
        return;
    }
    
    var wiki = context.RequestServices.GetRequiredService<Wiki.Models.Wiki>();
    var (isOk, exception) = wiki.DeletePage(id);
    if (!isOk)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("Problem deleting page");
        return;
    }

    context.Response.Redirect("/Index");
});

app.MapPost("/api/upload-image", async context => {
    var request = context.Request;
    var form = await request.ReadFormAsync();
    var file = form.Files["file"];
    var env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();

    if (file == null || file.Length == 0)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Upload a file");
        return;
    }

    var uploads = Path.Combine(env.WebRootPath, "uploads");
    if (!Directory.Exists(uploads))
        Directory.CreateDirectory(uploads);

    var filePath = Path.Combine(uploads, Guid.NewGuid().ToString() + Path.GetExtension(file.FileName));
    using (var stream = new FileStream(filePath, FileMode.Create))
    {
        await file.CopyToAsync(stream);
    }

    var fileUrl = $"/uploads/{Path.GetFileName(filePath)}";
    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync($"{{\"location\": \"{fileUrl}\"}}");
});

app.MapPost("/api/add-article", async context => {
    var request = context.Request;
    var form = await request.ReadFormAsync();
    var articleName = form["FormInput.ArticleName"];
    var content = form["FormInput.Content"];
    var wiki = context.RequestServices.GetRequiredService<Wiki.Models.Wiki>();

    // Modify image URLs to be root-relative
    content = wiki.MakeImageUrlsRootRelative(content);

    // Extract image URLs from content
    var imageUrls = new List<string>();
    var regex = new Regex("<img[^>]+src=\"(.*?)\"[^>]*>", RegexOptions.IgnoreCase);
    var matches = regex.Matches(content);
    foreach (Match match in matches)
    {
        imageUrls.Add(match.Groups[1].Value);
    }

    var input = new PageInput(
        null,
        articleName,
        content,
        string.Join(";", imageUrls)
    );

    var (success, page, exception) = wiki.SavePage(input);
    if (!success)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync(exception?.Message);
        return;
    }

    context.Session.SetString("SuccessMessage", "Article added successfully");
    context.Response.Redirect("/Index");
});

app.MapPost("/api/edit-article", async ([FromForm] PageInputEdit input, HttpContext context) => {
    var wiki = context.RequestServices.GetRequiredService<Wiki.Models.Wiki>();
    var env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();

    var oldPage = wiki.GetPageById(input.Id.Value); // Get the current page before updating

    if (oldPage == null)
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("Page not found");
        return;
    }

    // Modify image URLs to be root-relative
    var modifiedContent = wiki.MakeImageUrlsRootRelative(input.Content);

    // Extract new image URLs from content
    var newImageUrls = new List<string>();
    var regex = new Regex("<img[^>]+src=\"(.*?)\"[^>]*>", RegexOptions.IgnoreCase);
    var matches = regex.Matches(modifiedContent);
    foreach (Match match in matches)
    {
        newImageUrls.Add(match.Groups[1].Value);
    }

    // List of old image URLs
    var oldImageUrls = oldPage.Attachments ?? new List<string>();

    // Find images that are no longer used
    var imagesToDelete = oldImageUrls.Except(newImageUrls).ToList();

    // Delete unused images from the uploads folder
    foreach (var imageUrl in imagesToDelete)
    {
        var fileName = Path.GetFileName(imageUrl);
        // Ensure fileName is not null or empty
        if (!string.IsNullOrEmpty(fileName))
        {
            var filePath = Path.Combine(env.WebRootPath, "uploads", fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    // Update the page with new content and image URLs
    var updatedPage = new PageInput(
        input.Id,
        input.Name,
        modifiedContent,
        string.Join(";", newImageUrls)
    );

    var (success, page, exception) = wiki.SavePage(updatedPage);
    if (!success)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync(exception?.Message);
        return;
    }

    context.Session.SetString("SuccessMessage", "Article Edited successfully");
    context.Response.Redirect("/Index");
});

#endregion

app.Run();


