using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace cookie.Cheats.UI
{
    public class CheatMenu : MonoBehaviour
    {
        [FormerlySerializedAs("m_cheatHandlers")] 
        [SerializeField] private CheatHandler[] m_cheatHandlersPrefabs = null;
        [SerializeField] private Separator m_separatorPrefab = null;
        [SerializeField] private Transform m_parent = null;

        private Queue<CheatHandler> m_cheatHandlers = new Queue<CheatHandler>(20);

        private Dictionary<CheatProvider, IReadOnlyList<GameObject>> m_cheatProviders = new Dictionary<CheatProvider, IReadOnlyList<GameObject>>();
      
        private void Awake()
        {
            CheatDatabase.Instance.OnCheatsRegistered += AddCheatUI;
            CheatDatabase.Instance.OnCheatsUnregistered += RemoveCheatUI;
        }

        private void Start()
        {
            var dictionary = CheatDatabase.Instance.ProviderChetDictionary;
            foreach (var pair in dictionary)
            {
                if(m_cheatProviders.ContainsKey(pair.Key)) continue;
                AddCheatUI(pair.Key, pair.Value);
            }
        }

        private void AddCheatUI(CheatProvider provider, List<ICheat> cheats)
        {
            var list = new List<GameObject>(10);
            if (m_separatorPrefab != null)
            {
                var instance = Instantiate(m_separatorPrefab, m_parent);
                instance.Text = provider.gameObject.name;
                list.Add(instance.gameObject);
            }
            
            foreach (var cheat in cheats)
            {
                var handler = m_cheatHandlersPrefabs.FirstOrDefault(handler => handler.CanHandle(cheat));
                if (handler == null) continue;
                var instance = Instantiate(handler, m_parent);
                instance.Initialize(cheat);
                m_cheatHandlers.Enqueue(instance);
                list.Add(instance.gameObject);
            }
            m_cheatProviders.Add(provider, list);
        }

        private void RemoveCheatUI(CheatProvider provider, List<int> cheatsIDs)
        {
            if (!m_cheatProviders.TryGetValue(provider, out var list)) return;
            foreach (var gameObject in list) 
                Destroy(gameObject);
            m_cheatProviders.Remove(provider);
        }
    
        private void Update()
        {
            if (!m_cheatHandlers.Any()) return;

            var handler = m_cheatHandlers.Dequeue();
            handler.UpdateDisplay();
            m_cheatHandlers.Enqueue(handler);
        }
    }
}