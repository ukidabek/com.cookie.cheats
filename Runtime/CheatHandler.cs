using UnityEngine;

namespace cookie.Cheats.UI
{
    public abstract class CheatHandler : MonoBehaviour
    {
        protected virtual void Awake() { }

        public abstract bool CanHandle(ICheat cheat);
        
        public abstract void Initialize(ICheat cheat);
        public abstract void UpdateDisplay();
    }
    
    public abstract class CheatHandler<T> : CheatHandler where T : ICheat
    {
        protected T m_cheat;
        
        public override bool CanHandle(ICheat cheat) => cheat is T;

        public override void Initialize(ICheat cheat) => m_cheat = (T)cheat;
    }
}