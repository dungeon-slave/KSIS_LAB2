namespace TRC
{
    class Program
    {
        static void Main()
        {
            Traceroute TC  = new();
            TC.TraceRoute();
            System.Console.ReadLine();
        }
    }
}
