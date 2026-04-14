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
    public interface IEditorCheatFactory
    {
        IEditorCheat Build(CheatData data);
    }

    public interface IEditorCheat : ICheat
    {
        event Action<CheatPayload> Update;
        void OnGUI();
    }

    public interface IEditorCheatBuilder
    {
        Type Type { get; }
        IEditorCheat Build(CheatData data);
    }

    public class MethodCheatBuilder : IEditorCheatBuilder
    {
        private class EditorMethodCheat : IEditorCheat
        {
            public int ID { get; }
            public string Name { get; }
            public CheatAttributeData[] Attributes { get; }

            public CheatData ToDataTransferObject() => null;

            public event Action<CheatPayload> Update;

            public void OnGUI()
            {
                EditorGUILayout.BeginHorizontal();
                foreach (var attribute in Attributes)
                {
                    var name = string.IsNullOrEmpty(attribute.Name) ? Name : attribute.Name;
                    if (GUILayout.Button(name)) 
                        Update?.Invoke(new CheatPayload(ID, attribute.Parameters));
                }

                EditorGUILayout.EndHorizontal();
            }

            public EditorMethodCheat(CheatData data)
            {
                ID = data.ID;
                Name = data.Name;
                Attributes = data.Attributes;
            }
        }
        
        public Type Type => typeof(MethodCheat);
        
        public IEditorCheat Build(CheatData data) => new EditorMethodCheat(data);
    }
    
    public class PropertyCheatBuilder : FieldCheatBuilder
    {
        public override Type Type => typeof(PropertyCheat);
    }
    
    public class FieldCheatBuilder : IEditorCheatBuilder
    {
        private class EditorFieldCheat : IEditorCheat
        {
            public int ID { get; }
            public string Name { get; }
            public CheatAttributeData[] Attributes { get; }

            public CheatData ToDataTransferObject() => null;

            public event Action<CheatPayload> Update;

            private bool m_isNumericValue = false;
            private float m_numericValue = 0;
            private bool m_booleValue = false;

            public void OnGUI()
            {
                EditorGUI.BeginChangeCheck();
                if (m_isNumericValue)
                    m_numericValue = EditorGUILayout.FloatField(Name, m_numericValue);
                else
                    m_booleValue = EditorGUILayout.Toggle(Name, m_booleValue);

                if (EditorGUI.EndChangeCheck())
                    Update.Invoke(new CheatPayload(ID, new object[]
                    {
                        m_isNumericValue ? m_numericValue : m_booleValue
                    }));
            }

            public EditorFieldCheat(ValueCheatData data)
            {
                ID = data.ID;
                Name = data.Name;
                Attributes = data.Attributes;
                m_isNumericValue = data.IsNumeric;
                var value = data.Value;
                if (m_isNumericValue)
                    m_numericValue = (float)Convert.ChangeType(value, typeof(float));
                else
                    m_booleValue = (bool)Convert.ChangeType(value, typeof(bool));
            }
        }

        public virtual Type Type => typeof(FieldCheat);

        public IEditorCheat Build(CheatData data)
        {
            return new EditorFieldCheat(data as ValueCheatData);
        }
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
                var data = (CheatData[])binaryFormater.Deserialize(memoryStream);

                foreach (var identifier in data)
                {
                    var type = Type.GetType(identifier.AssemblyQualifiedName);
                    if(!m_fieldCheats.TryGetValue(type, out var builder)) continue;
                    var instance = builder.Build(identifier);
                    instance.Update += SendPayload;
                    m_editorCheats.Add(instance);
                }
            }
            m_currentState = State.Cheats;
            Repaint();
        }

        private async void SendPayload(CheatPayload payload)
        {
            if (m_tcpClient == null) return;
            var stream = m_tcpClient.GetStream();

            await stream.WriteAsync(BitConverter.GetBytes(CheatServer.SetPayload));
            var binaryFormater = new BinaryFormatter();
            using (var memoryStream = new MemoryStream())
            {
                binaryFormater.Serialize(memoryStream, payload);
                await stream.WriteAsync(memoryStream.ToArray(), 0, (int)memoryStream.Length);
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