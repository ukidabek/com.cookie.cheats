using System.Reflection;
using UnityEngine;

namespace cookie.Cheats.UI
{
    public class MethodCheatHandler : CheatHandler<MethodCheat>
    {
        [SerializeField] private CheatButton m_cheatButtonPrefab = null;
        [SerializeField] private Transform m_parent = null;

        protected override void Awake()
        {
            base.Awake();
            m_cheatButtonPrefab.gameObject.SetActive(false);
        }

        public override void Initialize(ICheat cheat)
        {
            base.Initialize(cheat);
            var methodInfo = (MethodInfo)m_cheat.MemberInfo;
            foreach (var attribute in m_cheat.Attributes)
            {
                var button = Instantiate(m_cheatButtonPrefab, m_parent);
                var name = string.IsNullOrEmpty(attribute.Name) ? m_cheat.Name : attribute.Name;
                button.gameObject.SetActive(true);
                button.Initialize(name, m_cheat.Target, methodInfo, attribute.Parameters);
            }
        }

        public override void UpdateDisplay() { }
    }
}