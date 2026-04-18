using System;
using System.Collections.Generic;
using System.Reflection;

namespace cookie.Cheats
{
    [Serializable]
    public class ValueCheatData : CheatData
    {
        public object Value;
        public bool IsNumeric;
        public bool IsWholeNumber;
        public bool CanRead;
        public bool CanWrite;
    }

    public interface IValueCheat
    {
        bool IsDirty { get; }
        object Get();
        void Set(object value);
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
        
        public Type ValueType { get; }
        public bool IsNumeric { get; }
        public bool IsWholeNumber { get; }
        public bool CanRead { get; }
        public bool CanWrite { get; }
        
        protected ValueCheat(int id, object target, T memberInfo, Type valueType, bool canRead, bool canWrite) 
            : base(id, target, memberInfo)
        {
            if (Attributes.Length > 1)
                throw new ArgumentException($"Value type cheat should have ony one {nameof(CheatAttribute)}!");
            
            ValueType = valueType;
            IsNumeric = NumericTypes.Contains(ValueType);
            IsWholeNumber = WholeNumberTypes.Contains(ValueType);
            CanRead = canRead;
            CanWrite = canWrite;
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
                Value = Get(),
                IsNumeric =  IsNumeric,
                IsWholeNumber = IsWholeNumber,
                CanRead = CanRead,
                CanWrite =  CanWrite
            };
        }
    }
}