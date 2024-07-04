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


