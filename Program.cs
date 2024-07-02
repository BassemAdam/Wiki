using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Antiforgery;
using Wiki.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAntiforgery()
    .AddMemoryCache()
    .AddSingleton<Wiki.Models.Wiki>()
    .AddRazorPages()
    .AddRazorRuntimeCompilation();

builder.Services.AddSingleton(new WikiConfig { PageName = "page-name", HomePageName = "home-page" });
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Logging.AddConsole().SetMinimumLevel(LogLevel.Warning);

var app = builder.Build();
app.MapRazorPages();

app.MapGet("/", async context =>{
    context.Response.Redirect("/Index");
});

app.MapGet("/edit/{pageName}", async context =>{
    var wiki = context.RequestServices.GetRequiredService<Wiki.Models.Wiki>();
    var pageName = context.Request.RouteValues["pageName"] as string;

    var page = wiki.GetPage(pageName);
    if (page == null)
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("Page not found");
        return;
    }

    var tokens = context.RequestServices.GetRequiredService<IAntiforgery>().GetAndStoreTokens(context);
    await context.Response.WriteAsync($@"
        <form method='post' action='/edit'>
            <input type='hidden' name='__RequestVerificationToken' value='{tokens.RequestToken}' />
            <input type='hidden' name='Id' value='{page.Id}' />
            <label for='Name'>Name</label>
            <input type='text' name='Name' value='{page.Name}' />
            <label for='Content'>Content</label>
            <textarea name='Content'>{page.Content}</textarea>
            <button type='submit'>Save</button>
        </form>");
});

app.MapPost("/edit", async context =>{
    var form = await context.Request.ReadFormAsync();
    var input = PageInput.From(form);

    var config = context.RequestServices.GetRequiredService<WikiConfig>();
    var validator = new PageInputValidator(config);
    var result = await validator.ValidateAsync(input);

    if (!result.IsValid)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync(string.Join("\n", result.Errors.Select(e => e.ErrorMessage)));
        return;
    }

    var wiki = context.RequestServices.GetRequiredService<Wiki.Models.Wiki>();
    var (isOk, page, ex) = wiki.SavePage(input);
    if (!isOk)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("Problem in saving page");
        return;
    }

    context.Response.Redirect($"/page/{page!.Name}");
});

app.MapGet("/page/{pageName}", async context =>{
    var wiki = context.RequestServices.GetRequiredService<Wiki.Models.Wiki>();
    var pageName = context.Request.RouteValues["pageName"] as string;

    var page = wiki.GetPage(pageName);
    if (page == null)
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("Page not found");
        return;
    }

    await context.Response.WriteAsync($@"
        <h1>{page.Name}</h1>
        {page.Content}
        <a href='/edit/{page.Name}'>Edit</a>
        <form method='post' action='/delete-page'>
            <input type='hidden' name='id' value='{page.Id}' />
            <button type='submit'>Delete</button>
        </form>");
});

app.MapPost("/delete-page", async context =>{
    var form = await context.Request.ReadFormAsync();
    var id = int.Parse(form["id"]);

    var config = context.RequestServices.GetRequiredService<WikiConfig>();
    var wiki = context.RequestServices.GetRequiredService<Wiki.Models.Wiki>();
    var (isOk, exception) = wiki.DeletePage(id, config.HomePageName);
    if (!isOk)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("Problem deleting page");
        return;
    }

    context.Response.Redirect("/");
});

app.MapPost("/delete-attachment", async context =>{
    var form = await context.Request.ReadFormAsync();
    var fileId = form["fileId"];
    var pageId = int.Parse(form["pageId"]);

    var wiki = context.RequestServices.GetRequiredService<Wiki.Models.Wiki>();
    var (isOk, page, ex) = wiki.DeleteAttachment(pageId, fileId);
    if (!isOk)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("Problem deleting attachment");
        return;
    }

    context.Response.Redirect($"/page/{page!.Name}");
});

app.MapGet("/htmx/side-panel", async context =>{
    var wiki = context.RequestServices.GetRequiredService<Wiki.Models.Wiki>();
    var allPages = wiki.GetAllPages();

    await context.Response.WriteAsync("<ul>");
    foreach (var pageName in allPages)
    {
        await context.Response.WriteAsync($@"<li><a href='/page/{pageName}' class='htmx-get'>{pageName}</a></li>");
    }
    await context.Response.WriteAsync("</ul>");
});

app.MapGet("/htmx/page-content/{pageName}", async context =>{
    var wiki = context.RequestServices.GetRequiredService<Wiki.Models.Wiki>();
    var pageName = context.Request.RouteValues["pageName"] as string;

    var page = wiki.GetPage(pageName);
    if (page == null)
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("Page not found");
        return;
    }

    await context.Response.WriteAsync($@"
        <h1>{page.Name}</h1>
        {page.Content}");
});

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

app.Run();


