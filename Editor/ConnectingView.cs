using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace cookie.Cheats
{
    internal class ConnectingView : View
    {
        public ConnectingView(CheatEditor cheatEditor) : base(cheatEditor)
        {
        }

        public override void OnGUI()
        {
            GUILayout.Label("Connecting...", EditorStyles.helpBox);
        }
    }

    internal class CheatsView : View
    {
        private readonly IEnumerable<IEditorCheat> m_cheats;
        private Vector2 m_scrollPosition =  Vector2.zero;

        public CheatsView(CheatEditor cheatEditor, IEnumerable<IEditorCheat> cheats) : base(cheatEditor)
        {
            m_cheats = cheats;
        }

        public override void OnGUI()
        {
            m_scrollPosition = GUILayout.BeginScrollView(m_scrollPosition);
            foreach (var cheat in m_cheats)
                cheat.OnGUI();
            GUILayout.EndScrollView();
        }
    }
}