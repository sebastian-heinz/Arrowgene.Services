namespace GuiServer.ServerImplementation.ViewModel
{
    using MarrySocket.MServer;
    using NetworkObjects;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Input;

    public class ClientViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int id;
        private ClientSocket clientSocket;
        private ComputerInfo computerInfo;

        public ClientViewModel(ClientSocket clientSocket)
        {
            this.CmdDisconnect = new CommandHandler(() => Disconnect(), this.CanDisconnect());
            this.clientSocket = clientSocket;
            this.Id = clientSocket.Id;
        }

        public ICommand CmdDisconnect { get; set; }
        public int Id { get { return this.id; } set { this.id = value; NotifyPropertyChanged("Id"); } }
        public ComputerInfo ComputerInfo { get { return this.computerInfo; } set { this.computerInfo = value; NotifyPropertyChanged("ComputerInfo"); } }
        public string Ip { get { return this.clientSocket.Ip; } }


        private void Disconnect()
        {
            this.clientSocket.Close();
        }

        private bool CanDisconnect()
        {
            return true;
        }

        private void NotifyPropertyChanged(string Obj)
        {
            if (PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(Obj));
            }
        }

    }
}
