using System;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace cookie.Cheats.UI
{
    internal class MultipleValueCheatHandler : ValueCheatHandler<object>
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

        private (TMP_InputField, Slider)[] m_uiTuple = null;
        
        private float m_minValue;
        private float m_maxValue;

        private int m_axisCount;
        private bool m_isWholeNumber;

        [SerializeField] private UnityEvent<object> m_onValueChanged = new();
        private MethodInfo m_getterMethodInfo;
        private MethodInfo m_setterMethodInfo;
        protected override UnityEvent<object> OnValueChanged => m_onValueChanged;

        protected override void Awake()
        {
            base.Awake();
            m_uiTuple = new[]
            {
                (m_xField, m_xSlider),
                (m_yField, m_ySlider),
                (m_zField, m_zSlider),
                (m_wField, m_wSlider),
            };
        }

        public override bool CanHandle(ICheat cheat) =>
            cheat is ValueCheat valueCheat && 
            TypeGroups.VectorTypes.Contains(valueCheat.ValueType);

        public override void Initialize(ICheat cheat)
        {
            base.Initialize(cheat);
            var vectorType = m_cheat.ValueType;
            m_axisCount = TypeGroups.ValuesCountDictionary[vectorType];
            m_isWholeNumber = TypeGroups.WholeNumberVectorTypes.Contains(vectorType);
            
            var cheatAttribute = cheat.Attributes[0];
            m_minValue = cheatAttribute.Min;
            m_maxValue = cheatAttribute.Max;

            var valute = m_cheat.Get();
            m_getterMethodInfo = vectorType.GetMethod("get_Item");
            m_setterMethodInfo = vectorType.GetMethod("set_Item");
            
            var fieldType = m_isWholeNumber ? typeof(int) : typeof(float);
            var parameters = new object[] { 0 };
            for (var i = 0; i < 4; i++)
            {
                parameters[0] = i;
                var pair = m_uiTuple[i];
                var validAxis = i < m_axisCount;
                var value = validAxis ? m_getterMethodInfo.Invoke(valute, parameters) : 0f;
                m_axisHandlers[i] = new AxisHandler(pair.Item1, pair.Item2, fieldType, value, m_minValue, m_maxValue);
                m_axisHandlers[i].SetActive(validAxis);
                m_axisHandlers[i].OnValueChanged += CreateInstance;
            }
        }

        private void CreateInstance()
        {
            var vectorType = m_cheat.ValueType;
            var instance = Activator.CreateInstance(vectorType);
            var valuesCount = TypeGroups.ValuesCountDictionary[vectorType];
            var parameters = new object[2];
            
            for (var i = 0; i < valuesCount; i++)
            {
                parameters[0] = i;
                parameters[1] = m_axisHandlers[i].Value;
                m_setterMethodInfo.Invoke(instance, parameters);
            }
            
            OnValueChanged.Invoke(instance);
        }


        public override void UpdateDisplay()
        {
            if (m_cheat == null) return;
            var currentValue = m_cheat.Get();
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
