using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace cookie.Cheats.UI
{
    public class CheatMenu : MonoBehaviour
    {
        [FormerlySerializedAs("m_cheatHandlers")] 
        [SerializeField] private CheatHandler[] m_cheatHandlersPrefabs;
        [SerializeField] private Transform m_parent;
        
        private List<CheatHandler>  m_cheatHandlers = new List<CheatHandler>();
        private int m_index = 0;
        
        private void Awake() => CheatDatabase.Instance.OnCheatsRegistered += AddCheatUI;

        private void AddCheatUI(CheatProvider provider, List<ICheat> cheats)
        {
            foreach (var cheat in cheats)
            {
                var handler = m_cheatHandlersPrefabs.FirstOrDefault(handler => handler.CanHandle(cheat));
                if (handler == null) continue;
                var instance = Instantiate(handler, m_parent);
                instance.Initialize(cheat);
                instance.transform.SetAsFirstSibling();
                m_cheatHandlers.Add(instance);
            }
        }

        private void Update()
        {
            if (!m_cheatHandlers.Any()) return;
            if (m_index == m_cheatHandlers.Count)
            {
                m_index = 0;
                return;
            }

            m_cheatHandlers[m_index++].UpdateDisplay();
        }
    }
}