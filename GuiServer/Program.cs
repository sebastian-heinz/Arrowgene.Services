namespace GuiServer
{
    using GuiServer.ViewImplementation;
    using System;

    public static class Program
    {
        [STAThreadAttribute()]
        public static void Main()
        {
            MainWindow mainWindow = new MainWindow();
            MainPresenter mainPresenter = new MainPresenter(mainWindow);
            mainPresenter.ShowWindow();
        }
    }
}
