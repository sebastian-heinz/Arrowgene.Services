namespace GuiServer.ServerImplementation.ViewModel
{
    using GuiServer.ServerImplementation.ViewModel;
    using MarrySocket.MServer;
    using System.Collections.ObjectModel;

    public class ClientViewModelContainer
    {
        private ObservableCollection<ClientViewModel> clientViewModels;

        public ClientViewModelContainer()
        {
            this.clientViewModels = new ObservableCollection<ClientViewModel>();
        }

        public ObservableCollection<ClientViewModel> ClientViewModels { get { return this.clientViewModels; } }

        public ClientViewModel GetClientViewModel(ClientSocket clientSocket)
        {
            foreach (ClientViewModel clientViewModel in this.clientViewModels)
            {
                if (clientViewModel.Id == clientSocket.Id)
                    return clientViewModel;
            }
            return null;
        }

        public void Add(ClientViewModel clientViewModel)
        {
            this.clientViewModels.Add(clientViewModel);
        }

        public void Remove(ClientViewModel clientViewModel)
        {
            this.clientViewModels.Remove(clientViewModel);
        }

        public void Clear()
        {
            this.clientViewModels.Clear();
        }
    }
}
