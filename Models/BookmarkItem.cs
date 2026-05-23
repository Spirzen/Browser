namespace Browser.Models;

public sealed class BookmarkItem
{
    public string Title { get; set; } = "";
    public string Url { get; set; } = "";
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
