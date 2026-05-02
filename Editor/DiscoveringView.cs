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

        public DiscoveringView(CheatEditor cheatEditor) : base(cheatEditor)
        {
            m_style = EditorStyles.helpBox;
            m_style.alignment = TextAnchor.MiddleCenter;
        }

        public override void OnGUI()
        {
            GUILayout.Label(m_labels[m_index++], m_style, GUILayout.Height(60));
            m_index %= m_labels.Length;
        }
    }
}