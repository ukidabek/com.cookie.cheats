using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace cookie.Cheats.UI
{
    internal class VectorCheatHandler : ValueCheatHandler<object>
    {
        [Header("X Axis")]
        [SerializeField] private TMP_InputField m_xField = null;
        [SerializeField] private Slider m_xSlider = null;

        [Header("Y Axis")]
        [SerializeField] private TMP_InputField m_yField = null;
        [SerializeField] private Slider m_ySlider = null;

        [Header("Z Axis (optional)")]
        [SerializeField] private TMP_InputField m_zField = null;
        [SerializeField] private Slider m_zSlider = null;

        [Header("W Axis (optional)")]
        [SerializeField] private TMP_InputField m_wField = null;
        [SerializeField] private Slider m_wSlider = null;

        private readonly AxisHandler[] m_axisHandlers = new AxisHandler[4];

        private (TMP_InputField, Slider)[] m_ui = null;
        
        private float m_minValue;
        private float m_maxValue;

        private Type m_vectorType;
        private int m_axisCount;
        private bool m_isWholeNumber;

        [SerializeField] private UnityEvent<object> m_onValueChanged = new();
        private MethodInfo m_getterMethodInfo;
        private MethodInfo m_setterMethodInfo;
        protected override UnityEvent<object> OnValueChanged => m_onValueChanged;

        protected override void Awake()
        {
            base.Awake();
            m_ui = new[]
            {
                (m_xField, m_xSlider),
                (m_yField, m_ySlider),
                (m_zField, m_zSlider),
                (m_wField, m_wSlider),
            };
        }

        public override bool CanHandle(ICheat cheat)
        {
            var valueType = GetValueType(cheat);
            return TypeGroups.VectorTypes.Contains(valueType);
        }

        public override void Initialize(ICheat cheat)
        {
            base.Initialize(cheat);
            m_vectorType = GetValueType(cheat);
            m_axisCount = TypeGroups.AxisCountDictionary[m_vectorType];
            m_isWholeNumber = TypeGroups.WholeNumberVectorTypes.Contains(m_vectorType);
            
            var cheatAttribute = cheat.Attributes[0];
            m_minValue = cheatAttribute.Min;
            m_maxValue = cheatAttribute.Max;

            var valute = GetMethodInfo.Invoke(cheat, null);
            m_getterMethodInfo = m_vectorType.GetMethod("get_Item");
            m_setterMethodInfo = m_vectorType.GetMethod("set_Item");
            
            var fieldType = m_isWholeNumber ? typeof(int) : typeof(float);
            var parameters = new object[] { 0 };
            for (var i = 0; i < 4; i++)
            {
                parameters[0] = i;
                var pair = m_ui[i];
                var validAxis = i < m_axisCount;
                var value = validAxis ? m_getterMethodInfo.Invoke(valute, parameters) : 0f;
                m_axisHandlers[i] = new AxisHandler(pair.Item1, pair.Item2, fieldType, value, m_minValue, m_maxValue);
                m_axisHandlers[i].SetActive(validAxis);
                m_axisHandlers[i].OnValueChanged += XXX;
            }
        }

        private void XXX()
        {
            var instance = Activator.CreateInstance(m_vectorType);
            var valuesCount = TypeGroups.AxisCountDictionary[m_vectorType];
            var parameters = new object[2];
            
            for (var i = 0; i < valuesCount; i++)
            {
                parameters[0] = i;
                parameters[1] = m_axisHandlers[i].Value;
                m_setterMethodInfo.Invoke(instance, parameters);
            }
            
            OnValueChanged.Invoke(instance);
        }

        protected override void UpdateValue(object value) => SetMethodInfo.Invoke(m_cheat, new[] { value });

        public override void UpdateDisplay()
        {
            if (m_cheat == null) return;
            var currentValue = GetMethodInfo.Invoke(m_cheat, null);
            var parameters = new object[] { 0 };
            for (var i = 0; i < m_axisCount; i++)
            {
                parameters[0] = i;
                var axisValue = m_getterMethodInfo.Invoke(currentValue, parameters);
                m_axisHandlers[i].Value = axisValue;
            }
        }
    }
}
