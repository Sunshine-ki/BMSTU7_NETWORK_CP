using System;
using System.Linq;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using NetworkServices;

namespace ChatServer
{
    public class ClientObject
    {
        protected internal string Id { get; private set; }
        protected internal string Nickname { get; private set; }
        protected internal NetworkStream Stream { get; private set; }

        private TcpClient _client;
        private ServerObject _server; 


        public ClientObject(TcpClient tcpClient, ServerObject serverObject)
        {
            Id = Guid.NewGuid().ToString();
            _client = tcpClient;
            _server = serverObject;
        }

        /// <summary>
        /// 1. Клиент выбирает уникальный индетификатор
        /// 2. Клиент выбирает режим (wait, connect)
        /// 3. Все дальнейшие сообщения отправляются собеседнику.
        /// </summary>
        public void Process()
        {
            try
            {
                Stream = _client.GetStream();
                Nickname = GetFreeNickname();
                selectAction();

                //while(server.ContainNicknames(Nickname))
                //{
                //    _ = ReadServices.GetMessage(Stream);
                //    //message = String.Format("{0}: {1}", Nickname, message);
                //    //Console.WriteLine(message);

                //    byte[] data = Encoding.Unicode.GetBytes("С вами еще никто не соединился");
                //    Stream.Write(data, 0, data.Length);
                //}

                Console.WriteLine($"{Nickname} connect");
                string message;
                while (true)
                {
                    try
                    {
                        message = ReadServices.GetMessage(Stream);
                        Console.WriteLine($"{Nickname}: {message}");
                        _server.SendToInterlocutor(Nickname, message);
                    }
                    catch
                    {
                        Console.WriteLine($"{Nickname}: disconnect");
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                _server.RemoveConnection(this.Id);
                Close();
            }
        }

        /// <summary>
        /// Позволяет клиенту выбрать одно из следующих действий.
        /// wait - добаить клиента в список ожидающих соединение.
        /// connect - подключиться к собеседнику.
        /// При подключении к собеседнку ожидает уникальный 
        /// индетификатор собеседника (никнейм).
        /// </summary>
        private void selectAction()
        {
            var action = ReadServices.GetMessage(Stream);
            if (action.Equals("wait"))
            {
                _server.AddWaitingClient(this);
            }
            else if (action.Equals("connect"))
            {
                var nicknameToConnect = ReadServices.GetMessage(Stream);
                while (!_server.ExistsWaitingClient(nicknameToConnect))
                {
                    WriteServices.SendNumber(Stream, 0);
                    nicknameToConnect = ReadServices.GetMessage(Stream);
                }
                _server.AddClientPair(this, nicknameToConnect);
                WriteServices.SendNumber(Stream, 1);
            }

        }

        /// <summary>
        /// Получает уникальный никнейм для пользователя
        /// </summary>
        /// <returns></returns>
        private string GetFreeNickname()
        {
            var nickname = ReadServices.GetMessage(Stream);

            while (_server.ExistsWaitingClient(nickname))
            {
                WriteServices.SendNumber(Stream, 0);
                nickname = ReadServices.GetMessage(Stream);
            }
            WriteServices.SendNumber(Stream, 1);

            return nickname;
        }

        protected internal void Close()
        {
            if (Stream != null) Stream.Close();
            if (_client != null) _client.Close();
        }
    }
}