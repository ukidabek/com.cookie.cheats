using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace cookie.Cheats.UI
{
    [Serializable]
    public class AxisHandler
    {
        [SerializeField] private TMP_InputField m_field = null;
        [SerializeField] private Slider m_slider = null;
        
        public FieldInfo FieldInfo { get; private set; }
        
        private readonly Type m_floatType = typeof(float);
        private readonly Type m_fieldType = default;
        private readonly bool m_isWholeNumber = false;
        
        public object Value
        {
            get => Convert.ChangeType(m_slider.value, m_fieldType);
            set => m_slider.value = (float)Convert.ChangeType(value, m_fieldType);
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
        }
        
        private void OnSliderChange(float arg0)
        {
            m_field.text = m_isWholeNumber ? arg0.ToString() : arg0.ToString("F1");
        }

        public void SetActive(bool status)
        {
            m_field.gameObject.SetActive(status);
            m_slider.gameObject.SetActive(status);
        }
    }
}