namespace GuiServer.ServerImplementation.ViewModel
{
    using MarrySocket.MExtra.Logging;
    using System;
    using System.ComponentModel;
    using System.Windows.Input;

    public class LogViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int id;
        private string text;
        private LogType logType;
        private DateTime dateTime;
        private Log log;

        public LogViewModel(Log log)
        {
            this.log = log;
            this.Text = log.Text;
            this.Id = log.Id;
            this.LogType = log.LogType;
            this.DateTime = log.DateTime;

            this.CmdClearLog = new CommandHandler(() => ClearLog(), this.CanClearLog());
            this.CmdClearLog = new CommandHandler(() => ClearAllLog(), this.CanClearAllLog());
        }

        private bool CanClearAllLog()
        {
           return false;
        }

        private  void ClearAllLog()
        {
 
        }

        private bool CanClearLog()
        {
            return false;
        }

        private void ClearLog()
        {
         
        }

        public ICommand CmdClearLog { get; set; }
        public ICommand CmdClearAllLog { get; set; }
        public int Id { get { return this.id; } set { this.id = value; NotifyPropertyChanged("Id"); } }
        public string Text { get { return this.text; } set { this.text = value; NotifyPropertyChanged("Text"); } }
        public LogType LogType { get { return this.logType; } set { this.logType = value; NotifyPropertyChanged("LogType"); } }
        public DateTime DateTime { get { return this.dateTime; } set { this.dateTime = value; NotifyPropertyChanged("DateTime"); } }

        private void NotifyPropertyChanged(string Obj)
        {
            if (PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(Obj));
            }
        }
    }
}
