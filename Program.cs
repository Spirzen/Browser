namespace Browser
{
    internal static class Program
    {
        /// <summary>
        ///  �������� ����� ����� � ����������
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new Views.MainView());
        }
    }
}