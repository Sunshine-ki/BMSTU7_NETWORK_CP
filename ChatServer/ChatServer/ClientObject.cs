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
                Nickname = GetNickname();
                _server.AddNickname(Nickname);
                selectAction();

                Console.WriteLine($"{Nickname} connect");
                //string message;
                //byte[] data;
                while (true)
                {
                    try
                    {
                        var data = ReadServices.GetByteArray(Stream);
                        _server.SendToInterlocutor(Nickname, data);
                    }
                    catch
                    {
                        var msg = $"{Nickname}: disconnect";
                        //_server.SendToInterlocutor(Nickname, msg);
                        Console.WriteLine(msg);
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
                Console.WriteLine($"{Nickname}: finally");
                _server.RemoveConnection(Id);
                _server.RemoveNicknames(Nickname);
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
        private string GetNickname()
        {
            var nickname = ReadServices.GetMessage(Stream);

            while (_server.ExistsNickname(nickname))
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