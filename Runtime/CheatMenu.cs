using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace cookie.Cheats.UI
{
    public abstract class CheatHandler : MonoBehaviour
    {
        protected virtual void Awake() { }

        public abstract bool CanHandle(ICheat cheat);
        
        public abstract void Initialize(ICheat cheat);
    }

    public abstract class CheatHandler<T> : CheatHandler where T : ICheat
    {
        protected T m_cheat;
        
        public override bool CanHandle(ICheat cheat) => cheat is T;

        public override void Initialize(ICheat cheat) => m_cheat = (T)cheat;
    }

    public class CheatMenu : MonoBehaviour
    {
        [SerializeField] private CheatHandler[] m_cheatHandlers;
        [SerializeField] private Transform m_parent;
        private readonly Dictionary<CheatProvider, List<CheatHandler>> m_cheatsDictionary = new Dictionary<CheatProvider, List<CheatHandler>>(30);

        private void Awake() => CheatDatabase.Instance.OnCheatsRegistered += AddCheatUI;

        private void AddCheatUI(CheatProvider provider, List<ICheat> cheats)
        {
            var cheatHandlers = new List<CheatHandler>(10);
            foreach (var cheat in cheats)
            {
                var handler = m_cheatHandlers.FirstOrDefault(handler => handler.CanHandle(cheat));
                if (handler == null) continue;
                var instance = Instantiate(handler, m_parent);
                instance.Initialize(cheat);
                instance.transform.SetAsFirstSibling();
                cheatHandlers.Add(instance);
            }
        }
    }
}