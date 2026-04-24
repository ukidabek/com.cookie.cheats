using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace cookie.Cheats.UI
{
    public class NumericCheatHandler : ValueCheatHandler<float>
    {
        [SerializeField] private Slider m_slider = null;
        [SerializeField] private TMP_InputField m_inputField = null;
        
        protected override UnityEvent<float> OnValueChanged => m_slider.onValueChanged;

        public override bool CanHandle(ICheat cheat) => cheat is ValueCheat { IsNumeric: true };

        public override void Initialize(ICheat cheat)
        {
            base.Initialize(cheat);
            m_slider.wholeNumbers = m_cheat.IsWholeNumber;
            var attribute = cheat.Attributes.First();
            m_slider.minValue = attribute.Min;
            m_slider.maxValue = attribute.Max;
            UpdateDisplay();
        }

        public override void UpdateDisplay()
        {
            var value = m_cheat.Get();
            value = Convert.ChangeType(value, typeof(float));
            if (value == null) return;
            m_inputField.text = m_cheat.IsWholeNumber ? value.ToString() : $"{value:F1}";
            m_slider.value = (float)value;
        }

        protected override void UpdateValue(float value)
        {
            m_cheat.Set(value);
            UpdateDisplay();
        }
    }
}