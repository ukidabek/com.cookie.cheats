using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using cookie.Cheats.Server;
using UnityEditor;
using UnityEngine;

namespace cookie.Cheats
{
    public class CheatEditor : EditorWindow
    {
        private enum State
        {
            Discovering,
            ConnectionList,
            Connecting,
            Cheats,
        }
        
        private readonly List<IPEndPoint> m_discoveredServer = new List<IPEndPoint>(10);
        private Dictionary<State, View> m_states = null;
        private Dictionary<Type, IEditorCheatBuilder> m_fieldCheats = null;
        private Dictionary<int, IEditorCheat> m_editorCheats = new  Dictionary<int, IEditorCheat>(30);
        private State m_currentState = State.ConnectionList;
        private TcpClient m_tcpClient = null;
        private Network.Connection m_connection = null;
        public int DiscoverPort { get; set; } = 2137;
     
        [MenuItem("Tools/Cheat remote")]
        public static void ShowWindow()
        {
            var window = GetWindow<CheatEditor>("Cheat remote");
            window.Show();
        }

        private void OnEnable()
        {
            m_fieldCheats = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && !type.IsInterface && typeof(IEditorCheatBuilder).IsAssignableFrom(type))
                .Select(type => (IEditorCheatBuilder)Activator.CreateInstance(type))
                .ToDictionary(builder => builder.Type, builder => builder);

            m_states = new Dictionary<State, View>(
                new KeyValuePair<State, View>[]
                {
                    new KeyValuePair<State, View>(State.Discovering, new DiscoveringView(this)),
                    new KeyValuePair<State, View>(State.ConnectionList, new ConnectionListView(this,
                        m_discoveredServer,
                        DiscoverServers,
                        ConnectToServer)),
                    new KeyValuePair<State, View>(State.Connecting, new ConnectingView(this)),
                    new KeyValuePair<State, View>(State.Cheats, new CheatsView(this, m_editorCheats.Values)),
                });
        }

        private void OnGUI()
        {
            if (m_connection != null)
            {
                if (m_connection.ReceiveQueue.TryDequeue(out var message))
                {
                    switch (message.ID)
                    {
                        case CheatServer.SynchronizeCheats:
                            var payload = message.GetPayload();
                
                            if(payload is not CheatData cheatData) break;
                            
                            var type = Type.GetType(cheatData.AssemblyQualifiedName);
                
                            if (!m_fieldCheats.TryGetValue(type, out var builder)) break;
                
                            var instance = builder.Build(cheatData);
                            instance.Update += SendPayload;
                            m_editorCheats.Add(instance.ID, instance);
                            break;
                    }
                }
            }
            
            if (!m_states.Any()) return;
            m_states[m_currentState].OnGUI();
        }

        private void OnDestroy()
        {
            if (m_tcpClient == null) return;
            
            m_tcpClient.Client.Shutdown(SocketShutdown.Both);
            m_tcpClient.Close();
            m_tcpClient.Dispose();
        }

        private async void DiscoverServers()
        {
            m_currentState = State.Discovering;
            Repaint();
            
            var discoveredServer = await SendDiscoverServerBroadcast();
            m_discoveredServer.Clear();
            m_discoveredServer.AddRange(discoveredServer);
            
            m_currentState = State.ConnectionList;
            Repaint();
        }
        
        private async void ConnectToServer(IPEndPoint endPoint)
        {
            var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            await socket.ConnectAsync(endPoint);
            m_connection = new Network.Connection(socket);
            
            m_currentState = State.Cheats;
            Repaint();
        }

        private void SendPayload(CheatPayload payload)
        {
            if (m_tcpClient == null) return;
            
            var message = new Message(CheatServer.SetPayload, payload);
            CheatServer.SendMessage(m_tcpClient.Client, message);
        }

        private async Task<List<IPEndPoint>> SendDiscoverServerBroadcast()
        {
            await Awaitable.BackgroundThreadAsync();
            var list = new List<IPEndPoint>(10);
            using var udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;
            var data = Encoding.UTF8.GetBytes(cookie.Cheats.Network.Server.DiscoverMessage);
            var start = DateTime.UtcNow;
            
            await udpClient.SendAsync(data, data.Length, new IPEndPoint(IPAddress.Broadcast, DiscoverPort));

            while ((DateTime.UtcNow - start).TotalMilliseconds < 3000)
            {
                try
                {
                    if (udpClient.Available > 0)
                    {
                        var result = await udpClient.ReceiveAsync();
                        var message = Encoding.UTF8.GetString(result.Buffer, 0, result.Buffer.Length);
                        var ipAndPort = message.Split(':');
                        var ip = IPAddress.Parse(ipAndPort[1]);
                        var ipEndPoint = new IPEndPoint(ip, int.Parse(ipAndPort[2]));
                        list.Add(ipEndPoint);
                    }
                    else
                        await Task.Yield();
                }
                catch (ObjectDisposedException)
                {
                    await Awaitable.MainThreadAsync();
                    return list;
                }
            }

            await Awaitable.MainThreadAsync();
            
            return list;
        }
    }
}