using System;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace cookie.Cheats.UI
{
    public abstract class ValueCheatHandler<ValueType> : CheatHandler
    {
        private static readonly Type ValueCheatType = typeof(ValueCheat);
        protected static readonly BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        protected PropertyInfo IsWholeNumberFieldInfo = null;
        protected PropertyInfo CanWriteFieldInfo = null;
        protected MethodInfo GetMethodInfo = null;
        protected MethodInfo SetMethodInfo = null;

        [SerializeField] private TMP_Text m_label = null;

        protected ICheat m_cheat = null;

        protected abstract UnityEvent<ValueType> OnValueChanged { get; }

        public override bool CanHandle(ICheat cheat)
        {
            var type = cheat.GetType();
            type = type.BaseType;
            return type.IsAssignableFrom(ValueCheatType);
        }

        public override void Initialize(ICheat cheat)
        {
            m_cheat = cheat;
            var type = m_cheat.GetType();

            IsWholeNumberFieldInfo = type.GetProperty("IsWholeNumber", bindingFlags);
            CanWriteFieldInfo = type.GetProperty("CanWrite", bindingFlags);
            GetMethodInfo = type.GetMethod("Get", bindingFlags);
            SetMethodInfo = type.GetMethod("Set", bindingFlags);

            var attribute = cheat.Attributes.First();
            m_label.text = string.IsNullOrEmpty(attribute.Name) ? m_cheat.Name : attribute.Name;
            OnValueChanged.AddListener(UpdateValue);
            UpdateDisplay();
        }

        private void OnEnable() => UpdateDisplay();

        protected abstract void UpdateValue(ValueType value);

        protected Type GetValueType(ICheat cheat)
        {
            var type = cheat.GetType();
            var valueTypeFieldInfo = type.GetProperty("ValueType", ValueCheatHandler<object>.bindingFlags);
            type = (Type)valueTypeFieldInfo.GetValue(cheat);
            return type;
        }
    }
}

namespace cookie.Cheats.Server
{
    public abstract class ValueCheatHandler<T> : ICheatHandler where T : ValueCheat
    {
        public Type CheatType => typeof(T);

        public void Handle(ICheat cheat, CheatPayload payload)
        {
            var fieldCheat = (T)cheat;
            var value = payload.Parameters.First();
            if (value is IProxy proxy)
                value = proxy.Parse();
            fieldCheat.Set(value);
        }
    }
    
}