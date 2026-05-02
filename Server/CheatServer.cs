using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace cookie.Cheats.Server
{
    public class CheatServer : MonoBehaviour
    {
        public const int DiscoverMessage = 0;
        public const int SynchronizeCheats = 1;
        public const int SetPayload = 2;
        public const int UpdateCheat = 3;
        public const int ReadyToReceiveData = 4;
        
        [SerializeField] private int m_discoverPort = 2137;
        [SerializeField] private int m_listenPort = 2138;
        [SerializeField, Min(1)] private int m_connectionCount = 1;
        
        private Dictionary<Type, IServerCheatHandler> m_cheatChandlerDictionary;
        private ConcurrentDictionary<int, MessageHandler> m_messageHandlerDictionary;
        private Dictionary<Socket, Connection> m_messageQueueDictionary = new  Dictionary<Socket, Connection>();
        public IEnumerable<ICheat> Cheats => CheatDatabase.Instance.ChetDictionary.Values;

        private ItemProcessor<int, ICheat> m_itemProcessor = null;

        private Network.Server m_server = null;
        
        private void Start()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsClass && !type.IsAbstract)
                .ToArray();

            var cheatHandlerType = typeof(IServerCheatHandler);
            m_cheatChandlerDictionary = types
                .Where(type => cheatHandlerType.IsAssignableFrom(type))
                .Select(type => (IServerCheatHandler)Activator.CreateInstance(type))
                .SelectMany(handler => handler.CheatType.Select(type => (type, handler)))
                .ToDictionary(pair => pair.type, pair => pair.handler);

            var messageHandlerType = typeof(MessageHandler);
            var cheatServer = new object[] { this };
            m_messageHandlerDictionary = new ConcurrentDictionary<int, MessageHandler>(types
                .Where(type => messageHandlerType.IsAssignableFrom(type))
                .Select(type => (MessageHandler)Activator.CreateInstance(type, cheatServer))
                .ToDictionary(handler => handler.Id));

            m_itemProcessor = new ItemProcessor<int, ICheat>(CheatDatabase.Instance.ChetDictionary, 
                cheat =>
                {
                    if (!m_messageQueueDictionary.Any()) return false;
                    if (cheat is not IValueCheat { IsDirty: true } valueCheat) return false;
                    
                    var message = new Message(UpdateCheat, new[]
                    {
                        cheat.ID,
                        valueCheat.ToSerializableObject(),
                    });
                    
                    var data = SerializeMessage(message);
                    foreach (var concurrentQueue in m_messageQueueDictionary.Values)
                    {
                        if (!concurrentQueue.ReadyToReceiveData) continue;
                        concurrentQueue.MessageQueue.Enqueue(data);
                    }

                    return false;
                },
                cheat => cheat.ID);

            m_server = new Network.Server();
            m_server.Start();
        }

        private void Update() => m_itemProcessor.Process();

        public static byte[] SerializeMessage(object payload)
        {
            var binaryFormater = new BinaryFormatter();
            using var memoryStream = new MemoryStream();
            binaryFormater.Serialize(memoryStream, payload);
            return memoryStream.ToArray();
        }

        public static void SendMessage(Socket socket, object payload) => socket.Send(SerializeMessage(payload));

        public static int ReceiveMessage<T>(Socket socket, out T output, byte[] buffer = null, int bufferSize = 4096)
        {
            var binaryFormatter = new BinaryFormatter();
            buffer ??= new byte[bufferSize];
            
            var lenght = socket.Receive(buffer);
            if (lenght == 0)
            {
                output = default;
                return 0;
            }
            
            using var messageStream = new MemoryStream(buffer,  0, lenght);
            output = (T)binaryFormatter.Deserialize(messageStream);
            
            return lenght;
        }

        private async Task MessageHandling(Connection connection)
        {
            await Awaitable.BackgroundThreadAsync();
            var data = new byte[4096];
            
            while (true)
            {
                try
                {
                    var socket = connection.Socket;
                    if (ReceiveMessage<Message>(socket, out var message, data) == 0)
                    {
                        await Awaitable.MainThreadAsync();
                        m_messageQueueDictionary.Remove(socket);
                        break;
                    }
                    
                    if (message.ID == ReadyToReceiveData)
                    {
                        connection.ReadyToReceiveData = true;
                        continue;
                    }
                    
                    if (!m_messageHandlerDictionary.TryGetValue(message.ID, out var handler)) continue;

                    var response = handler.Handle(message.Payload);
                    
                    if (response == null) continue;
                    
                    connection.MessageQueue.Enqueue(SerializeMessage(response));
                }
                catch (Exception e) when (e is ObjectDisposedException or SocketException or IOException)
                {
                    break;
                }
                catch (Exception e)
                {
                    await Awaitable.MainThreadAsync();
                    Debug.LogException(e);
                    await Awaitable.BackgroundThreadAsync();
                }
            }
        }

        public void InvokeCheat(CheatPayload payload)
        {
            var cheat = CheatDatabase.Instance.ChetDictionary[payload.ID];
            var type = cheat.GetType();
            m_cheatChandlerDictionary[type].Handle(cheat, payload);
        }

        private void OnDestroy()
        {
        }
    }
}