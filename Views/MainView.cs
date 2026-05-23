using Browser.Controls;
using Browser.Helpers;
using Browser.Models;
using Browser.Services;
using Browser.UI;
using Microsoft.Web.WebView2.Core;

namespace Browser.Views;

public partial class MainView : Form
{
    private readonly AppSettings _settings;
    private readonly List<BookmarkItem> _bookmarks;
    private readonly List<HistoryEntry> _history;

    private MenuStrip _menuStrip = null!;
    private Panel _toolbar = null!;
    private Panel _bookmarkBar = null!;
    private TabControl _tabs = null!;
    private TextBox _addressBar = null!;
    private Panel _statusBar = null!;
    private Label _statusLabel = null!;
    private ProgressBar _progressBar = null!;
    private Button _btnBack = null!;
    private Button _btnForward = null!;
    private Button _btnReload = null!;
    private Button _btnStop = null!;
    private Button _btnHome = null!;
    private Button _btnBookmark = null!;
    private Label _securityLabel = null!;

    private bool _privateMode;
    private bool _updatingAddressBar;

    public MainView()
    {
        _settings = AppDataService.LoadSettings();
        _bookmarks = AppDataService.LoadBookmarks();
        _history = AppDataService.LoadHistory();

        if (string.IsNullOrWhiteSpace(_settings.DownloadFolder))
            _settings.DownloadFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

        InitializeComponent();
        BuildChrome();
        LoadInitialTabs();
    }

    private void BuildChrome()
    {
        Text = "Portfolio Browser";
        Size = new Size(1280, 800);
        MinimumSize = new Size(640, 480);
        BrowserThemeApplier.ApplyForm(this);
        KeyPreview = true;

        _menuStrip = BuildMenu();
        _toolbar = BuildToolbar();
        _bookmarkBar = BuildBookmarkBar();
        _tabs = BuildTabControl();
        _statusBar = BuildStatusBar();

        var content = new Panel { Dock = DockStyle.Fill };
        BrowserThemeApplier.ApplyPanel(content);
        content.Controls.Add(_tabs);
        content.Controls.Add(_bookmarkBar);

        Controls.Add(content);
        Controls.Add(_statusBar);
        Controls.Add(_toolbar);
        Controls.Add(_menuStrip);
        MainMenuStrip = _menuStrip;

        FormClosing += MainView_FormClosing;
    }

    private MenuStrip BuildMenu()
    {
        var menu = new MenuStrip();
        BrowserThemeApplier.ApplyMenuStrip(menu);

        var file = new ToolStripMenuItem("Файл");
        file.DropDownItems.Add("Новая вкладка\tCtrl+T", null, (_, _) => NewTab(_settings.HomePage));
        file.DropDownItems.Add("Закрыть вкладку\tCtrl+W", null, (_, _) => CloseCurrentTab());
        file.DropDownItems.Add(new ToolStripSeparator());
        file.DropDownItems.Add("Выход", null, (_, _) => Close());

        var view = new ToolStripMenuItem("Вид");
        view.DropDownItems.Add("Увеличить\tCtrl++", null, (_, _) => Zoom(0.1));
        view.DropDownItems.Add("Уменьшить\tCtrl+-", null, (_, _) => Zoom(-0.1));
        view.DropDownItems.Add("Сбросить масштаб\tCtrl+0", null, (_, _) => Zoom(0, reset: true));
        view.DropDownItems.Add(new ToolStripSeparator());
        view.DropDownItems.Add("Инструменты разработчика\tF12", null, (_, _) => OpenDevTools());

        var bookmarks = new ToolStripMenuItem("Закладки");
        bookmarks.DropDownItems.Add("Добавить\tCtrl+D", null, (_, _) => AddCurrentBookmark());
        bookmarks.DropDownItems.Add("Менеджер закладок", null, (_, _) => ShowBookmarks());

        var history = new ToolStripMenuItem("История");
        history.DropDownItems.Add("Показать историю\tCtrl+H", null, (_, _) => ShowHistory());

        var tools = new ToolStripMenuItem("Сервис");
        tools.DropDownItems.Add("Настройки", null, (_, _) => ShowSettings());
        tools.DropDownItems.Add(new ToolStripSeparator());
        tools.DropDownItems.Add("Режим инкогнито", null, (_, _) => TogglePrivateMode());

        var help = new ToolStripMenuItem("Справка");
        help.DropDownItems.Add("О программе", null, (_, _) =>
            MessageBox.Show(
                "Portfolio Browser\n\nПолнофункциональный браузер на WebView2 (Chromium).\n\n© 2026",
                "О программе",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information));

        menu.Items.AddRange(new ToolStripItem[] { file, view, bookmarks, history, tools, help });
        return menu;
    }

    private Panel BuildToolbar()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 50,
            BackColor = BrowserTheme.ToolbarBg,
            Padding = new Padding(8, 6, 8, 6)
        };

        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = BrowserTheme.ToolbarBg
        };

        _btnBack = ToolButton("←", "Назад (Alt+←)");
        _btnForward = ToolButton("→", "Вперёд (Alt+→)");
        _btnReload = ToolButton("↻", "Обновить (F5)");
        _btnStop = ToolButton("✕", "Остановить (Esc)");
        _btnHome = ToolButton("⌂", "Домой");
        _btnBookmark = ToolButton("★", "Добавить в закладки (Ctrl+D)");

        _btnBack.Click += (_, _) => ActiveTab?.WebView.CoreWebView2?.GoBack();
        _btnForward.Click += (_, _) => ActiveTab?.WebView.CoreWebView2?.GoForward();
        _btnReload.Click += (_, _) => ActiveTab?.WebView.CoreWebView2?.Reload();
        _btnStop.Click += (_, _) => ActiveTab?.WebView.CoreWebView2?.Stop();
        _btnHome.Click += (_, _) => NavigateActive(_settings.HomePage);
        _btnBookmark.Click += (_, _) => AddCurrentBookmark();

        var newTabBtn = ToolButton("+", "Новая вкладка (Ctrl+T)");
        newTabBtn.Click += (_, _) => NewTab(_settings.HomePage);

        _addressBar = new TextBox { Width = 480, Height = 28, BorderStyle = BorderStyle.None };
        BrowserThemeApplier.ApplyTextBox(_addressBar);
        _addressBar.KeyDown += AddressBar_KeyDown;

        var addressShell = new Panel
        {
            Width = 520,
            Height = 34,
            BackColor = BrowserTheme.Border,
            Padding = new Padding(1),
            Margin = new Padding(6, 2, 4, 2)
        };
        _addressBar.Dock = DockStyle.Fill;
        addressShell.Controls.Add(_addressBar);

        var goBtn = ToolButton("Перейти", "Перейти (Enter)", primary: true);
        goBtn.Width = 88;
        goBtn.Click += (_, _) => NavigateFromAddressBar();

        var historyBtn = ToolButton("🕐", "История");
        historyBtn.Click += (_, _) => ShowHistory();

        var settingsBtn = ToolButton("⚙", "Настройки");
        settingsBtn.Click += (_, _) => ShowSettings();

        var devBtn = ToolButton("{ }", "DevTools (F12)");
        devBtn.Click += (_, _) => OpenDevTools();

        flow.Controls.AddRange(new Control[]
        {
            _btnBack, _btnForward, _btnReload, _btnStop, _btnHome, newTabBtn,
            addressShell, goBtn,
            _btnBookmark, historyBtn, settingsBtn, devBtn
        });

        panel.Controls.Add(flow);
        return panel;
    }

    private Panel BuildBookmarkBar()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 32,
            BackColor = BrowserTheme.BookmarkBarBg,
            AutoScroll = true
        };

        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(4, 2, 4, 2)
        };

        foreach (var url in _settings.BookmarkBarUrls)
        {
            var host = GetHostLabel(url);
            var btn = new Button
            {
                Text = host,
                Margin = new Padding(4, 3, 4, 3),
                Tag = url
            };
            BrowserThemeApplier.ApplyBookmarkChip(btn);
            btn.Click += (_, _) => NavigateActive((string)btn.Tag!);
            flow.Controls.Add(btn);
        }

        panel.Controls.Add(flow);
        return panel;
    }

    private TabControl BuildTabControl()
    {
        var tabs = new TabControl
        {
            Dock = DockStyle.Fill,
            Appearance = TabAppearance.Normal,
            SizeMode = TabSizeMode.Normal,
            Padding = new Point(14, 5),
            DrawMode = TabDrawMode.OwnerDrawFixed,
            ItemSize = new Size(180, 32)
        };
        BrowserThemeApplier.ApplyTabControl(tabs);
        tabs.DrawItem += Tabs_DrawItem;
        tabs.SelectedIndexChanged += (_, _) => OnActiveTabChanged();
        tabs.MouseUp += Tabs_MouseUp;
        return tabs;
    }

    private Panel BuildStatusBar()
    {
        var panel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 24,
            BackColor = BrowserTheme.StatusBg
        };

        _securityLabel = new Label
        {
            Text = "🔒",
            AutoSize = true,
            Dock = DockStyle.Left,
            Padding = new Padding(10, 4, 0, 0),
            Font = new Font("Segoe UI", 10F)
        };
        BrowserThemeApplier.ApplyLabel(_securityLabel);

        _statusLabel = new Label
        {
            Text = "Готово",
            AutoSize = false,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(6, 4, 0, 0)
        };
        BrowserThemeApplier.ApplyLabel(_statusLabel, muted: true);

        _progressBar = new ProgressBar
        {
            Dock = DockStyle.Right,
            Width = 140,
            Height = 16,
            Style = ProgressBarStyle.Continuous,
            Visible = false
        };

        panel.Controls.Add(_statusLabel);
        panel.Controls.Add(_progressBar);
        panel.Controls.Add(_securityLabel);
        return panel;
    }

    private async void LoadInitialTabs()
    {
        var urls = new List<string>();
        if (_settings.RestoreSessionOnStartup && _settings.LastSessionUrls.Count > 0)
            urls.AddRange(_settings.LastSessionUrls.Where(u => !string.IsNullOrWhiteSpace(u)));
        else
            urls.Add(_settings.HomePage);

        if (urls.Count == 0)
            urls.Add(_settings.HomePage);

        foreach (var url in urls)
            NewTab(url, select: urls.Count == 1 || url == urls[^1]);

        if (_tabs.TabCount == 0)
            NewTab(_settings.HomePage, select: true);

        await Task.CompletedTask;
    }

    private BrowserTabPage? ActiveTab =>
        _tabs.SelectedTab as BrowserTabPage;

    private void NewTab(string url, bool select = true)
    {
        var tab = new BrowserTabPage();
        tab.PendingUrl = url;
        tab.TabStateChanged += (_, _) =>
        {
            if (tab == ActiveTab)
                RefreshChromeForActiveTab();
        };
        tab.DownloadStarting += Tab_DownloadStarting;
        tab.NewWindowRequested += Tab_NewWindowRequested;

        _tabs.TabPages.Add(tab);
        if (select)
            _tabs.SelectedTab = tab;

        _ = InitTabAsync(tab);
    }

    private async Task InitTabAsync(BrowserTabPage tab)
    {
        try
        {
            await tab.InitializeAsync(AppDataService.ProfileFolder(_privateMode));
            HookCore(tab);
            if (tab == ActiveTab)
                RefreshChromeForActiveTab();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Не удалось запустить WebView2.\n\n{ex.Message}\n\nУстановите WebView2 Runtime.",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void CloseCurrentTab()
    {
        if (_tabs.TabCount <= 1)
        {
            NavigateActive(_settings.HomePage);
            return;
        }

        var tab = ActiveTab;
        if (tab == null) return;
        _tabs.TabPages.Remove(tab);
        tab.Dispose();
    }

    private void OnActiveTabChanged()
    {
        RefreshChromeForActiveTab();
    }

    private void RefreshChromeForActiveTab()
    {
        var tab = ActiveTab;
        var core = tab?.WebView.CoreWebView2;

        _btnBack.Enabled = core?.CanGoBack ?? false;
        _btnForward.Enabled = core?.CanGoForward ?? false;
        BrowserThemeApplier.UpdateNavButtons(_btnBack, _btnForward);
        _btnStop.Visible = core != null && IsLoading(tab!);
        _btnReload.Visible = !_btnStop.Visible;

        if (core != null && !_updatingAddressBar)
        {
            _updatingAddressBar = true;
            _addressBar.Text = core.Source;
            _updatingAddressBar = false;
            _securityLabel.Text = core.Source.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ? "🔒" : "⚠";
            Text = $"Portfolio Browser — {core.DocumentTitle}";
            _statusLabel.Text = core.Source;
        }
    }

    private static bool IsLoading(BrowserTabPage tab)
    {
        return tab.WebView.CoreWebView2 != null; // simplified; progress events drive UI
    }

    private void NavigateFromAddressBar()
    {
        if (UrlNormalizer.TryResolve(_addressBar.Text, _settings.SearchUrlTemplate, out var uri))
            NavigateActive(uri.ToString());
    }

    private void NavigateActive(string url)
    {
        var tab = ActiveTab;
        if (tab?.WebView.CoreWebView2 != null)
            tab.WebView.CoreWebView2.Navigate(url);
        else if (tab != null)
            tab.PendingUrl = url;
    }

    private void AddressBar_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            NavigateFromAddressBar();
            e.Handled = true;
        }
    }

    private void Tab_DownloadStarting(object? sender, CoreWebView2DownloadStartingEventArgs e)
    {
        Directory.CreateDirectory(_settings.DownloadFolder);
        var name = Path.GetFileName(e.DownloadOperation.Uri) ?? "download";
        if (string.IsNullOrEmpty(Path.GetExtension(name)))
            name = "download";
        e.ResultFilePath = Path.Combine(_settings.DownloadFolder, name);
        e.Handled = true;
        e.DownloadOperation.StateChanged += (_, _) =>
        {
            if (e.DownloadOperation.State == CoreWebView2DownloadState.Completed)
            {
                BeginInvoke(() =>
                {
                    _statusLabel.Text = $"Загружено: {Path.GetFileName(e.ResultFilePath)}";
                    var open = MessageBox.Show(
                        $"Файл сохранён:\n{e.ResultFilePath}\n\nОткрыть папку?",
                        "Загрузка",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);
                    if (open == DialogResult.Yes)
                        System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{e.ResultFilePath}\"");
                });
            }
        };
    }

    private void Tab_NewWindowRequested(object? sender, CoreWebView2NewWindowRequestedEventArgs e)
    {
        BeginInvoke(() => NewTab(e.Uri));
    }

    private void AddCurrentBookmark()
    {
        var core = ActiveTab?.WebView.CoreWebView2;
        if (core == null) return;

        var url = core.Source;
        if (_bookmarks.Any(b => b.Url == url))
        {
            MessageBox.Show("Уже в закладках.", Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        _bookmarks.Add(new BookmarkItem
        {
            Title = core.DocumentTitle,
            Url = url,
            AddedAt = DateTime.UtcNow
        });
        AppDataService.SaveBookmarks(_bookmarks);
        _statusLabel.Text = "Закладка добавлена";
    }

    private void RecordHistory(string title, string url)
    {
        if (string.IsNullOrWhiteSpace(url) || url.StartsWith("about:", StringComparison.OrdinalIgnoreCase))
            return;

        _history.RemoveAll(h => h.Url == url);
        _history.Insert(0, new HistoryEntry
        {
            Title = title,
            Url = url,
            VisitedAt = DateTime.UtcNow
        });
        AppDataService.SaveHistory(_history);
    }

    private void ShowBookmarks()
    {
        using var dlg = new BookmarksDialog(_bookmarks);
        if (dlg.ShowDialog(this) == DialogResult.OK && dlg.SelectedUrl != null)
            NavigateActive(dlg.SelectedUrl);
    }

    private void ShowHistory()
    {
        using var dlg = new HistoryDialog(_history);
        if (dlg.ShowDialog(this) == DialogResult.OK && dlg.SelectedUrl != null)
            NavigateActive(dlg.SelectedUrl);
    }

    private void ShowSettings()
    {
        using var dlg = new SettingsDialog(_settings);
        if (dlg.ShowDialog(this) == DialogResult.OK)
            RebuildBookmarkBar();
    }

    private void RebuildBookmarkBar()
    {
        var parent = _bookmarkBar.Parent;
        var idx = parent?.Controls.GetChildIndex(_bookmarkBar) ?? -1;
        _bookmarkBar.Dispose();
        _bookmarkBar = BuildBookmarkBar();
        if (parent != null && idx >= 0)
        {
            parent.Controls.Add(_bookmarkBar);
            parent.Controls.SetChildIndex(_bookmarkBar, idx);
        }
    }

    private void TogglePrivateMode()
    {
        if (MessageBox.Show(
                "Режим инкогнито использует отдельный профиль.\nВсе вкладки будут закрыты. Продолжить?",
                Text,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        _privateMode = !_privateMode;
        Text = _privateMode ? "Portfolio Browser — Инкогнито" : "Portfolio Browser";

        while (_tabs.TabCount > 0)
        {
            var tab = _tabs.TabPages[0] as BrowserTabPage;
            _tabs.TabPages.RemoveAt(0);
            tab?.Dispose();
        }

        NewTab(_settings.HomePage);
    }

    private void OpenDevTools() => ActiveTab?.WebView.CoreWebView2?.OpenDevToolsWindow();

    private void Zoom(double delta, bool reset = false)
    {
        var view = ActiveTab?.WebView;
        if (view?.CoreWebView2 == null) return;
        if (reset)
            view.ZoomFactor = 1.0;
        else
            view.ZoomFactor = Math.Clamp(view.ZoomFactor + delta, 0.25, 4.0);
    }

    private void Tabs_DrawItem(object? sender, DrawItemEventArgs e)
    {
        var selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
        var bg = selected ? BrowserTheme.TabActive : BrowserTheme.TabInactive;
        var bounds = e.Bounds;
        bounds.Inflate(-2, -2);

        using (var brush = new SolidBrush(bg))
            e.Graphics.FillRectangle(brush, bounds);

        using (var border = new Pen(selected ? BrowserTheme.Accent : BrowserTheme.Border))
            e.Graphics.DrawRectangle(border, bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1);

        var text = _tabs.TabPages[e.Index].Text;
        var textRect = bounds;
        textRect.X += 10;
        textRect.Width -= 12;
        TextRenderer.DrawText(
            e.Graphics,
            text,
            BrowserTheme.TabFont,
            textRect,
            BrowserTheme.Foreground,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    private void Tabs_MouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Middle) return;
        for (int i = 0; i < _tabs.TabCount; i++)
        {
            if (_tabs.GetTabRect(i).Contains(e.Location))
            {
                if (_tabs.TabCount > 1)
                {
                    var tab = _tabs.TabPages[i] as BrowserTabPage;
                    _tabs.TabPages.RemoveAt(i);
                    tab?.Dispose();
                }
                break;
            }
        }
    }

    private void MainView_FormClosing(object? sender, FormClosingEventArgs e)
    {
        _settings.LastSessionUrls = new List<string>();
        foreach (BrowserTabPage tab in _tabs.TabPages)
        {
            var src = tab.WebView.CoreWebView2?.Source;
            if (!string.IsNullOrWhiteSpace(src))
                _settings.LastSessionUrls.Add(src);
        }
        AppDataService.SaveSettings(_settings);
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        WireNavigationEvents();
    }

    private void WireNavigationEvents()
    {
        foreach (BrowserTabPage tab in _tabs.TabPages)
            AttachTabEvents(tab);
        _tabs.ControlAdded += (_, ev) =>
        {
            if (ev.Control is BrowserTabPage tab)
                AttachTabEvents(tab);
        };
    }

    private void AttachTabEvents(BrowserTabPage tab)
    {
        if (tab.WebView.CoreWebView2 == null)
        {
            tab.WebView.CoreWebView2InitializationCompleted += (_, _) =>
            {
                if (tab.WebView.CoreWebView2 != null)
                    HookCore(tab);
            };
        }
        else
        {
            HookCore(tab);
        }
    }

    private void HookCore(BrowserTabPage tab)
    {
        var core = tab.WebView.CoreWebView2;
        if (core == null) return;

        core.NavigationStarting += (_, _) =>
        {
            if (tab == ActiveTab)
            {
                _progressBar.Style = ProgressBarStyle.Marquee;
                _progressBar.Visible = true;
                _statusLabel.Text = "Загрузка…";
                _statusLabel.ForeColor = BrowserTheme.Foreground;
            }
        };
        core.NavigationCompleted += (_, args) =>
        {
            if (tab != ActiveTab) return;
            _progressBar.Visible = false;
            _statusLabel.ForeColor = BrowserTheme.Muted;
            if (args.IsSuccess)
            {
                RecordHistory(core.DocumentTitle, core.Source);
                RefreshChromeForActiveTab();
            }
            else
                _statusLabel.Text = "Ошибка загрузки";
        };
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == (Keys.Control | Keys.T)) { NewTab(_settings.HomePage); return true; }
        if (keyData == (Keys.Control | Keys.W)) { CloseCurrentTab(); return true; }
        if (keyData == (Keys.Control | Keys.L)) { _addressBar.Focus(); _addressBar.SelectAll(); return true; }
        if (keyData == (Keys.Control | Keys.R) || keyData == Keys.F5) { ActiveTab?.WebView.CoreWebView2?.Reload(); return true; }
        if (keyData == Keys.F12) { OpenDevTools(); return true; }
        if (keyData == (Keys.Control | Keys.D)) { AddCurrentBookmark(); return true; }
        if (keyData == (Keys.Control | Keys.H)) { ShowHistory(); return true; }
        if (keyData == (Keys.Alt | Keys.Left)) { ActiveTab?.WebView.CoreWebView2?.GoBack(); return true; }
        if (keyData == (Keys.Alt | Keys.Right)) { ActiveTab?.WebView.CoreWebView2?.GoForward(); return true; }
        if (keyData == Keys.Escape) { ActiveTab?.WebView.CoreWebView2?.Stop(); return true; }
        if (keyData == (Keys.Control | Keys.Add) || keyData == (Keys.Control | Keys.Oemplus)) { Zoom(0.1); return true; }
        if (keyData == (Keys.Control | Keys.Subtract) || keyData == (Keys.Control | Keys.OemMinus)) { Zoom(-0.1); return true; }
        if (keyData == (Keys.Control | Keys.D0)) { Zoom(0, reset: true); return true; }
        return base.ProcessCmdKey(ref msg, keyData);
    }

    private static Button ToolButton(string glyph, string tooltip, bool primary = false)
    {
        var btn = new Button
        {
            Text = glyph,
            Size = primary ? new Size(88, 34) : new Size(40, 34),
            Margin = new Padding(2, 2, 2, 2)
        };

        if (primary)
            BrowserThemeApplier.ApplyPrimaryButton(btn);
        else
            BrowserThemeApplier.ApplyToolbarButton(btn);

        var tip = new ToolTip { AutoPopDelay = 8000, InitialDelay = 400 };
        tip.SetToolTip(btn, tooltip);
        return btn;
    }

    private static string GetHostLabel(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return uri.Host.Replace("www.", "");
        return url;
    }

}
