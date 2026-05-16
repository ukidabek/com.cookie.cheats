namespace cookie.Cheats
{
    internal abstract class View
    {
        protected readonly CheatEditor m_cheatEditor = null;

        protected View(CheatEditor cheatEditor) => m_cheatEditor = cheatEditor;

        public abstract void OnGUI();

        public virtual void Activate()
        {
        }

        public virtual void Deactivate()
        {
        }
    }
}