namespace Arrowgene.Services.Playground
{
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
