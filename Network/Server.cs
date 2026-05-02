using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace cookie.Cheats.Network
{
    public class Server : NetworkUser
    {
        public const string DiscoverMessage = "DISCOVER_SERVER";
        
        private string m_name = nameof(Server);
        private int m_discoverPort = 2137;
        private int m_listenPort = 2138;
        private int m_connectionCount = 1;
        private CancellationTokenSource m_token = new CancellationTokenSource();

        private Socket m_broadcast = null;
        private Socket m_listen = null;
        
        private IPAddress m_listenAddress = null;
        private ConcurrentQueue<Message> ReceiveQueue = new ConcurrentQueue<Message>();
        private ConcurrentDictionary<Socket, Connection> Connections = new ConcurrentDictionary<Socket, Connection>();
        
        private object m_lock = new object();
        private List<Message> m_helloMessagesList = new List<Message>(10);
        
        public Server(string name, int discoverPort, int listenPort, int connectionCount) : this()
        {
            m_name = name;
            m_discoverPort = discoverPort;
            m_listenPort = listenPort;
            m_connectionCount = connectionCount;
        }

        public Server()
        {
            var hostName = Dns.GetHostName();
            var hostAddresses = Dns.GetHostAddresses(hostName);
            m_listenAddress = hostAddresses.First(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }

        public void Start()
        {
            Task.Run(() => BroadcastListening(m_token.Token));
            Task.Run(() => AcceptConnection(m_token.Token));
        }

        public void SetHelloMessages(IEnumerable<Message> messages)
        {
            lock (m_lock)
            {
                m_helloMessagesList.Clear();
                m_helloMessagesList.AddRange(messages);
            }
        }
        
        
        private void AcceptConnection(CancellationToken token)
        {
            m_listen = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_listen.Bind(new IPEndPoint(m_listenAddress, m_listenPort));
            m_listen.Listen(m_connectionCount);
            
            while (!token.IsCancellationRequested)
            {
                try
                {
                    var socket = m_listen.Accept();
                    var connection = new Connection(socket);
                    Connections.TryAdd(socket, connection);
                    lock (m_lock)
                    {                            
                        var connectionSendQueue = connection.SendQueue;
                        foreach (var message in m_helloMessagesList) 
                            connectionSendQueue.Enqueue(message);
                    }
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }
        }
        
        private void BroadcastListening(CancellationToken token)
        {
            var data = new byte[DiscoverMessage.Length];
            EndPoint source = new IPEndPoint(IPAddress.Any, 0);
            
            m_broadcast = new  Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            m_broadcast.Bind(new IPEndPoint(IPAddress.Any, m_discoverPort));
            
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (m_broadcast.Available <= 0) continue;

                    var received = m_broadcast.ReceiveFrom(data, ref source);

                    if (received != DiscoverMessage.Length) continue;
                    
                    var message = Encoding.UTF8.GetString(data, 0, received);
                    
                    if (message != DiscoverMessage) continue;

                    data = Encoding.UTF8.GetBytes($"{m_name}:{m_listenAddress}:{m_listenPort}");
                    m_broadcast.SendTo(data, source);
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
            }
        }
        
        public override void Dispose()
        {
            m_token.Cancel();
            m_broadcast?.Dispose();
            m_listen?.Dispose();
        }
    }
}