using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace NetworkServices
{
    public static class ReadServices
    {
        /// <summary>
        /// Чтение входящего сообщения и преобразование в строку
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static string GetMessage(NetworkStream stream)
        {
            var data = new byte[64];
            var builder = new StringBuilder();
            int bytes;

            do
            {
                bytes = stream.Read(data, 0, data.Length);
                if (bytes == 0) throw new Exception("Data read error");
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (stream.DataAvailable);

            return builder.ToString();
        }

        public static byte[] GetByteArray(NetworkStream stream)
        {
            var data = new byte[32];
            var result = new List<byte>();

            int bytes;
            do
            {
                bytes = stream.Read(data, 0, data.Length);
                if (bytes == 0) throw new Exception("Data read error");
                add(result, data, bytes);
            }
            while (stream.DataAvailable);

            return result.ToArray();
        }

        private static void add(List<byte> result, byte[] data, int bytes)
        {
            for (int i = 0; i < bytes; i++)
                result.Add(data[i]);
        }

        public static int GetNumber(NetworkStream stream)
        {
            var dataFrom = new byte[64];
            stream.Read(dataFrom, 0, dataFrom.Length);
            return BitConverter.ToInt32(dataFrom, 0);
        }
    }
}
