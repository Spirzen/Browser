using Browser.Models;
using Browser.Services;
using Browser.UI;

namespace Browser.Views;

public sealed class BookmarksDialog : Form
{
    public string? SelectedUrl { get; private set; }
    private readonly List<BookmarkItem> _bookmarks;

    public BookmarksDialog(List<BookmarkItem> bookmarks)
    {
        _bookmarks = bookmarks;
        Text = "Закладки";
        Size = new Size(600, 460);
        StartPosition = FormStartPosition.CenterParent;
        BrowserThemeApplier.ApplyForm(this);

        var list = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true
        };
        BrowserThemeApplier.ApplyListView(list);
        list.Columns.Add("Название", 200);
        list.Columns.Add("URL", 360);

        void Refresh()
        {
            list.Items.Clear();
            foreach (var b in _bookmarks.OrderBy(x => x.Title))
            {
                var item = new ListViewItem(b.Title) { Tag = b.Url };
                item.SubItems.Add(b.Url);
                list.Items.Add(item);
            }
        }

        Refresh();
        list.DoubleClick += (_, _) => OpenSelected(list);

        var deleteBtn = MakeButton("Удалить", primary: false);
        deleteBtn.Click += (_, _) =>
        {
            if (list.SelectedItems.Count == 0) return;
            var url = list.SelectedItems[0].Tag as string;
            _bookmarks.RemoveAll(b => b.Url == url);
            AppDataService.SaveBookmarks(_bookmarks);
            Refresh();
        };

        var openBtn = MakeButton("Открыть", primary: true);
        openBtn.Click += (_, _) => OpenSelected(list);

        var bottom = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 52,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(12),
            BackColor = BrowserTheme.ToolbarBg
        };
        bottom.Controls.Add(openBtn);
        bottom.Controls.Add(deleteBtn);

        Controls.Add(list);
        Controls.Add(bottom);
    }

    private void OpenSelected(ListView list)
    {
        if (list.SelectedItems.Count == 0) return;
        SelectedUrl = list.SelectedItems[0].Tag as string;
        DialogResult = DialogResult.OK;
        Close();
    }

    private static Button MakeButton(string text, bool primary)
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
