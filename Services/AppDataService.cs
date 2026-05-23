using System.Text.Json;
using Browser.Models;

namespace Browser.Services;

public static class AppDataService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string DataFolder =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PortfolioBrowser");

    public static string SettingsPath => Path.Combine(DataFolder, "settings.json");
    public static string BookmarksPath => Path.Combine(DataFolder, "bookmarks.json");
    public static string HistoryPath => Path.Combine(DataFolder, "history.json");

    public static string ProfileFolder(bool privateMode) =>
        Path.Combine(DataFolder, privateMode ? "PrivateProfile" : "DefaultProfile");

    public static void EnsureDataFolder()
    {
        Directory.CreateDirectory(DataFolder);
    }

    public static AppSettings LoadSettings()
    {
        EnsureDataFolder();
        if (!File.Exists(SettingsPath))
            return new AppSettings();

        try
        {
            var json = File.ReadAllText(SettingsPath);
            return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public static void SaveSettings(AppSettings settings)
    {
        EnsureDataFolder();
        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(SettingsPath, json);
    }

    public static List<BookmarkItem> LoadBookmarks()
    {
        EnsureDataFolder();
        if (!File.Exists(BookmarksPath))
            return new List<BookmarkItem>();

        try
        {
            var json = File.ReadAllText(BookmarksPath);
            return JsonSerializer.Deserialize<List<BookmarkItem>>(json, JsonOptions) ?? new List<BookmarkItem>();
        }
        catch
        {
            return new List<BookmarkItem>();
        }
    }

    public static void SaveBookmarks(IEnumerable<BookmarkItem> bookmarks)
    {
        EnsureDataFolder();
        var json = JsonSerializer.Serialize(bookmarks.ToList(), JsonOptions);
        File.WriteAllText(BookmarksPath, json);
    }

    public static List<HistoryEntry> LoadHistory()
    {
        EnsureDataFolder();
        if (!File.Exists(HistoryPath))
            return new List<HistoryEntry>();

        try
        {
            var json = File.ReadAllText(HistoryPath);
            return JsonSerializer.Deserialize<List<HistoryEntry>>(json, JsonOptions) ?? new List<HistoryEntry>();
        }
        catch
        {
            return new List<HistoryEntry>();
        }
    }

    public static void SaveHistory(IEnumerable<HistoryEntry> history)
    {
        EnsureDataFolder();
        var trimmed = history
            .OrderByDescending(h => h.VisitedAt)
            .Take(5000)
            .ToList();
        var json = JsonSerializer.Serialize(trimmed, JsonOptions);
        File.WriteAllText(HistoryPath, json);
    }
}
