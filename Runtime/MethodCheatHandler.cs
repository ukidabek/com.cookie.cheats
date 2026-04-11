using System;
using System.Linq;
using System.Reflection;
using Codice.CM.SEIDInfo;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace cookie.Cheats
{
    public class MethodCheatHandler : CheatHandler<MethodCheat>
    {
        [SerializeField] private CheatButton m_cheatButtonPrefab = null;
        [SerializeField] private Transform m_parent = null;
        
        public override void Initialize(ICheat cheat)
        {
            base.Initialize(cheat);
            foreach (var attribute in m_cheat.Attributes)
            {
                var button = Instantiate(m_cheatButtonPrefab, m_parent);
                var name = string.IsNullOrEmpty(attribute.Name) ? m_cheat.Name : attribute.Name;
                button.gameObject.SetActive(true);
                button.Initialize(name, m_cheat.Target, m_cheat.MemberInfo, attribute.Parameters);
            }
        }
    }

    public abstract class ValueCheatHandler<T> : CheatHandler
    {
        private static readonly Type ValueCheatType = typeof(ValueCheat<>);
        protected static readonly BindingFlags  bindingFlags = BindingFlags.Public | BindingFlags.Instance  | BindingFlags.FlattenHierarchy;
        
        protected PropertyInfo ValueTypeFieldInfo = null;
        protected PropertyInfo IsWholeNumberFieldInfo = null;
        protected PropertyInfo CanWriteFieldInfo = null;
        protected MethodInfo GetMethodInfo = null;
        protected MethodInfo SetMethodInfo = null;
        
        [SerializeField] private TMP_Text m_label = null;
        
        protected ICheat m_cheat = null;
        protected abstract UnityEvent<T> OnValueChanged { get; }
        
        public override bool CanHandle(ICheat cheat)
        {
            var type = cheat.GetType();
            type = type.BaseType;
            if (!type.IsGenericType) return false;
            return type.GetGenericTypeDefinition() == ValueCheatType;
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
        
        protected abstract void UpdateDisplay();
        
        protected abstract void UpdateValue(T value);
    }
}