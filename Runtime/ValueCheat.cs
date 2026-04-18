using System;
using System.Collections.Generic;
using System.Reflection;

namespace cookie.Cheats
{
    [Serializable]
    public class ValueCheatData : CheatData
    {
        public MemberFlags MemberFlags;
        public string ValueAssemblyQualifiedName;
    }

    public interface IValueCheat
    {
        bool IsDirty { get; }
        object Get();
        void Set(object value);
    }

    [Flags]
    public enum MemberFlags : byte
    {
        None          = 0,
        IsNumeric     = 1 << 0, // 0000 0001
        IsWholeNumber = 1 << 1, // 0000 0010
        CanRead       = 1 << 2, // 0000 0100
        CanWrite      = 1 << 3, // 0000 1000
        IsEnum        = 1 << 4  // 0001 0000
    }


    public abstract class ValueCheat<T> : Cheat<T>, IValueCheat where T : MemberInfo
    {
        private static readonly HashSet<Type> NumericTypes = new()
        {
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(float),
            typeof(double),
            typeof(decimal)
        };
        
        private static readonly HashSet<Type> WholeNumberTypes = new()
        {
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(ushort),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong)
        };
        
        protected MemberFlags m_flags = MemberFlags.None;
        
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
            
            IsDirty = true;
            
            if (NumericTypes.Contains(ValueType)) m_flags |= MemberFlags.IsNumeric;
            if (WholeNumberTypes.Contains(ValueType)) m_flags |= MemberFlags.IsWholeNumber;
            if (canRead) m_flags |= MemberFlags.CanRead;
            if (canWrite) m_flags |= MemberFlags.CanWrite;
            if (ValueType.IsEnum) m_flags |= MemberFlags.IsEnum;
        }

        public abstract object Get();
        
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