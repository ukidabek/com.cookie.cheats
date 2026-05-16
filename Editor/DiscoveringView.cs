using UnityEditor;
using UnityEngine;

namespace cookie.Cheats
{
    internal class DiscoveringView : View
    {
        private GUIStyle m_style = null;

        private readonly string[] m_labels = new[]
        {
            "Searching for servers",
            "Searching for servers.",
            "Searching for servers..",
            "Searching for servers..."
        };

        private int m_index = 0;
        private double m_lastUpdateTime = .25f;
        private  double m_interval = .5f;

        public DiscoveringView(CheatEditor cheatEditor) : base(cheatEditor)
        {
            m_style = EditorStyles.helpBox;
            m_style.alignment = TextAnchor.MiddleCenter;
        }

        public override void OnGUI() => GUILayout.Label(m_labels[m_index], m_style, GUILayout.Height(60));

        public override void Activate()
        {
            EditorApplication.update += OnEditorUpdate;
            m_lastUpdateTime = EditorApplication.timeSinceStartup;
        }

        private void OnEditorUpdate()
        {
            var delta = EditorApplication.timeSinceStartup - m_lastUpdateTime;
            if (delta < m_interval) return;
            m_lastUpdateTime = EditorApplication.timeSinceStartup;
            m_index++;
            m_index %= m_labels.Length;
            m_cheatEditor.Repaint();
        }

        public override void Deactivate()
        {
            EditorApplication.update -= OnEditorUpdate;
        }
    }
}