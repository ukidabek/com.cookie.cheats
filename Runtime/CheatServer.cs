using System;
using System.Collections;
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
    public class CheatServer : MonoBehaviour
    {
        public const int DiscoverMessage = 0;
        public const int GetCheatsMessage = 1;
        
        [SerializeField] private int m_discoverPort = 2137;
        [SerializeField] private int m_listenPort = 2138;
        
        private Socket m_broadcast = null;
        private Socket m_listen = null;
        
        private Task m_broadcastTask = null;
        private Task m_listenTask = null;

        private IPAddress m_listenAddress = null;
        
        private void Awake()
        {
            m_broadcast = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            m_broadcast.Bind(new IPEndPoint(IPAddress.Any, m_discoverPort));

            m_broadcastTask = BroadcastListeningTask();
            
            m_listenAddress = Dns.GetHostAddresses(Dns.GetHostName())
                .First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            
            m_listen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_listen.Bind(new IPEndPoint(m_listenAddress, m_listenPort));
            m_listen.Listen(1);

            m_broadcastTask = ListeningTask();
        }

        private async Task ListeningTask()
        {
            var data = new byte[1024];
            EndPoint source = new IPEndPoint(IPAddress.Any, 0);
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
                            var cheats = CheatDatabase.Instance.ChetDictionary.Values.ToArray();
                            var formatter = new BinaryFormatter();
                            using (var memoryStream = new MemoryStream())
                            {
                                formatter.Serialize(memoryStream, cheats);
                                var buffer = memoryStream.ToArray();
                                await socket.SendAsync(buffer, SocketFlags.None);
                            }

                            break;
                    }
                    socket.Close();
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }
        }

        private void OnDestroy()
        {
            m_broadcast?.Dispose();
            m_listen?.Dispose();
            m_broadcastTask.Dispose();
        }

        [ContextMenu("Broadcast")]
        public void Test()
        {
            var cheats = CheatDatabase.Instance.ChetDictionary.Values.ToArray();
            var formatter = new BinaryFormatter();
            using (var memoryStream = new MemoryStream())
            {
                formatter.Serialize(memoryStream, cheats);
                var buffer = memoryStream.ToArray();
               
            }
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