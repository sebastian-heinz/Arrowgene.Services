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
            this.serverConfig.BufferSize = 700000;
            this.dispatcher = dispatcher;
            this.handlePacket = new HandlePacket(this.clientViewModelContainer, this.dispatcher);

        }

        protected override void Handle(int packetId, object receivedClass, ClientSocket clientSocket)
        {
            this.dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                 {
                     this.addLog(new LogViewModel(new Log("Packet Arrived!")));
                 }));

            this.handlePacket.Handle(packetId, receivedClass, clientSocket);
        }

        protected override void onClientConnected(MarrySocket.MServer.ClientSocket clientSocket)
        {
            this.dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                this.clientViewModelContainer.Add(new ClientViewModel(clientSocket));
            }));

            base.onClientConnected(clientSocket);
        }

        protected override void onClientDisconnected(MarrySocket.MServer.ClientSocket clientSocket)
        {
            this.dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                ClientViewModel clientViewModel = this.clientViewModelContainer.GetClientViewModel(clientSocket);
                this.clientViewModelContainer.Remove(clientViewModel);
            }));

            base.onClientDisconnected(clientSocket);
        }

        protected override void OnLogWrite(MarrySocket.MExtra.Logging.Log log)
        {
            this.dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
              this.addLog(new LogViewModel(log));
            }));

            base.OnLogWrite(log);
        }

        private void addLog(LogViewModel logViewModel)
        {
            logViewModel.CmdClearLog = new CommandHandler(() => this.ClearLog(logViewModel), this.CanClearLog());
            logViewModel.CmdClearAllLog = new CommandHandler(() => this.ClearAllLog(), this.CanClearAllLog());
            this.logViewModels.Add(logViewModel);
        }

        private bool CanClearAllLog()
        {
            if (this.logger.Count > 0)
                return true;
            else
                return false;
        }

        private void ClearAllLog()
        {
            this.logger.Clear();
            this.logViewModels.Clear();
        }

        private bool CanClearLog()
        {
            return true;
        }

        private void ClearLog(LogViewModel logViewModel)
        {
            if (logViewModel != null)
            {
                this.logger.Remove(logViewModel.Id);
                this.logViewModels.Remove(logViewModel);
            }
        }

    }
}
