using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEditor;
using UnityEngine;

namespace cookie.Cheats
{
    internal class ConnectionView : View
    {
        private readonly List<IPEndPoint> m_discoveredServer = null;
        private readonly Action OnRefresh = null;
        private readonly Action<IPEndPoint> OnConnect = null;

        public ConnectionView(List<IPEndPoint> discoveredServer, Action onRefresh, Action<IPEndPoint> connect)
        {
            m_discoveredServer = discoveredServer;
            OnRefresh = onRefresh;
            OnConnect = connect;
        }

        public override void OnGUI()
        {
            if (m_discoveredServer.Any())
            {
                var style = EditorStyles.helpBox;
                foreach (var endPoint in m_discoveredServer)
                {
                    GUILayout.BeginHorizontal(style);
                    GUILayout.Label(endPoint.ToString());
                    if(GUILayout.Button("Connect", GUILayout.Width(60)))
                        OnConnect.Invoke(endPoint);
                    GUILayout.EndHorizontal();
                }
            }
            else
                GUILayout.Label("No servers available");

            if(GUILayout.Button("Refresh"))
                OnRefresh.Invoke();
        }
    }
}