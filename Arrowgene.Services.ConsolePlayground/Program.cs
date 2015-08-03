namespace SvrKitConsolePlayground
{
    using System;

    public class Program
    {
        public static void Main(string[] args)
        {
            Proxy p = new Proxy();
            Console.ReadKey();
            p.close();
        }
    }
}
