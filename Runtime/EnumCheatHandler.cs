using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace cookie.Cheats.UI
{
    public class EnumCheatHandler : ValueCheatHandler<object>
    {
        [SerializeField] private TMP_Dropdown m_dropdown = null;
        [SerializeField] private UnityEvent<object> m_onValueChanged = new UnityEvent<object>();

        private Type m_enumType;
        private Array m_enumValues;

        protected override UnityEvent<object> OnValueChanged => m_onValueChanged;

        public override bool CanHandle(ICheat cheat) => base.CanHandle(cheat) && GetValueType(cheat).IsEnum;

        public override void Initialize(ICheat cheat)
        {
            m_enumType = GetValueType(cheat);
            m_enumValues = Enum.GetValues(m_enumType);
            
            m_dropdown.ClearOptions();
            m_dropdown.AddOptions(Enum.GetNames(m_enumType).ToList());
            m_dropdown.onValueChanged.AddListener(OnDropdownChanged);
            
            base.Initialize(cheat);
        }

        private void OnDropdownChanged(int index)
        {
            var selectedValue = m_enumValues.GetValue(index);
            SetMethodInfo.Invoke(m_cheat, new[] { selectedValue });
            OnValueChanged.Invoke(selectedValue);
        }

        protected override void UpdateValue(object value)
        {
            var index = Array.IndexOf(m_enumValues, value);
            if (index < 0) index = 0;
            m_dropdown.SetValueWithoutNotify(index);
        }

        public override void UpdateDisplay()
        {
            if (m_cheat == null) return;
            UpdateValue(GetMethodInfo.Invoke(m_cheat, null));
        }
    }
}