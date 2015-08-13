namespace Arrowgene.Services.Playground
{
    using System;

    public class Program
    {
        public static void Main(string[] args)
        {
            Proxy p = new Proxy();
            p.Run();
            p.close();
        }
    }
}
