using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace cookie.Cheats
{
    internal class ConnectingView : View
    {
        public override void OnGUI()
        {
            GUILayout.Label("Connecting...", EditorStyles.helpBox);
        }
    }

    internal class CheatsView : View
    {
        private readonly List<IEditorCheat> m_cheats;

        public CheatsView(List<IEditorCheat> cheats)
        {
            m_cheats = cheats;
        }

        public override void OnGUI()
        {
            foreach (var cheat in m_cheats)
                cheat.OnGUI();
        }
    }
}