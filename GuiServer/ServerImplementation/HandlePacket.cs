namespace GuiServer.ServerImplementation
{
    using GuiServer.ServerImplementation.ViewModel;
    using GuiServer.ViewImplementation.Windows;
    using MarrySocket.MServer;
    using NetworkObjects;
    using System;
    using System.IO;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;
    using System.Drawing;


    public class HandlePacket
    {
        private ClientViewModelContainer clientViewModelContainer;
        private Dispatcher dispatcher;

        public HandlePacket(ClientViewModelContainer clientViewModelContainer, Dispatcher dispatcher)
        {
            this.clientViewModelContainer = clientViewModelContainer;
            this.dispatcher = dispatcher;
        }

        public void Handle(int packetId, object receivedClass, ClientSocket clientSocket)
        {
            if (receivedClass is ComputerInfo)
            {
                ComputerInfo computerInfo = receivedClass as ComputerInfo;
                ClientViewModel clientViewModel = this.clientViewModelContainer.GetClientViewModel(clientSocket);
                if (clientViewModel != null)
                {
                    clientViewModel.ComputerInfo = computerInfo;
                }
            }
            else if (receivedClass is ScreenShot)
            {
                ScreenShot screenShot = receivedClass as ScreenShot;
                BitmapImage image = null;
                try
                {
                    using (MemoryStream ms = new MemoryStream(screenShot.Screen))
                    {
                        image = new BitmapImage();

                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = ms;
                        image.EndInit();
                        image.Freeze();
                    }
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                }

                this.dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    ScreenShotWindow ssw = new ScreenShotWindow(image, "Client[" + clientSocket.Id.ToString() + "]" + DateTime.Now.ToString("dd-MM-yyyy hh:mm:ss"));
                    ssw.Show();
                }));

            }
        }
    }
}

