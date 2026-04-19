using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace cookie.Cheats.UI
{
    public class AxisHandler
    {
        private TMP_InputField m_field = null;
        private Slider m_slider = null;
        
        private readonly Type m_floatType = typeof(float);
        private readonly Type m_fieldType = null;
        private readonly bool m_isWholeNumber = false;
        
        public event Action OnValueChanged;
        
        public object Value
        {
            get => Convert.ChangeType(m_slider.value, m_fieldType);
            set
            {
                var convertedValue = Convert.ChangeType(value, m_floatType);
                m_slider.value = (float)convertedValue;
                OnSliderChange(m_slider.value);
            }
        }

        public AxisHandler(TMP_InputField field, 
            Slider slider, 
            Type fieldType, 
            object value, 
            float minValue,
            float maxValue)
        {
            m_field = field;
            m_slider = slider;
            m_fieldType = fieldType;
            m_slider.value = (float)Convert.ChangeType(value, m_floatType);
            m_slider.minValue = minValue;
            m_slider.maxValue = maxValue;
            m_slider.wholeNumbers = m_isWholeNumber;
            
            m_field.onValueChanged.AddListener(OnFieldChange);
            m_slider.onValueChanged.AddListener(OnSliderChange);
        }
        
        private void OnFieldChange(string arg0)
        {
            var value = float.Parse(arg0);
            m_slider.value = value;
            OnValueChanged?.Invoke();
        }
        
        private void OnSliderChange(float arg0)
        {
            m_field.text = m_isWholeNumber ? arg0.ToString("0") : arg0.ToString("F1");
            OnValueChanged?.Invoke();
        }

        public void SetActive(bool status)
        {
            m_field.gameObject.SetActive(status);
            m_slider.gameObject.SetActive(status);
        }
    }
}