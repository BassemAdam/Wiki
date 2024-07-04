using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace Wiki.Models;

public class Page
{
    public int Id { get; set; }
    public int NumOfVisits { get; set; } = 0;
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime LastModifiedUtc { get; set; }
    public List<string> Attachments { get; set; }
}

public record PageInput(int? Id, string Name, string Content, string? FilePaths)
{
    public static PageInput From(IFormCollection form)
    {
        return new PageInput(
            int.TryParse(form["Id"], out var id) ? id : null,
            form["Name"],
            form["Content"],
            form["FilePaths"]
        );
    }
}
public record PageInputEdit(int? Id, string Name, string Content)
{
    public static PageInputEdit From(IFormCollection form)
    {
        return new PageInputEdit(
            int.TryParse(form["Id"], out var id) ? id : null,
            form["Name"],
            form["Content"]
        );
    }
}

public class WikiConfig
{
    public string PageName { get; set; }
    public string HomePageName { get; set; }
}
public class PageInputValidator : AbstractValidator<PageInput>
{
    public PageInputValidator(WikiConfig config)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .Must(name => !name.Equals(config.HomePageName, StringComparison.OrdinalIgnoreCase)).WithMessage("Page name cannot be 'home-page'.");
        RuleFor(x => x.Content).NotEmpty().WithMessage("Content is required.");
    }
}
