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

        public override bool CanHandle(ICheat cheat) => cheat is ValueCheat valueCheat && 
                                                        valueCheat.ValueType.IsEnum;

        public override void Initialize(ICheat cheat)
        {
            base.Initialize(cheat);
            var valueType = m_cheat.ValueType;
            m_enumValues = Enum.GetValues(valueType);
            m_dropdown.ClearOptions();
            m_dropdown.AddOptions(Enum.GetNames(valueType).ToList());
            m_dropdown.onValueChanged.AddListener(OnDropdownChanged);
            UpdateDisplay();
        }

        private void OnDropdownChanged(int index) => m_cheat.Set(m_enumValues.GetValue(index));

        protected override void UpdateValue(object value)
        {
            var index = Array.IndexOf(m_enumValues, value);
            if (index < 0) index = 0;
            m_dropdown.SetValueWithoutNotify(index);
        }

        public override void UpdateDisplay()
        {
            if (m_cheat == null) return;
            UpdateValue(m_cheat.Get());
        }
    }
}