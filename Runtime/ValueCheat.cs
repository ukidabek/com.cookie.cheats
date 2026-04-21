using System;
using System.Reflection;

namespace cookie.Cheats
{
    public abstract class ValueCheat : Cheat, IValueCheat
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
        public bool IsMultipleValue => m_flags.HasFlag(MemberFlags.IsMultipleValue);

        protected ValueCheat(int id, object target, PropertyInfo propertyInfo)
            : this(id, target, propertyInfo, propertyInfo.PropertyType, propertyInfo.CanRead, propertyInfo.CanWrite)
        {
            
        }

        protected ValueCheat(int id, object target, FieldInfo fieldInfo)
            : this(id, target, fieldInfo, fieldInfo.FieldType, canRead: true, canWrite: true)
        {
        }
        
        private ValueCheat(int id, object target, MemberInfo memberInfo, Type valueType, bool canRead, bool canWrite) 
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

        public object Get()
        {
            return MemberInfo switch
            {
                FieldInfo fieldInfo => fieldInfo.GetValue(Target),
                PropertyInfo propertyInfo => !propertyInfo.CanRead ? null : propertyInfo.GetValue(Target),
                _ => null
            };
        }
        
        public object ToSerializableObject()
        {
            return Get();
            //--------------
            var value = Get();
            return TypeGroups.MultiValueTypes.Contains(ValueType) ? 
                new MultipleValueTypeProxy(value) : 
                value;
        }

        public void MartAsDirty() => m_lastValue = null;

        public void Set(object value)
        {
            switch (MemberInfo)
            {
                case FieldInfo fieldInfo:
                    fieldInfo.SetValue(Target, value);
                    break;
                case PropertyInfo propertyInfo:
                    if (!propertyInfo.CanWrite) break;
                    propertyInfo.SetValue(Target, value);
                    break;
            }
        }

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