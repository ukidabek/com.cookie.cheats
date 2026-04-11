using System;
using System.Reflection;
using Codice.CM.SEIDInfo;
using UnityEngine;

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

    public abstract class ValueCheatHandler : CheatHandler
    {
        protected static readonly Type ValueCheatType = typeof(ValueCheat<>);
        protected static readonly FieldInfo ValueTypeFieldInfo = null;
        protected static readonly FieldInfo IsWholeNumberFieldInfo = null;
        protected static readonly FieldInfo CanWriteFieldInfo = null;
        protected static readonly MethodInfo GetMethodInfo = null;
        protected static readonly MethodInfo SetMethodInfo = null;
        
        static ValueCheatHandler()
        {
            var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            ValueTypeFieldInfo = ValueCheatType.GetField(nameof(ValueCheat<MemberInfo>.ValueType), bindingFlags);
            IsWholeNumberFieldInfo = ValueCheatType.GetField(nameof(ValueCheat<MemberInfo>.IsWholeNumber), bindingFlags);
            CanWriteFieldInfo = ValueCheatType.GetField(nameof(ValueCheat<MemberInfo>.CanWrite), bindingFlags);
            GetMethodInfo = ValueCheatType.GetMethod(nameof(ValueCheat<MemberInfo>.Get), bindingFlags);
            SetMethodInfo = ValueCheatType.GetMethod(nameof(ValueCheat<MemberInfo>.Set), bindingFlags);
        }
        
        public override bool CanHandle(ICheat cheat)
        {
            var type = cheat.GetType();
            return type.IsSubclassOf(ValueCheatType);
        }

        public override void Initialize(ICheat cheat)
        {
            
        }
    }

    public class ToggleCheatHandler : ValueCheatHandler
    {
        private static readonly Type BoolType = typeof(bool);
        
        public override bool CanHandle(ICheat cheat)
        {
            if(!base.CanHandle(cheat)) return false;
            var type = (Type)ValueTypeFieldInfo.GetValue(cheat);
            return type == BoolType;

        }

        public override void Initialize(ICheat cheat)
        {
            throw new System.NotImplementedException();
        }
    }
}