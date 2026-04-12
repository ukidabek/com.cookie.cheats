using System;
using System.Collections.Generic;
using System.Reflection;

namespace cookie.Cheats
{
    public abstract class ValueCheat<T> : Cheat<T> where T : MemberInfo
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
        
        protected ValueCheat(object target, T memberInfo, Type valueType, bool canRead, bool canWrite) 
            : base(target, memberInfo)
        {
            if (Attributes.Count > 1)
                throw new ArgumentException($"Value type cheat should have ony one {nameof(CheatAttribute)}!");
            
            ValueType = valueType;
            IsNumeric = NumericTypes.Contains(ValueType);
            IsWholeNumber = WholeNumberTypes.Contains(ValueType);
            CanRead = canRead;
            CanWrite = canWrite;
        }

        public abstract object Get();
        public abstract void Set(object value);
    }
}