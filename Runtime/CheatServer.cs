using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace cookie.Cheats
{
    [Serializable]
    public class CheatPayload
    {
        public readonly int ID;
        public readonly object[] Parameters;

        public CheatPayload(int id, object[] parameters)
        {
            ID = id;
            Parameters = parameters;
        }
    }

    public interface ICheatHandler
    {
        Type CheatType { get; }
        void Handle(ICheat cheat, CheatPayload payload);
        Task Update(ICheat cheat, Socket socket);
    }

    public class FieldCheatHandler : ICheatHandler
    {
        public Type CheatType => typeof(FieldCheat);
        
        public void Handle(ICheat cheat, CheatPayload payload)
        {
            var fieldCheat = (FieldCheat)cheat;
            fieldCheat.Set(payload.Parameters.First());
        }

        public Task Update(ICheat cheat, Socket socket)
        {
            throw new NotImplementedException();
        }
    }
    
    public class CheatServer : MonoBehaviour
    {
        public const int DiscoverMessage = 0;
        public const int GetCheatsMessage = 1;
        
        [SerializeField] private int m_discoverPort = 2137;
        [SerializeField] private int m_listenPort = 2138;
        [SerializeField, Min(1)] private int m_connectionCount = 1;
        
        private Socket m_broadcast = null;
        private Socket m_listen = null;
        
        private Task m_broadcastTask = null;
        private Task m_listenTask = null;

        private IPAddress m_listenAddress = null;
        
        private Dictionary<Type, ICheatHandler> m_cheatDictionary = new  Dictionary<Type, ICheatHandler>(30);
        
        private IEnumerator Start()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes());

            var cheatHandlerType = typeof(ICheatHandler);
            var count = 0;
            foreach (var type in types)
            {
                if (!type.IsAbstract &&
                    !type.IsInterface &&
                    cheatHandlerType.IsAssignableFrom(type))
                {
                    var instance = (ICheatHandler)Activator.CreateInstance(type);
                    m_cheatDictionary.Add(instance.CheatType, instance);
                }
                if (count++ != 100) continue;
                count = 0;
                yield return null;
            }
            
            m_broadcast = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            m_broadcast.Bind(new IPEndPoint(IPAddress.Any, m_discoverPort));

            m_broadcastTask = BroadcastListeningTask();
            
            m_listenAddress = Dns.GetHostAddresses(Dns.GetHostName())
                .First(ip => ip.AddressFamily == AddressFamily.InterNetwork);

            m_listen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_listen.Bind(new IPEndPoint(m_listenAddress, m_listenPort));
            m_listen.Listen(m_connectionCount);

            m_broadcastTask = ListeningTask();
        }

        private async Task ListeningTask()
        {
            var data = new byte[1024];
            while (true)
            {
                try
                {
                    var socket = await m_listen.AcceptAsync();
                    var lenght = socket.Receive(data);

                    if (lenght < sizeof(int)) continue;

                    var message = BitConverter.ToInt32(data, 0);
                    switch (message)
                    {
                        case GetCheatsMessage:
                            var cheats = CheatDatabase.Instance.ChetDictionary.Values
                                .Select(cheat => new CheatIdentifier(cheat))
                                .ToArray();

                            var formatter = new BinaryFormatter();
                            using (var memoryStream = new MemoryStream())
                            {
                                formatter.Serialize(memoryStream, cheats);
                                var buffer = memoryStream.ToArray();
                                socket.Send(BitConverter.GetBytes(buffer.Length));
                                socket.Send(buffer);
                            }

                            break;
                    }
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        private void OnDestroy()
        {
            m_broadcast?.Dispose();
            m_listen?.Dispose();
            m_broadcastTask.Dispose();
        }
        
        private async Task BroadcastListeningTask()
        {
            var data = new byte[1024];
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