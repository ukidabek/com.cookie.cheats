using UnityEditor;
using UnityEngine;

namespace cookie.Cheats
{
    internal class DiscoveringView : View
    {
        private GUIStyle m_style = null;
        
        public DiscoveringView(CheatEditor cheatEditor) : base(cheatEditor)
        {
            m_style = EditorStyles.helpBox;
            m_style.alignment = TextAnchor.MiddleCenter;
        }

        public override void OnGUI() => GUILayout.Label("Searching for servers…", m_style, GUILayout.Height(60));
    }
}