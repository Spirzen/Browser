using Browser.Models;
using Browser.UI;

namespace Browser.Views;

public sealed class HistoryDialog : Form
{
    public string? SelectedUrl { get; private set; }

    public HistoryDialog(IReadOnlyList<HistoryEntry> history)
    {
        Text = "История";
        Size = new Size(680, 500);
        StartPosition = FormStartPosition.CenterParent;
        BrowserThemeApplier.ApplyForm(this);

        var search = new TextBox
        {
            Dock = DockStyle.Top,
            Height = 32,
            PlaceholderText = "Поиск по истории…"
        };
        BrowserThemeApplier.ApplyTextBox(search);

        var list = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true
        };
        BrowserThemeApplier.ApplyListView(list);
        list.Columns.Add("Заголовок", 240);
        list.Columns.Add("URL", 300);
        list.Columns.Add("Когда", 120);

        void Populate(string filter)
        {
            list.Items.Clear();
            var q = (filter ?? "").Trim().ToLowerInvariant();
            foreach (var entry in history.OrderByDescending(h => h.VisitedAt))
            {
                if (!string.IsNullOrEmpty(q) &&
                    !entry.Title.ToLowerInvariant().Contains(q) &&
                    !entry.Url.ToLowerInvariant().Contains(q))
                    continue;

                var item = new ListViewItem(entry.Title) { Tag = entry.Url };
                item.SubItems.Add(entry.Url);
                item.SubItems.Add(entry.VisitedAt.ToLocalTime().ToString("g"));
                list.Items.Add(item);
            }
        }

        Populate("");
        search.TextChanged += (_, _) => Populate(search.Text);

        list.DoubleClick += (_, _) => OpenSelected(list);

        var openBtn = new Button { Text = "Открыть", Width = 110, Height = 34 };
        BrowserThemeApplier.ApplyPrimaryButton(openBtn);
        openBtn.Click += (_, _) => OpenSelected(list);

        var bottom = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 52,
            Padding = new Padding(12),
            BackColor = BrowserTheme.ToolbarBg
        };
        openBtn.Dock = DockStyle.Right;
        bottom.Controls.Add(openBtn);

        Controls.Add(list);
        Controls.Add(bottom);
        Controls.Add(search);
    }

    private void OpenSelected(ListView list)
    {
        if (list.SelectedItems.Count == 0) return;
        SelectedUrl = list.SelectedItems[0].Tag as string;
        DialogResult = DialogResult.OK;
        Close();
    }
}
