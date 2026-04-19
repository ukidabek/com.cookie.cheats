using System;
using System.Reflection;

namespace cookie.Cheats
{
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
            if (TypeGroups.WholeNumberTypes.Contains(ValueType)) m_flags |= MemberFlags.IsWholeNumber;
            if (canRead) m_flags |= MemberFlags.CanRead;
            if (canWrite) m_flags |= MemberFlags.CanWrite;
            if (ValueType.IsEnum) m_flags |= MemberFlags.IsEnum;
        }

        public abstract object Get();
        
        public object GetSerialized()
        {
            var value = Get();

            if (TypeGroups.MultiValueTypes.Contains(ValueType))
                return null;
            
            return value;
        }
        
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