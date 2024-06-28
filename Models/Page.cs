namespace Wiki.Models;

public class Page
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime LastModifiedUtc { get; set; }
    public List<string> Attachments { get; set; } = new List<string>();
}