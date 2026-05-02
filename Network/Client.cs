using System.Net;
using System.Net.Sockets;

namespace cookie.Cheats.Network
{
    public class Client
    {
        private Socket m_socket = null;
        private Connection m_connection = null;

        public Client(EndPoint endPoint)
        {
            m_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_socket.Connect(endPoint);
            m_connection = new Connection(m_socket);
        }
    }
}