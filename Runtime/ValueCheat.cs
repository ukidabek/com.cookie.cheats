using System;
using System.Reflection;

namespace cookie.Cheats
{
    [Serializable]
    public class MultipleValueTypeProxy
    {
        public readonly object[] Values = null;

        public MultipleValueTypeProxy(object value)
        {
            var type = value.GetType();
            
            var valuesCount = TypeGroups.ValuesCountDictionary[type];
            Values = new object[valuesCount];

            var m_getterMethodInfo = type.GetMethod("get_Item");
            var parameters = new object[] { 0 };
            for (var i = 0; i < valuesCount; i++)
            {
                parameters[0] = i;
                Values[i] = m_getterMethodInfo.Invoke(value, parameters);
            }
        }

        public object Parse(Type type)
        {
            var instance = Activator.CreateInstance(type);
            var valuesCount = TypeGroups.ValuesCountDictionary[type];
            var m_setterMethodInfo = type.GetMethod("set_Item");
            var parameters = new object[] { 0, 0 };
            for (var i = 0; i < valuesCount; i++)
            {
                parameters[0] = i;
                parameters[1] = Values[i];
                m_setterMethodInfo.Invoke(instance, parameters);
            }

            return instance;
        }
    }
    
    public abstract class ValueCheat<T> : Cheat<T>, IValueCheat where T : MemberInfo
    {
        protected MemberFlags m_flags = MemberFlags.None;
        
        private object m_lastValue;
        public bool IsDirty
        {
            get
            {
                var currentValue = Get();
                if (Equals(currentValue, m_lastValue)) return false;
                m_lastValue = currentValue;
                return true;
            }
        }

        public Type ValueType { get; }
        public bool IsNumeric => m_flags.HasFlag(MemberFlags.IsNumeric);
        public bool IsWholeNumber => m_flags.HasFlag(MemberFlags.IsWholeNumber);
        public bool CanRead => m_flags.HasFlag(MemberFlags.CanRead);
        public bool CanWrite => m_flags.HasFlag(MemberFlags.CanWrite);
        public bool IsEnum => m_flags.HasFlag(MemberFlags.IsEnum);
        
        protected ValueCheat(int id, object target, T memberInfo, Type valueType, bool canRead, bool canWrite) 
            : base(id, target, memberInfo)
        {
            if (Attributes.Length > 1)
                throw new ArgumentException($"Value type cheat should have ony one {nameof(CheatAttribute)}!");
            
            ValueType = valueType;

            if (TypeGroups.NumericTypes.Contains(ValueType)) m_flags |= MemberFlags.IsNumeric;
            if (canRead) m_flags |= MemberFlags.CanRead;
            if (canWrite) m_flags |= MemberFlags.CanWrite;
            if (ValueType.IsEnum) m_flags |= MemberFlags.IsEnum;
            if (TypeGroups.MultiValueTypes.Contains(ValueType)) m_flags |= MemberFlags.IsMultipleValue;
            if (m_flags.HasFlag(MemberFlags.IsMultipleValue))
            {
                if (TypeGroups.WholeNumberVectorTypes.Contains(ValueType))
                    m_flags |= MemberFlags.IsWholeNumber;
            }
            else if (TypeGroups.WholeNumberTypes.Contains(ValueType)) 
                m_flags |= MemberFlags.IsWholeNumber;
        }

        public abstract object Get();
        
        public object GetSerialized()
        {
            var value = Get();
            return TypeGroups.MultiValueTypes.Contains(ValueType) ? 
                new MultipleValueTypeProxy(value) : 
                value;
        }

        public void MartAsDirty() => m_lastValue = null;

        public abstract void Set(object value);

        public override CheatData ToDataTransferObject()
        {
            return new ValueCheatData()
            {
                ID = ID,
                Name = Name,
                Attributes = Attributes,
                AssemblyQualifiedName = GetType().AssemblyQualifiedName,
                MemberFlags = m_flags,
                ValueAssemblyQualifiedName = ValueType.AssemblyQualifiedName,
            };
        }
    }
}