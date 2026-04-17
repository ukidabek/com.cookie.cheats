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

        public CheatsView(CheatEditor cheatEditor, IEnumerable<IEditorCheat> cheats) : base(cheatEditor)
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