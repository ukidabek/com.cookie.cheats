namespace cookie.Cheats
{
    internal abstract class View
    {
        protected readonly CheatEditor m_cheatEditor = null;

        public View(CheatEditor cheatEditor)
        {
            m_cheatEditor = cheatEditor;
        }

        public abstract void OnGUI();
    }
}