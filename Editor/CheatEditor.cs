using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
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
        private List<IEditorCheat> m_editorCheats = new  List<IEditorCheat>(30);
       
        private State m_currentState = State.ConnectionList;
        private TcpClient m_tcpClient = null;

        public int DiscoverPort { get; set; } = 2137;
     
        [MenuItem("Tools/My Custom Window")]
        public static void ShowWindow()
        {
            // Create and show the window
            var window = GetWindow<CheatEditor>("My Window");
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
                    new KeyValuePair<State, View>(State.Cheats, new CheatsView(this, m_editorCheats)),
                });
        }

        private void OnGUI()
        {
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
            m_currentState = State.Connecting;
            await Awaitable.BackgroundThreadAsync();
            
            m_tcpClient?.Close();
            m_tcpClient ??= new TcpClient();

            await m_tcpClient.ConnectAsync(endPoint.Address, endPoint.Port);

            var stream = m_tcpClient.GetStream();
              
            var message = new Message(CheatServer.GetCheatsMessage, null);
            var binaryFormatter = new BinaryFormatter();

            using (var memoryStream = new MemoryStream())
            {
                binaryFormatter.Serialize(memoryStream, message);
                await stream.WriteAsync(memoryStream.ToArray());
            }

            var buffer = new byte[4096];
            var lenght = await stream.ReadAsync(buffer);

            using (var memoryStream = new MemoryStream(buffer, 0, lenght))
            {
                message = (Message)binaryFormatter.Deserialize(memoryStream);
            }

            var data = (CheatData[])message.Payload;
            
            foreach (var identifier in data)
            {
                var type = Type.GetType(identifier.AssemblyQualifiedName);
                if (!m_fieldCheats.TryGetValue(type, out var builder)) continue;
                var instance = builder.Build(identifier);
                instance.Update += SendPayload;
                m_editorCheats.Add(instance);
            }
            
            await Awaitable.MainThreadAsync();
            m_currentState = State.Cheats;
            Repaint();
        }

        private async void SendPayload(CheatPayload payload)
        {
            if (m_tcpClient == null) return;

            var stream = m_tcpClient.GetStream();

            var binaryFormater = new BinaryFormatter();
            var message = new Message(CheatServer.SetPayload, payload);

            using var memoryStream = new MemoryStream();
            binaryFormater.Serialize(memoryStream, message);

            await stream.WriteAsync(memoryStream.ToArray());
        }

        private async Task<List<IPEndPoint>> SendDiscoverServerBroadcast()
        {
            await Awaitable.BackgroundThreadAsync();
            var list = new List<IPEndPoint>(10);
            using var udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;
            var data = BitConverter.GetBytes(CheatServer.DiscoverMessage);
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
                        var ip = IPAddress.Parse(ipAndPort[0]);
                        var ipEndPoint = new IPEndPoint(ip, int.Parse(ipAndPort[1]));
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