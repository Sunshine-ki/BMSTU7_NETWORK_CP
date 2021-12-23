using System;
using System.Threading;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.Numerics;
using NetworkServices;

namespace ChatClient
{
    class Program
    {
        private static Client client;

        static void Main(string[] args)
        {
            try
            {
                client = new Client();

                Thread receiveThread = new Thread(new ThreadStart(client.ReceiveMessage));
                receiveThread.Start();

                Console.WriteLine($"[You communicate under: {client.UserName}]");
                client.SendMessage();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                client.Disconnect();
            }
        }

    
    }
}