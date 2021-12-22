using NetworkServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace ChatClient
{
    public class Client
    {
        protected internal NetworkStream Stream { get; private set; }
        protected internal string UserName { get; private set; }
        protected internal string Interlocutor { get; private set; } = string.Empty;

        private TcpClient _client;
        private int _privateKey;

        private byte[] key =
            {
                0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
                0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16,
            };
        Encryption encryption;

        public Client()
        {
            _client = new TcpClient();
            _client.Connect(Constants.Host, Constants.Port);
            setup();
        }

        public Client(TcpClient client)
        {
            _client = client;
            setup();
        }

        private void setup()
        {
            Stream = _client.GetStream();
            // Получаем уникальный идентификатор (nickname)
            UserName = getNickname(); 
            // Выбираем режим: ожидаем подключения или подключаемся к собеседнику
            var action = selectAction();
            // Создаем общий приватный ключ
            doAction(action);

            encryption = new Encryption(key);
        }

        private string selectAction()
        {
            var action = string.Empty;

            while (!(action.Equals("wait") || action.Equals("connect")))
            {
                Console.Write("Select action: wait; connect: ");
                action = Console.ReadLine();
            }

            return action;
        }

        private void doAction(string action)
        {
            WriteServices.SendString(Stream, action);

            if (action.Equals("wait"))
            {
                _privateKey = DiffieHellman.Wait(Stream);
               
            }
            else if (action.Equals("connect"))
            {
                Interlocutor = chooseNicknameOfInterlocutor();
                _privateKey = DiffieHellman.Connect(Stream);
            }
        }

        private string chooseNicknameOfInterlocutor()
        {
            var interlocutor = "Interlocutor";
            var answer = 0;

            while (answer != 1)
            {
                Console.Write("Enter the name of the interlocutor: ");
                interlocutor = Console.ReadLine();

                if (interlocutor.Equals(UserName)) continue;

                WriteServices.SendString(Stream, interlocutor);
                answer = ReadServices.GetNumber(Stream);
            }

            return interlocutor;
        }

        private string getNickname()
        {
            var nickname = string.Empty;
            var answer = 0;

            while (answer != 1)
            {
                Console.Write("Enter your name: ");
                nickname = Console.ReadLine();

                WriteServices.SendString(Stream, nickname);
                answer = ReadServices.GetNumber(Stream);
            }

            return nickname;
        }

        /// <summary>
        /// Отправка сообщений
        /// </summary>
        public void SendMessage()
        {
            Console.WriteLine("Enter the message: ");

            while (true)
            {
                string message = Console.ReadLine();
                
                var data = encryption.ToAes256(message);
                WriteServices.SendByteArray(Stream, data);
            }
        }

        /// <summary>
        /// Получение сообщений и вывод на экран
        /// </summary>
        public void ReceiveMessage()
        {
            while (true)
            {
                try
                {
                    var data = ReadServices.GetByteArray(Stream);
                    var message = encryption.FromAes256(data);
                    Console.WriteLine($"{Interlocutor}: {message}");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"e = {e.Message}");
                    Console.WriteLine("Connection interrupted");
                    Console.ReadLine();
                    Disconnect();
                }
            }
        }

        public void Disconnect()
        {
            if (Stream != null) Stream.Close();
            if (_client != null) _client.Close();
            Environment.Exit(0); 
        }
    }
}
