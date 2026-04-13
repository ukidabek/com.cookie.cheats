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
using Object = System.Object;

namespace cookie.Cheats
{
    public interface IEditorCheatFactory
    {
        IEditorCheat Build(CheatIdentifier identifier);
    }

    public interface IEditorCheat : ICheat
    {
        event Action<CheatPayload> Update;
        void OnGUI();
    }

    public interface IEditorCheatBuilder
    {
        Type Type { get; }
        IEditorCheat Build(CheatIdentifier identifier);
    }

    public class FieldCheatBuilder : IEditorCheatBuilder
    {
        private class EditorFieldCheat : IEditorCheat
        {
            public int ID { get; }
            public string Name { get; }
            public CheatData[] Attributes { get; }
            public event Action<CheatPayload> Update;

            private float m_value = 0;

            public void OnGUI()
            {
                EditorGUI.BeginChangeCheck();
                m_value = EditorGUILayout.FloatField(Name, m_value);
                if (EditorGUI.EndChangeCheck())
                    Update.Invoke(new CheatPayload(ID, new object[] { m_value }));
            }

            public EditorFieldCheat(CheatIdentifier identifier)
            {
                ID = identifier.ID;
                Name = identifier.Name;
                Attributes = identifier.Attributes;
            }
        }

        public Type Type => typeof(FieldCheat);

        public IEditorCheat Build(CheatIdentifier identifier) => new EditorFieldCheat(identifier);
    }
    
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
            m_fieldCheats = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => !type.IsAbstract && !type.IsInterface && typeof(IEditorCheatBuilder).IsAssignableFrom(type))
                .Select(type => (IEditorCheatBuilder)Activator.CreateInstance(type))
                .ToDictionary(builder => builder.Type, builder => builder);
            
            m_states = new Dictionary<State, View>(
                new KeyValuePair<State,View>[]
                {
                    new KeyValuePair<State, View>(State.Discovering, new DiscoveringView()),
                    new KeyValuePair<State, View>(State.ConnectionList, new ConnectionListView(
                        m_discoveredServer, 
                        DiscoverServers,
                        ConnectToServer)),
                    new KeyValuePair<State, View>(State.Connecting, new ConnectingView()),
                    new KeyValuePair<State, View>(State.Cheats, new CheatsView(m_editorCheats)),
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
            
            m_currentState = State.ConnectionList;
            m_discoveredServer.Clear();
            m_discoveredServer.AddRange(discoveredServer);
            Repaint();
        }
        
        private async void ConnectToServer(IPEndPoint endPoint)
        {
            m_currentState = State.Connecting;

            m_tcpClient?.Close();
            m_tcpClient ??= new TcpClient();
            
            await m_tcpClient.ConnectAsync(endPoint.Address, endPoint.Port);

            var stream = m_tcpClient.GetStream();
            
            stream.Write(BitConverter.GetBytes(CheatServer.GetCheatsMessage));
            var sizeBuffer = new byte[sizeof(int)];
            var lenght = await stream.ReadAsync(sizeBuffer, 0, sizeBuffer.Length);
            lenght = BitConverter.ToInt32(sizeBuffer, 0);
            var buffer = new byte[lenght];
            lenght = await stream.ReadAsync(buffer, 0, buffer.Length);
           
            using (var memoryStream = new MemoryStream(buffer, 0, lenght))
            {
                var binaryFormater = new BinaryFormatter();
                var data = (CheatIdentifier[])binaryFormater.Deserialize(memoryStream);

                foreach (var identifier in data)
                {
                    var type = Type.GetType(identifier.AssemblyQualifiedName);
                    if(!m_fieldCheats.TryGetValue(type, out var builder)) continue;
                    var instance = builder.Build(identifier);
                    m_editorCheats.Add(instance);
                }
            }
            m_currentState = State.Cheats;
            Repaint();
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