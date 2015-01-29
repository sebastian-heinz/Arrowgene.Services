namespace GuiServer.ServerImplementation
{
    using GuiServer.ServerImplementation.ViewModel;
    using MarrySocket.MExtra.Layout;
    using MarrySocket.MExtra.Logging;
    using MarrySocket.MServer;
    using System;
    using System.Collections.ObjectModel;
    using System.Windows;
    using System.Windows.Threading;

    public class Server : ServerLayout
    {
        private ObservableCollection<LogViewModel> logViewModels;
        private HandlePacket handlePacket;
        private ClientViewModelContainer clientViewModelContainer;
        private Dispatcher dispatcher;

        public Server(ClientViewModelContainer clientViewModelContainer, ObservableCollection<LogViewModel> logViewModels, Dispatcher dispatcher)
        {
            this.clientViewModelContainer = clientViewModelContainer;
            this.logViewModels = logViewModels;
            this.dispatcher = dispatcher;
            this.handlePacket = new HandlePacket(this.clientViewModelContainer);
        }

        protected override void Handle(int packetId, object receivedClass, ClientSocket clientSocket)
        {
            this.dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
                 {
                     this.logViewModels.Add(new LogViewModel(new Log("Packet Arrived!")));
                 }));

            this.handlePacket.Handle(packetId, receivedClass, clientSocket);
        }

        protected override void onClientConnected(MarrySocket.MServer.ClientSocket clientSocket)
        {
            this.dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                this.clientViewModelContainer.Add(new ClientViewModel(clientSocket));
            }));

            base.onClientConnected(clientSocket);
        }

        protected override void onClientDisconnected(MarrySocket.MServer.ClientSocket clientSocket)
        {
            this.dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                ClientViewModel clientViewModel = this.clientViewModelContainer.GetClientViewModel(clientSocket);
                this.clientViewModelContainer.Remove(clientViewModel);
            }));

            base.onClientDisconnected(clientSocket);
        }

        protected override void OnLogWrite(MarrySocket.MExtra.Logging.Log log)
        {
            this.dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                this.logViewModels.Add(new LogViewModel(log));
            }));

            base.OnLogWrite(log);
        }

    }
}
