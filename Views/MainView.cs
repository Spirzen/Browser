using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace Browser.Views
{
    /// <summary>
    /// Основная визуальная часть формы
    /// </summary>
    public partial class MainView : Form
    {
        private TextBox addressBar;
        private Button navigateButton;
        private WebView2 webView;

        public MainView()
        {
            InitializeComponent();
            addressBar = new TextBox();
            navigateButton = new Button();
            webView = new WebView2();
            InitializeUI();
        }
        /// <summary>
        /// Инициализация интерфейса
        /// </summary>
        private void InitializeUI()
        {
            this.Text = "Мини-браузер";
            this.Size = new Size(1024, 768);
            this.BackColor = Color.FromArgb(240, 240, 240);

            addressBar = new TextBox
            {
                Dock = DockStyle.Top,
                Text = "https://spirzen.github.io/ ",
                Font = new Font("Segoe UI", 10),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                ForeColor = Color.Black,
                Height = 30
            };
            addressBar.KeyDown += AddressBar_KeyDown;
            this.Controls.Add(addressBar);

            Panel buttonPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 30,
                BackColor = Color.FromArgb(255, 255, 255)
            };
            this.Controls.Add(buttonPanel);

            AddStyledButton(buttonPanel, "Назад", 100, BackButton_Click);
            AddStyledButton(buttonPanel, "Вперёд", 100, ForwardButton_Click);
            AddStyledButton(buttonPanel, "Обновить", 100, RefreshButton_Click, DockStyle.Right);
            navigateButton = AddStyledButton(buttonPanel, "Перейти", 100, NavigateButton_Click, DockStyle.Right);

            webView = new WebView2
            {
                Dock = DockStyle.Fill
            };
            this.Controls.Add(webView);

            InitializeWebViewAsync();
        }
        /// <summary>
        /// Инициализация WebView
        /// </summary>
        private async void InitializeWebViewAsync()
        {
            await webView.EnsureCoreWebView2Async(null);
            webView.CoreWebView2.NavigationCompleted += WebView_NavigationCompleted;
        }

        private Button AddStyledButton(Panel panel, string text, int width, EventHandler clickHandler, DockStyle dock = DockStyle.Left)
        {
            var button = new Button
            {
                Dock = dock,
                Text = text,
                Width = width,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatAppearance = { BorderSize = 0 }
            };
            button.Click += clickHandler;
            panel.Controls.Add(button);
            return button;
        }
        /// <summary>
        /// Обработчик кнопки перехода
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NavigateButton_Click(object? sender, EventArgs e) => NavigateToUrl();
        /// <summary>
        /// Обработчик нажатия кнопки перехода - Enter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddressBar_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                NavigateToUrl();
            }
        }
        /// <summary>
        /// Обработчик кнопки "Назад"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackButton_Click(object? sender, EventArgs e) => webView.GoBack();
        /// <summary>
        /// Обработчик кнопки "Вперёд"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ForwardButton_Click(object? sender, EventArgs e) => webView.GoForward();
        /// <summary>
        /// Обработчик кнопки "Обновить"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshButton_Click(object? sender, EventArgs e) => webView.Reload();
        /// <summary>
        /// Переход к URL
        /// </summary>
        private void NavigateToUrl()
        {
            if (Uri.TryCreate(addressBar.Text, UriKind.Absolute, out Uri uri))
            {
                webView.Source = uri;
            }
            else
            {
                MessageBox.Show("Некорректный URL");
            }
        }
        /// <summary>
        /// Обработчик завершения перехода
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void WebView_NavigationCompleted(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                this.Text = $"Мини-браузер - {webView.CoreWebView2.DocumentTitle}";
            }
            else
            {
                MessageBox.Show("Не удалось загрузить страницу.");
            }
        }
    }
}