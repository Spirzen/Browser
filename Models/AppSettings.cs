namespace Browser.Models;

public sealed class AppSettings
{
    public string HomePage { get; set; } = "https://www.google.com";
    public string SearchUrlTemplate { get; set; } = "https://www.google.com/search?q={0}";
    public string DownloadFolder { get; set; } = "";
    public bool RestoreSessionOnStartup { get; set; } = true;
    public List<string> BookmarkBarUrls { get; set; } = new()
    {
        "https://www.google.com",
        "https://github.com",
        "https://stackoverflow.com"
    };
    public List<string> LastSessionUrls { get; set; } = new();
}
