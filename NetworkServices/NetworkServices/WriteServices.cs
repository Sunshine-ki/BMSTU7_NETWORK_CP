using System;
using System.Net.Sockets;
using System.Text;

namespace NetworkServices
{
    public static class WriteServices
    {
        public static void SendNumber(NetworkStream stream, int num)
        {
            var data = BitConverter.GetBytes(num);
            writeToSream(stream, data);
        }

        public static void SendString(NetworkStream stream, string message)
        {
            var data = Encoding.Unicode.GetBytes(message);
            writeToSream(stream, data);
        }

        private static void writeToSream(NetworkStream stream, byte[] data)
        {
            stream.Write(data, 0, data.Length);
        }

    }
}
