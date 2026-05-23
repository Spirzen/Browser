using Browser.Models;
using Browser.Services;
using Browser.UI;

namespace Browser.Views;

public sealed class SettingsDialog : Form
{
    private readonly TextBox _homePage = new();
    private readonly TextBox _searchTemplate = new();
    private readonly TextBox _downloadFolder = new();
    private readonly CheckBox _restoreSession = new();

    public AppSettings Settings { get; private set; }

    public SettingsDialog(AppSettings settings)
    {
        Settings = settings;
        Text = "Настройки";
        Size = new Size(540, 340);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BrowserThemeApplier.ApplyForm(this);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Padding = new Padding(16),
            BackColor = BrowserTheme.WindowBg
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddRow(layout, "Домашняя страница:", _homePage, 0);
        AddRow(layout, "Поиск (шаблон {0}):", _searchTemplate, 1);
        AddRow(layout, "Папка загрузок:", _downloadFolder, 2, browse: true);

        var startLabel = new Label { Text = "При запуске:", AutoSize = true };
        BrowserThemeApplier.ApplyLabel(startLabel);
        layout.Controls.Add(startLabel, 0, 3);

        _restoreSession.Text = "Восстанавливать вкладки";
        _restoreSession.ForeColor = BrowserTheme.Foreground;
        _restoreSession.BackColor = BrowserTheme.WindowBg;
        _restoreSession.AutoSize = true;
        layout.SetColumnSpan(_restoreSession, 2);
        layout.Controls.Add(_restoreSession, 1, 3);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 52,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(12),
            BackColor = BrowserTheme.ToolbarBg
        };
        var ok = CreateButton("Сохранить", primary: true);
        var cancel = CreateButton("Отмена", primary: false);
        ok.Click += (_, _) => { if (SaveFromFields()) DialogResult = DialogResult.OK; };
        cancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        buttons.Controls.Add(ok);
        buttons.Controls.Add(cancel);

        _homePage.Text = settings.HomePage;
        _searchTemplate.Text = settings.SearchUrlTemplate;
        _downloadFolder.Text = string.IsNullOrEmpty(settings.DownloadFolder)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")
            : settings.DownloadFolder;
        _restoreSession.Checked = settings.RestoreSessionOnStartup;

        BrowserThemeApplier.ApplyTextBox(_homePage);
        BrowserThemeApplier.ApplyTextBox(_searchTemplate);
        BrowserThemeApplier.ApplyTextBox(_downloadFolder);

        Controls.Add(layout);
        Controls.Add(buttons);
    }

    private bool SaveFromFields()
    {
        if (string.IsNullOrWhiteSpace(_homePage.Text))
        {
            MessageBox.Show("Укажите домашнюю страницу.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        Settings.HomePage = _homePage.Text.Trim();
        Settings.SearchUrlTemplate = _searchTemplate.Text.Trim();
        Settings.DownloadFolder = _downloadFolder.Text.Trim();
        Settings.RestoreSessionOnStartup = _restoreSession.Checked;
        AppDataService.SaveSettings(Settings);
        return true;
    }

    private void AddRow(TableLayoutPanel layout, string label, TextBox box, int row, bool browse = false)
    {
        var lbl = new Label { Text = label, AutoSize = true };
        BrowserThemeApplier.ApplyLabel(lbl);
        layout.Controls.Add(lbl, 0, row);

        if (!browse)
        {
            box.Dock = DockStyle.Fill;
            layout.Controls.Add(box, 1, row);
            return;
        }

        var panel = new Panel { Dock = DockStyle.Fill, Height = 30 };
        box.Dock = DockStyle.Fill;
        var btn = CreateButton("Обзор…", primary: false);
        btn.Width = 80;
        btn.Dock = DockStyle.Right;
        btn.Click += (_, _) =>
        {
            using var dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
                box.Text = dlg.SelectedPath;
        };
        panel.Controls.Add(box);
        panel.Controls.Add(btn);
        layout.Controls.Add(panel, 1, row);
    }

    private static Button CreateButton(string text, bool primary)
    {
        var btn = new Button
        {
            Text = text,
            Width = 110,
            Height = 34,
            Margin = new Padding(6)
        };
        if (primary)
            BrowserThemeApplier.ApplyPrimaryButton(btn);
        else
            BrowserThemeApplier.ApplySecondaryButton(btn);
        return btn;
    }
}
