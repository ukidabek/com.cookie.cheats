using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace cookie.Cheats
{
    public interface ICheat
    {
        int ID { get; }
        object Target { get; }
        string Name { get; }
        public IReadOnlyList<CheatAttribute> Attributes { get; }
    }

    public abstract class Cheat<T> : ICheat where T : MemberInfo
    {
        private static int ChatID = 0;

        public int ID { get; }
        public object Target { get; }
        public string Name { get; }

        public readonly T MemberInfo = null;
        
        public IReadOnlyList<CheatAttribute> Attributes { get; }

        protected Cheat(object target, T memberInfo)
        {
            Target = target;
            MemberInfo = memberInfo;
            Name = memberInfo.Name;
            ID = ChatID++;
            Attributes = memberInfo.GetCustomAttributes<CheatAttribute>().ToArray();
        }
    }

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

    public class FieldCheat : ValueCheat<FieldInfo>
    {
        public FieldCheat(object target, FieldInfo memberInfo) 
            : base(target, memberInfo, memberInfo.FieldType, true, true)
        {
        }

        public override object Get() => MemberInfo.GetValue(Target);

        public override void Set(object value) => MemberInfo.SetValue(Target, value);
    }

    public class PropertyCheat : ValueCheat<PropertyInfo>
    {
        public PropertyCheat(object target, PropertyInfo propertyInfo) 
            : base(target, propertyInfo, propertyInfo.PropertyType, propertyInfo.CanRead, propertyInfo.CanWrite)
        {
        }

        public override object Get() => !CanRead ? null : MemberInfo.GetValue(Target);

        public override void Set(object value)
        {
            if (!CanWrite) return;
            MemberInfo.SetValue(Target, value);
        }
    }

    
    public class MethodCheat : Cheat<MethodInfo>
    {
        public MethodCheat(object target, MethodInfo memberInfo) : base(target, memberInfo)
        {
        }

        public void Invoke(object[] parameters)
        {
            MemberInfo.Invoke(Target, parameters);
        }
    }


    public class CheatDatabase
    {
        private const BindingFlags BindingFlag = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        private static readonly Type MonoBehaviorType = typeof(MonoBehaviour);

        private static CheatDatabase m_instance = null;
        internal static CheatDatabase Instance
        {
            get
            {
                m_instance ??= new CheatDatabase();
                return m_instance;
            }
        }
        
        private readonly Dictionary<Type, IReadOnlyList<MemberInfo>> m_membersDictionary = new Dictionary<Type, IReadOnlyList<MemberInfo>>(30);
        private readonly Dictionary<CheatProvider, List<ICheat>> m_chetDictionary = new Dictionary<CheatProvider, List<ICheat>>(30);
        public IReadOnlyDictionary<CheatProvider, List<ICheat>> ChetDictionary => m_chetDictionary;
        
        private IChatFactory m_chatFactory = new ChatFactory();
        
        public event Action<CheatProvider, List<ICheat>> OnCheatsRegistered;

        public void Register(CheatProvider cheatProvider)
        {
            var list = new List<ICheat>(30);
            foreach (var monoBehaviour in cheatProvider.Components)
            {
                if(!m_membersDictionary.TryGetValue(monoBehaviour.GetType(), out var members))
                {
                    members = new List<MemberInfo>(ExtractCheats(monoBehaviour));
                    m_membersDictionary.Add(monoBehaviour.GetType(), members);
                }
                
                list.AddRange(members.Select(member => m_chatFactory.Build(monoBehaviour, member)));
            }
            
            m_chetDictionary.Add(cheatProvider, list);
            OnCheatsRegistered?.Invoke(cheatProvider, list);
        }

        public void Unregister(CheatProvider cheatProvider)
        {
            m_chetDictionary.Remove(cheatProvider);
        }
        
        private IEnumerable<MemberInfo> ExtractCheats(MonoBehaviour component)
        {
            IEnumerable<MemberInfo> ExtractMembers(Type type)
            {
                var extractMembers = type
                    .GetMembers(BindingFlag)
                    .Where(memberInfo => memberInfo.GetCustomAttributes<CheatAttribute>().Any());
                
                return type.BaseType  == MonoBehaviorType ? extractMembers : extractMembers.Concat(ExtractMembers(type.BaseType));
            }
           
            return ExtractMembers(component.GetType());;
        }
    }
}