using Browser.UI;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace Browser.Controls;

public sealed class BrowserTabPage : TabPage
{
    public WebView2 WebView { get; }
    public bool IsInitialized { get; private set; }
    public string? PendingUrl { get; set; }

    public event EventHandler? TabStateChanged;

    public BrowserTabPage(string title = "Новая вкладка")
    {
        Text = title;
        BackColor = BrowserTheme.TabActive;
        Padding = new Padding(0);
        WebView = new WebView2
        {
            Dock = DockStyle.Fill,
            DefaultBackgroundColor = Color.White
        };
        Controls.Add(WebView);
    }

    public async Task InitializeAsync(string userDataFolder)
    {
        if (IsInitialized)
            return;

        var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
        await WebView.EnsureCoreWebView2Async(env);
        IsInitialized = true;

        var core = WebView.CoreWebView2;
        core.Settings.AreDefaultContextMenusEnabled = true;
        core.Settings.AreDevToolsEnabled = true;
        core.Settings.IsStatusBarEnabled = false;

        core.NavigationStarting += (_, _) => RaiseStateChanged();
        core.SourceChanged += (_, _) => RaiseStateChanged();
        core.DocumentTitleChanged += (_, _) => UpdateTitle();
        core.HistoryChanged += (_, _) => RaiseStateChanged();
        core.NavigationCompleted += OnNavigationCompleted;
        core.DownloadStarting += OnDownloadStarting;
        core.NewWindowRequested += OnNewWindowRequested;

        if (!string.IsNullOrEmpty(PendingUrl))
        {
            core.Navigate(PendingUrl);
            PendingUrl = null;
        }

        RaiseStateChanged();
    }

    private void OnNavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
    {
        UpdateTitle();
        RaiseStateChanged();
    }

    private void OnDownloadStarting(object? sender, CoreWebView2DownloadStartingEventArgs e)
    {
        e.Handled = false;
        DownloadStarting?.Invoke(this, e);
    }

    private void OnNewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
    {
        e.Handled = true;
        NewWindowRequested?.Invoke(this, e);
    }

    public event EventHandler<CoreWebView2DownloadStartingEventArgs>? DownloadStarting;
    public event EventHandler<CoreWebView2NewWindowRequestedEventArgs>? NewWindowRequested;

    public void UpdateTitle()
    {
        if (WebView.CoreWebView2 == null)
            return;

        var title = WebView.CoreWebView2.DocumentTitle;
        Text = string.IsNullOrWhiteSpace(title) ? "Новая вкладка" : Truncate(title, 28);
    }

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..(max - 1)] + "…";

    private void RaiseStateChanged() => TabStateChanged?.Invoke(this, EventArgs.Empty);
}
