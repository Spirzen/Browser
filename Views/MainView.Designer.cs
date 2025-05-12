using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Browser.Views
{
    /// <summary>
    /// Часть класса MainView
    /// </summary>
    partial class MainView
    {
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Утилизация компонентов
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        /// <summary>
        /// Инициализация компонентов
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Text = "MainView";
            this.Size = new System.Drawing.Size(1024, 768);
        }
    }
}
