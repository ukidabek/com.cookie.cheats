using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace cookie.Cheats.Network
{
    public class Connection
    {
        private Socket Socket = null;
        public ConcurrentQueue<Message> SendQueue = new ConcurrentQueue<Message>();
        public ConcurrentQueue<Message> ReceiveQueue = new ConcurrentQueue<Message>();
       
        private ConcurrentDictionary<Socket, Connection> Connections = new ConcurrentDictionary<Socket, Connection>();

        private CancellationTokenSource m_token = new CancellationTokenSource();
        
        private byte[] m_receiveBuffer = new byte[1024];
        private byte[] m_sendBuffer = new byte[1024];
        
        public Connection(Socket socket, ConcurrentDictionary<Socket, Connection> connections) : this(socket)
        {
            Connections = connections;
            
        }

        public Connection(Socket socket)
        {
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
                    var json = Encoding.UTF8.GetString(m_receiveBuffer, 0, lenght);
                    var message = JsonUtility.FromJson<Message>(json);
                    ReceiveQueue.Enqueue(message);
                }
                catch (ObjectDisposedException)
                {
                    CancelAndRemove();
                    break;
                }
            }
        }

        private void Send()
        {
            while (!m_token.IsCancellationRequested)
            {
                try
                {
                    if (SendQueue.Count == 0) continue;
                    if (!SendQueue.TryDequeue(out var message)) continue;
                    var json = JsonUtility.ToJson(message);
                    var lenght = json.Length;
                    Encoding.UTF8.GetBytes(json, 0, lenght, m_sendBuffer, 0);
                    Socket.Send(m_sendBuffer, 0, lenght, SocketFlags.None);
                }
                catch (ObjectDisposedException)
                {
                    CancelAndRemove();
                    break;
                }
            }
        }

        private void CancelAndRemove()
        {
            m_token.Cancel();
            Connections.TryRemove(Socket, out _);
        }
    }
}