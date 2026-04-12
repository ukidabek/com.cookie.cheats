using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace cookie.Cheats
{
    public class CheatEditor : EditorWindow
    {
        private enum State
        {
            Discovering,
            Connecting,
        }
        
        private readonly List<IPEndPoint> m_discoveredServer = new List<IPEndPoint>(10);
        private Dictionary<State, View> m_states = new Dictionary<State, View>();
        private State m_currentState = State.Discovering;

        private TcpClient m_tcpClient = null;
        
        // Add menu item to open the window
        [MenuItem("Tools/My Custom Window")]
        public static void ShowWindow()
        {
            // Create and show the window
            var window = GetWindow<CheatEditor>("My Window");
            window.Show();
        }

        private void OnEnable()
        {
            m_states = new Dictionary<State, View>(
                new KeyValuePair<State,View>[]
                {
                    new KeyValuePair<State, View>(State.Discovering, new DiscoveringView()),
                    new KeyValuePair<State, View>(State.Connecting, new ConnectionView(
                        m_discoveredServer, 
                        DiscoverServers,
                        ConnectToServer)),
                });
           DiscoverServers();
        }

        private void OnGUI()
        {
            if (!m_states.Any()) return;
            m_states[m_currentState].OnGUI();
        }

        private async void DiscoverServers()
        {
            m_currentState = State.Discovering;
            Repaint();
            
            var discoveredServer = await SendDiscoverServerBroadcast();
            foreach (var endPoint in discoveredServer) 
                Debug.Log(endPoint);
            
            m_currentState = State.Connecting;
            m_discoveredServer.Clear();
            m_discoveredServer.AddRange(discoveredServer);
            Repaint();
        }
        
        private async void ConnectToServer(IPEndPoint endPoint)
        {
            var buffer = new byte[1024];
            m_tcpClient?.Close();
            m_tcpClient ??= new TcpClient();
            await m_tcpClient.ConnectAsync(endPoint.Address, endPoint.Port);

            var stream = m_tcpClient.GetStream();
            stream.Write(BitConverter.GetBytes(CheatServer.GetCheatsMessage));

            
            var lenght = await stream.ReadAsync(buffer, 0, buffer.Length);
            using (var memoryStream = new MemoryStream(buffer, 0, lenght))
            {
                var binaryFormater = new BinaryFormatter();
                var data = binaryFormater.Deserialize(memoryStream);
            }
        }

        private async Task<List<IPEndPoint>> SendDiscoverServerBroadcast()
        {
            var list = new List<IPEndPoint>(10);
            using var udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;
            var data = BitConverter.GetBytes(CheatServer.DiscoverMessage);
            var start = DateTime.UtcNow;
            
            await udpClient.SendAsync(data, data.Length, new IPEndPoint(IPAddress.Broadcast, 2137));

            while ((DateTime.UtcNow - start).TotalMilliseconds < 3000)
            {
                try
                {
                    if (udpClient.Available > 0)
                    {
                        var result = await udpClient.ReceiveAsync();
                        var message = Encoding.UTF8.GetString(result.Buffer, 0, result.Buffer.Length);
                        var ipAndPort = message.Split(':');
                        var ip = IPAddress.Parse(ipAndPort[0]);
                        list.Add(new IPEndPoint(ip, int.Parse(ipAndPort[1])));
                    }
                    else
                        await Task.Yield();
                }
                catch (ObjectDisposedException)
                {
                    return list;
                }
            }

            return list;
        }
    }
}