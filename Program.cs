namespace Browser
{
    internal static class Program
    {
        /// <summary>
        ///  Основная точка входа в приложение
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new Views.MainView());
        }
    }
}