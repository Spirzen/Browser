namespace Browser.UI;

public static class BrowserThemeApplier
{
    public static void ApplyForm(Form form)
    {
        form.BackColor = BrowserTheme.WindowBg;
        form.ForeColor = BrowserTheme.Foreground;
        form.Font = BrowserTheme.UiFont;
    }

    public static void ApplyPanel(Panel panel, Color? bg = null)
    {
        panel.BackColor = bg ?? BrowserTheme.WindowBg;
        panel.ForeColor = BrowserTheme.Foreground;
    }

    public static void ApplyLabel(Label label, bool muted = false)
    {
        label.ForeColor = muted ? BrowserTheme.Muted : BrowserTheme.Foreground;
        label.BackColor = Color.Transparent;
    }

    public static void ApplyTextBox(TextBox box)
    {
        box.BackColor = BrowserTheme.AddressBg;
        box.ForeColor = BrowserTheme.Foreground;
        box.BorderStyle = BorderStyle.FixedSingle;
        box.Font = BrowserTheme.AddressFont;
    }

    public static void ApplyToolbarButton(Button button)
    {
        button.UseVisualStyleBackColor = false;
        button.FlatStyle = FlatStyle.Flat;
        button.BackColor = BrowserTheme.ButtonBg;
        button.ForeColor = BrowserTheme.Foreground;
        button.Font = BrowserTheme.ToolbarFont;
        button.FlatAppearance.BorderColor = BrowserTheme.Border;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = BrowserTheme.ButtonHover;
        button.FlatAppearance.MouseDownBackColor = BrowserTheme.TabInactive;
        button.Cursor = Cursors.Hand;
        button.EnabledChanged += (_, _) => UpdateButtonEnabledLook(button);
        UpdateButtonEnabledLook(button);
    }

    public static void ApplyPrimaryButton(Button button)
    {
        button.UseVisualStyleBackColor = false;
        button.FlatStyle = FlatStyle.Flat;
        button.BackColor = BrowserTheme.Accent;
        button.ForeColor = Color.White;
        button.Font = BrowserTheme.UiFont;
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = BrowserTheme.AccentHover;
        button.Cursor = Cursors.Hand;
    }

    public static void ApplySecondaryButton(Button button)
    {
        ApplyToolbarButton(button);
        button.ForeColor = BrowserTheme.Foreground;
    }

    public static void ApplyBookmarkChip(Button button)
    {
        button.UseVisualStyleBackColor = false;
        button.FlatStyle = FlatStyle.Flat;
        button.BackColor = BrowserTheme.ButtonBg;
        button.ForeColor = BrowserTheme.Link;
        button.Font = BrowserTheme.UiFont;
        button.FlatAppearance.BorderColor = BrowserTheme.Border;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = BrowserTheme.ButtonHover;
        button.Cursor = Cursors.Hand;
        button.Padding = new Padding(8, 2, 8, 2);
        button.AutoSize = true;
        button.MinimumSize = new Size(48, 26);
    }

    public static void ApplyMenuStrip(MenuStrip menu)
    {
        menu.BackColor = BrowserTheme.ToolbarBg;
        menu.ForeColor = BrowserTheme.Foreground;
        menu.Renderer = new LightMenuRenderer();
        StyleMenuItems(menu.Items);
    }

    public static void ApplyTabControl(TabControl tabs)
    {
        tabs.BackColor = BrowserTheme.TabBarBg;
        tabs.ForeColor = BrowserTheme.Foreground;
        tabs.Font = BrowserTheme.TabFont;
    }

    public static void ApplyListView(ListView list)
    {
        list.BackColor = BrowserTheme.AddressBg;
        list.ForeColor = BrowserTheme.Foreground;
        list.BorderStyle = BorderStyle.FixedSingle;
        list.Font = BrowserTheme.UiFont;
    }

    public static void UpdateNavButtons(Button back, Button forward)
    {
        UpdateButtonEnabledLook(back);
        UpdateButtonEnabledLook(forward);
    }

    private static void UpdateButtonEnabledLook(Button button)
    {
        if (button.BackColor == BrowserTheme.Accent)
            return;

        if (button.Enabled)
        {
            button.ForeColor = BrowserTheme.Foreground;
            button.BackColor = BrowserTheme.ButtonBg;
        }
        else
        {
            button.ForeColor = BrowserTheme.DisabledText;
            button.BackColor = BrowserTheme.DisabledBg;
        }
    }

    private static void StyleMenuItems(ToolStripItemCollection items)
    {
        foreach (ToolStripItem item in items)
        {
            item.ForeColor = BrowserTheme.Foreground;
            item.BackColor = BrowserTheme.AddressBg;
            if (item is ToolStripMenuItem menuItem)
                StyleMenuItems(menuItem.DropDownItems);
        }
    }

    private sealed class LightMenuRenderer : ToolStripProfessionalRenderer
    {
        public LightMenuRenderer() : base(new LightColorTable()) { }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            var rect = new Rectangle(Point.Empty, e.Item.Size);
            var bg = e.Item.Selected ? BrowserTheme.ButtonHover : BrowserTheme.AddressBg;
            using var brush = new SolidBrush(bg);
            e.Graphics.FillRectangle(brush, rect);
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = BrowserTheme.Foreground;
            base.OnRenderItemText(e);
        }
    }

    private sealed class LightColorTable : ProfessionalColorTable
    {
        public override Color MenuItemSelected => BrowserTheme.ButtonHover;
        public override Color MenuItemBorder => BrowserTheme.Border;
        public override Color ToolStripDropDownBackground => BrowserTheme.AddressBg;
        public override Color ImageMarginGradientBegin => BrowserTheme.AddressBg;
        public override Color ImageMarginGradientMiddle => BrowserTheme.AddressBg;
        public override Color ImageMarginGradientEnd => BrowserTheme.AddressBg;
        public override Color MenuBorder => BrowserTheme.Border;
        public override Color SeparatorDark => BrowserTheme.Border;
        public override Color SeparatorLight => BrowserTheme.Border;
    }
}
