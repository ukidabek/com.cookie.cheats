using System;
using System.Net.Sockets;
using System.Threading;

namespace cookie.Cheats.Network
{
    public abstract class NetworkUser : IDisposable
    {
        public void SendData(Socket socket, byte[] data) => socket.Send(data);

        public int ReceiveData(Socket socket, byte[] buffer)
        {
            var lenght = socket.Receive(buffer);
            return lenght;
        }

        public virtual void Dispose() { }
    }
}