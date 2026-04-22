using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace cookie.Cheats.UI
{
    public class ToggleCheatHandler : ValueCheatHandler<bool>
    {
        [SerializeField] private Toggle m_toggle = null;

        protected override UnityEvent<bool> OnValueChanged => m_toggle.onValueChanged;

        public override void UpdateDisplay()
        {
            if (m_cheat == null) return;
            m_toggle.isOn = (bool)m_cheat.Get();
        }
    }
}