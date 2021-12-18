using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using NetworkServices;

namespace ChatServer
{
    public class ClientPairs : List<KeyValuePair<ClientObject, ClientObject>> { }

    public class ServerObject
    {
        private int _port = 8888;

        // Сервер для прослушивания
        static TcpListener _tcpListener; 
        
        /// Все клиенты, ожидающие подключаения к ним собеседника
        List<ClientObject> _waitingClients = new List<ClientObject>(); 
        
        // Все пары клиентов, которые соединились друг с другом
        ClientPairs _clientPairs = new ClientPairs();

        /// <summary>
        /// Прослушивание входящих подключений
        /// </summary>
        protected internal void Listen()
        {
            try
            {
                _tcpListener = new TcpListener(IPAddress.Any, _port);
                _tcpListener.Start();
                Console.WriteLine("The server is running. Waiting for connections...");
                listen();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Disconnect();
            }
        }

        private void listen()
        {
            while (true)
            {
                var tcpClient = _tcpListener.AcceptTcpClient();
                var clientObject = new ClientObject(tcpClient, this);

                var clientThread = new Thread(new ThreadStart(clientObject.Process));
                clientThread.Start();
            }
        }

        /// <summary>
        /// Добавляет пару в массив пар, удаляет клиента из списка ожидающих.
        /// </summary>
        /// <param name="firstClient">Первый клиент</param>
        /// <param name="nickname">Уникальный индетификатор второго клиента</param>
        protected internal void AddClientPair(ClientObject firstClient, string nickname)
        {
            if (!ExistsWaitingClient(nickname)) return;

            var secondClient = _waitingClients.FirstOrDefault(c => c.Nickname == nickname);
            _waitingClients.Remove(secondClient);
            _clientPairs.Add(new KeyValuePair<ClientObject, ClientObject>(firstClient, secondClient));

            Console.WriteLine($"Pair is created {nickname} --- {firstClient.Nickname}");
        }

        /// <summary>
        /// Отпарялет сообщение собеседнику
        /// </summary>
        /// <param name="nickname">Уникальный инлетификатор отправителя</param>
        /// <param name="message"></param>
        protected internal void SendToInterlocutor(string nickname, string message)
        {
            NetworkStream stream = null;

            foreach (var cp in _clientPairs)
            {
                if (cp.Key.Nickname.Equals(nickname))
                {
                    stream = cp.Value.Stream;
                    break;
                }
                else if(cp.Value.Nickname.Equals(nickname))
                {
                    stream = cp.Key.Stream;
                    break;
                }
            }

            if (stream != null)
                WriteServices.SendString(stream, message);
        }

        protected internal bool ExistsWaitingClient(string nickname)
        {
            return _waitingClients.Exists(c => c.Nickname == nickname);
        }

        protected internal void AddWaitingClient(ClientObject clientObject)
        {
            _waitingClients.Add(clientObject);
        }

        protected internal void SendByNickname(string nickname, string message)
        {
            var client = _waitingClients.FirstOrDefault(x => x.Nickname.Equals(nickname));
            WriteServices.SendString(client.Stream, message);
        }

        /// <summary>
        /// Удяляет клиента из списка ожидающих клиентов или из списка пар сконнектившихся клиентов.
        /// </summary>
        /// <param name="id"></param>
        protected internal void RemoveConnection(string id)
        {
            var client = _waitingClients.FirstOrDefault(c => c.Id == id);

            if (client is null)
            {
                client = _clientPairs.FirstOrDefault(kvp => kvp.Key.Id == id).Key;
                if (client is null)
                    client = _clientPairs.FirstOrDefault(kvp => kvp.Value.Id == id).Value;
            }

            if (client != null)
                _waitingClients.Remove(client);
        }

        /// <summary>
        /// Отключение всех клиентов
        /// </summary>
        protected internal void Disconnect()
        {
            _tcpListener.Stop();

            _waitingClients.ForEach((client) => { client.Close(); });
            _clientPairs.ForEach((kvp) => { kvp.Key.Close(); kvp.Value.Close(); });
            
            Environment.Exit(0); // Завершаем процесс
        }
    }
}