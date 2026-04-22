using System.Collections.Concurrent;
using System.Net.Sockets;

namespace cookie.Cheats.Server
{
    public class Connection
    {
        public readonly Socket Socket;
        public readonly ConcurrentQueue<byte[]> MessageQueue = null;
        public bool ReadyToReceiveData = false;

        public Connection(Socket socket, ConcurrentQueue<byte[]> messageQueue)
        {
            Socket = socket;
            MessageQueue = messageQueue;
        }
    }
}