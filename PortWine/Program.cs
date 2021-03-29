using System;
namespace PortWine
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Progammstart.");

            ReliableSerialPort port = new ReliableSerialPort("COM1", 9600, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
            port.DataReceived += Rec;
            port.Open();

            string request = "AT";
            while (true)
            {
                Console.WriteLine("AT-Befehl:");
                request = Console.ReadLine();

                if (request.Length == 0) break;

                port.WriteLine(request);
            }

            Console.WriteLine("Progammende.");
            Console.ReadKey();

            port.Close();
            port.Dispose();
        }

        static void Rec(object sender, DataReceivedArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(e.Data);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}


