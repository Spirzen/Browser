using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Web.WebView2.WinForms;

namespace Browser.Helpers
{
    /// <summary>
    /// Класс для выноса повторяющейся логики с WebView
    /// </summary>
    public static class WebViewHelper
    {
        /// <summary>
        /// Повторение перехода
        /// </summary>
        /// <param name="url"></param>
        /// <param name="webView"></param>
        /// <returns></returns>
        public static bool TryNavigate(string url, WebView2 webView)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                webView.Source = uri;
                return true;
            }
            return false;
        }
    }
}
