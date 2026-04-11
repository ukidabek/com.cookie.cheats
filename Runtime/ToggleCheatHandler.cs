using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace cookie.Cheats
{
    public class ToggleCheatHandler : ValueCheatHandler<bool>
    {
        private static readonly Type BoolType = typeof(bool);
        
        [SerializeField] private Toggle m_toggle = null;

        protected override UnityEvent<bool> OnValueChanged => m_toggle.onValueChanged;

        public override bool CanHandle(ICheat cheat)
        {
            if (!base.CanHandle(cheat)) return false;
            var type = cheat.GetType();
            ValueTypeFieldInfo = type.GetProperty("ValueType", bindingFlags);
            type = (Type)ValueTypeFieldInfo.GetValue(cheat);
            return type == BoolType;
        }

        protected override void UpdateDisplay()
        {
            if (GetMethodInfo == null) return;
            m_toggle.isOn = (bool)GetMethodInfo.Invoke(m_cheat, null);
        }

        protected override void UpdateValue(bool value) => SetMethodInfo.Invoke(m_cheat, new object[] { value });
    }
}