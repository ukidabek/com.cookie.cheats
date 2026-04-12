using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace cookie.Cheats
{
    public class NumericCheatHandler : ValueCheatHandler<float>
    {
        [SerializeField] private Slider m_slider = null;
        [SerializeField] private TMP_InputField m_inputField = null;
        
        protected override UnityEvent<float> OnValueChanged => m_slider.onValueChanged;

        public override void Initialize(ICheat cheat)
        {
            base.Initialize(cheat);
            m_slider.wholeNumbers = (bool)IsWholeNumberFieldInfo.GetValue(cheat);
            var attribute = cheat.Attributes.First();
            m_slider.minValue = attribute.Min;
            m_slider.maxValue = attribute.Max;
        }

        protected override void UpdateDisplay()
        {
            if (GetMethodInfo == null) return;
            var value = GetMethodInfo.Invoke(m_cheat, null);
            value = Convert.ChangeType(value, typeof(float));
            var isWholeNumber = (bool)IsWholeNumberFieldInfo.GetValue(m_cheat);
            m_inputField.text = isWholeNumber ? value.ToString() : $"{value:F1}";
            m_slider.value = (float)value;
        }

        protected override void UpdateValue(float value)
        {
            SetMethodInfo.Invoke(m_cheat, new object[] { value });
            UpdateDisplay();
        }
    }
}