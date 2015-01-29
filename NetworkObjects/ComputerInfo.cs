namespace NetworkObjects
{
    using System;

    [Serializable]
    public class ComputerInfo
    {
        public string Device { get; set; }
        public string HostName { get; set; }
        public int LogonCount { get; set; }

        public ComputerInfo(string hostName)
        {
            this.HostName = hostName;
        }
    }
}
