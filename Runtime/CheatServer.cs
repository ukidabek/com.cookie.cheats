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
        public const int GetCheatsMessage = 1;
        public const int SetPayload = 2;
        public const int UpdateCheat = 3;
        
        [SerializeField] private int m_discoverPort = 2137;
        [SerializeField] private int m_listenPort = 2138;
        [SerializeField, Min(1)] private int m_connectionCount = 1;
        
        private Socket m_broadcast = null;
        private Socket m_listen = null;
        
        private IPAddress m_listenAddress = null;

        private Dictionary<Type, ICheatHandler> m_cheatChandlerDictionary;
        private Dictionary<int, MessageHandler> m_messageHandlerDictionary;
        private Dictionary<Socket, ConcurrentQueue<byte[]>> m_messageQueueDictionary = new  Dictionary<Socket, ConcurrentQueue<byte[]>>();
        public IEnumerable<ICheat> Cheats => CheatDatabase.Instance.ChetDictionary.Values;
        
        private Queue<ICheat> m_cheatValidationQueue = new Queue<ICheat>(100);

        private void Start()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsClass && !type.IsAbstract)
                .ToArray();

            var cheatHandlerType = typeof(ICheatHandler);
            m_cheatChandlerDictionary = types
                .Where(type => cheatHandlerType.IsAssignableFrom(type))
                .Select(type => (ICheatHandler)Activator.CreateInstance(type))
                .ToDictionary(handler => handler.CheatType);

            var messageHandlerType = typeof(MessageHandler);
            var cheatServer = new object[] { this };
            m_messageHandlerDictionary = types
                .Where(type => messageHandlerType.IsAssignableFrom(type))
                .Select(type => (MessageHandler)Activator.CreateInstance(type, cheatServer))
                .ToDictionary(handler => handler.Id);

            m_broadcast = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            m_broadcast.Bind(new IPEndPoint(IPAddress.Any, m_discoverPort));
            _ = BroadcastListeningTask();

            m_listenAddress = Dns.GetHostAddresses(Dns.GetHostName()).First(ip => ip.AddressFamily == AddressFamily.InterNetwork);

            m_listen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_listen.Bind(new IPEndPoint(m_listenAddress, m_listenPort));
            m_listen.Listen(m_connectionCount);
            _ = ConnectionAccept();
            
            
        }

        private void Update()
        {
            if (m_cheatValidationQueue.Count == 0)
            {
                foreach (var cheatHandler in Cheats)
                    m_cheatValidationQueue.Enqueue(cheatHandler);
            }
            
            var cheat = m_cheatValidationQueue.Dequeue();
            if (!CheatDatabase.Instance.ChetDictionary.ContainsKey(cheat.ID)) return;
            if (cheat is IValueCheat { IsDirty: true } valueCheat)
            {
                var message = new Message(UpdateCheat, new[]
                {
                    cheat.ID,
                    valueCheat.Get(),
                });
                var data = SerializeMessage(message);
                foreach (var concurrentQueue in m_messageQueueDictionary.Values)
                    concurrentQueue.Enqueue(data);
            }
            m_cheatValidationQueue.Enqueue(cheat);
        }

        private async Task ConnectionAccept()
        {
            await Awaitable.BackgroundThreadAsync();
            while (true)
            {
                try
                {
                    var socket = await m_listen.AcceptAsync();
                    var messageQueue = new ConcurrentQueue<byte[]>();
                    await Awaitable.MainThreadAsync();
                    m_messageQueueDictionary.Add(socket, messageQueue);
                    await Awaitable.BackgroundThreadAsync();
#pragma warning disable CS4014
                    Task.Run(() => MessageHandling(socket, messageQueue));
                    Task.Run(() => ResponseHandling(socket, messageQueue));
#pragma warning restore CS4014
                    await Task.Yield();
                }
                catch (ObjectDisposedException)
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

        private async Task ResponseHandling(Socket socket, ConcurrentQueue<byte[]> queue)
        {
            await Awaitable.BackgroundThreadAsync();
       
            while (true)
            {
                try
                {
                    if (!socket.Connected) break;

                    if (queue.TryDequeue(out var message))
                        socket.Send(message);
                    else
                        await Task.Yield();
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }
        }

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
                output = default(T);
                return 0;
            }
            
            using var messageStream = new MemoryStream(buffer,  0, lenght);
            output = (T)binaryFormatter.Deserialize(messageStream);
            
            return lenght;
        }

        private async Task MessageHandling(Socket socket, ConcurrentQueue<byte[]> queue)
        {
            await Awaitable.BackgroundThreadAsync();
            var data = new byte[4096];
            
            while (true)
            {
                try
                {
                    if (0 == ReceiveMessage<Message>(socket, out var message, data))
                    {
                        await Awaitable.MainThreadAsync();
                        m_messageQueueDictionary.Remove(socket);
                        break;
                    }
                    
                    if (!m_messageHandlerDictionary.TryGetValue(message.ID, out var handler)) continue;

                    var response = handler.Handle(message.Payload);
                    
                    if (response == null) continue;
                    
                    queue.Enqueue(SerializeMessage(response));
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

        public void NewMethod(CheatPayload payload)
        {
            var cheat = CheatDatabase.Instance.ChetDictionary[payload.ID];
            var type = cheat.GetType();
            m_cheatChandlerDictionary[type].Handle(cheat, payload);
        }

        private void OnDestroy()
        {
            m_broadcast.Close();
            m_broadcast.Dispose();

            m_listen.Close();
            m_listen.Dispose();
        }
        
        private async Task BroadcastListeningTask()
        {
            await Awaitable.BackgroundThreadAsync();
            var data = new byte[sizeof(int)];
            EndPoint source = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                try
                {
                    if (m_broadcast.Available > 0)
                    {
                        var received = m_broadcast.ReceiveFrom(data, ref source);
                        
                        if (received != sizeof(int)) continue;
                        
                        var message = BitConverter.ToInt32(data, 0);

                        if (message != DiscoverMessage) continue;
                        
                        data = Encoding.UTF8.GetBytes($"{m_listenAddress}:{m_listenPort}");
                        m_broadcast.SendTo(data, source);
                    }
                    else
                        await Task.Yield();
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }
        }
    }
}