using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace cookie.Cheats
{
    public class CheatDatabase
    {
        private class MemberInfoEqualityComparer : IEqualityComparer<MemberInfo>
        {
            public bool Equals(MemberInfo x, MemberInfo y)
            {
                if (x is null || y is null)
                    return false;

                return x.MetadataToken == y.MetadataToken &&
                       x.Module == y.Module;
            }

            public int GetHashCode(MemberInfo obj) => HashCode.Combine(obj.MetadataToken, obj.Module);
        }
        
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
        private readonly MemberInfoEqualityComparer m_comparer = new MemberInfoEqualityComparer();
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

        public void Unregister(CheatProvider cheatProvider) => m_chetDictionary.Remove(cheatProvider);
        
        private IEnumerable<MemberInfo> ExtractCheats(MonoBehaviour component)
        {
            return ExtractMembers(component.GetType()).Distinct(m_comparer);

            IEnumerable<MemberInfo> ExtractMembers(Type type)
            {
                var extractMembers = type
                    .GetMembers(BindingFlag)
                    .Where(memberInfo => memberInfo.GetCustomAttributes<CheatAttribute>().Any());
                
                return type.BaseType  == MonoBehaviorType ? extractMembers : extractMembers.Concat(ExtractMembers(type.BaseType));
            }
        }
    }
}