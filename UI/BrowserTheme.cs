using System.Drawing;

namespace Browser.UI;

/// <summary>
/// Светлая тема с высоким контрастом (читаемость на любых мониторах).
/// </summary>
public static class BrowserTheme
{
    public static readonly Color WindowBg = Color.FromArgb(248, 249, 250);
    public static readonly Color ToolbarBg = Color.FromArgb(237, 238, 240);
    public static readonly Color TabBarBg = Color.FromArgb(222, 225, 230);
    public static readonly Color TabActive = Color.White;
    public static readonly Color TabInactive = Color.FromArgb(210, 214, 220);
    public static readonly Color AddressBg = Color.White;
    public static readonly Color ButtonBg = Color.White;
    public static readonly Color ButtonHover = Color.FromArgb(232, 240, 254);
    public static readonly Color Foreground = Color.FromArgb(32, 33, 36);
    public static readonly Color Muted = Color.FromArgb(95, 99, 104);
    public static readonly Color Link = Color.FromArgb(26, 115, 232);
    public static readonly Color Accent = Color.FromArgb(26, 115, 232);
    public static readonly Color AccentHover = Color.FromArgb(23, 78, 166);
    public static readonly Color Border = Color.FromArgb(218, 220, 224);
    public static readonly Color BorderStrong = Color.FromArgb(154, 160, 166);
    public static readonly Color StatusBg = Color.FromArgb(237, 238, 240);
    public static readonly Color BookmarkBarBg = Color.FromArgb(241, 243, 244);
    public static readonly Color DisabledText = Color.FromArgb(154, 160, 166);
    public static readonly Color DisabledBg = Color.FromArgb(241, 243, 244);
    public static readonly Color ListRowAlt = Color.FromArgb(248, 249, 250);

    public static Font UiFont => new("Segoe UI", 9.25F);
    public static Font AddressFont => new("Segoe UI", 10.25F);
    public static Font TabFont => new("Segoe UI Semibold", 9F);
    public static Font ToolbarFont => new("Segoe UI", 10.5F, FontStyle.Bold);
}
