namespace GuiServer.ServerImplementation
{
    using GuiServer.ServerImplementation.ViewModel;
    using MarrySocket.MServer;
    using NetworkObjects;

    public class HandlePacket
    {
        private ClientViewModelContainer clientViewModelContainer;

        public HandlePacket(ClientViewModelContainer clientViewModelContainer)
        {
            this.clientViewModelContainer = clientViewModelContainer;
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
        }
    }
}
