using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace cookie.Cheats.Network
{
    public class Connection : IDisposable
    {
        private Socket Socket = null;
        public readonly ConcurrentQueue<Message> SendQueue = new ConcurrentQueue<Message>();
        public readonly ConcurrentQueue<Message> ReceiveQueue = new ConcurrentQueue<Message>();
       
        private ConcurrentDictionary<Socket, Connection> Connections = new ConcurrentDictionary<Socket, Connection>();

        private readonly CancellationTokenSource m_token = new CancellationTokenSource();
        
        private readonly byte[] m_receiveBuffer = null;
        private readonly byte[] m_sendBuffer = null;
        
        private readonly JsonSerializerSettings m_settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto // ✅ Preserves real type
        };
        
        public Connection(Socket socket, ConcurrentDictionary<Socket, Connection> connections, int size = 1024 * 20) : this(socket, size)
        {
            Connections = connections;
        }

        public Connection(Socket socket, int size = 1024 * 20)
        {
            m_receiveBuffer =  new byte[size];
            m_sendBuffer = new byte[size];
            Socket = socket;
            
            Task.Run(Receive);
            Task.Run(Send);
        }

        private void Receive()
        {
            while (!m_token.IsCancellationRequested)
            {
                try
                {
                    var lenght = Socket.Receive(m_receiveBuffer);
                    
                    if(lenght == 0) break;
                    
                    var json = Encoding.UTF8.GetString(m_receiveBuffer, 0, lenght);

                    using var stringReader = new StringReader(json);
                    using var reader = new JsonTextReader(stringReader)
                    {
                        SupportMultipleContent = true,
                    };

                    while (reader.Read())
                    {
                        var jobject = JObject.Load(reader);
                        var message = JsonConvert.DeserializeObject<Message>(jobject.ToString(), m_settings);
                        ReceiveQueue.Enqueue(message);
                    }
                }
                catch (ObjectDisposedException)
                {
                    CancelAndRemove();
                    break;
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

        private void Send()
        {
            while (!m_token.IsCancellationRequested)
            {
                try
                {
                    if (SendQueue.Count == 0)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    if (!SendQueue.TryDequeue(out var message)) continue;
                    var json = JsonConvert.SerializeObject(message, m_settings);
                    var lenght = json.Length;
                    Encoding.UTF8.GetBytes(json, 0, lenght, m_sendBuffer, 0);
                    lenght = Socket.Send(m_sendBuffer, 0, lenght, SocketFlags.None);
                }
                catch (ObjectDisposedException)
                {
                    CancelAndRemove();
                    break;
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }

        private void CancelAndRemove()
        {
            m_token.Cancel();
            Connections?.TryRemove(Socket, out _);
        }

        public void Dispose()
        {
            if (m_token != null)
            {
                m_token.Cancel();
                m_token.Dispose();
            }

            if (Socket != null)
            {
                Socket.Close();
                Socket?.Dispose();
            }
        }
    }
}