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
            public bool Equals(MemberInfo memberInfoA, MemberInfo memberInfoB)
            {
                if (memberInfoA is null || memberInfoB is null)
                    return false;

                return memberInfoA.MetadataToken == memberInfoB.MetadataToken &&
                       memberInfoA.Module == memberInfoB.Module;
            }

            public int GetHashCode(MemberInfo memberInfo) => HashCode.Combine(memberInfo.MetadataToken, memberInfo.Module);
        }
        
        private const BindingFlags BindingFlag = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        private static readonly Type MonoBehaviorType = typeof(MonoBehaviour);

        private static CheatDatabase m_instance = null;
        public static CheatDatabase Instance
        {
            get
            {
                m_instance ??= new CheatDatabase();
                return m_instance;
            }
        }
        private readonly MemberInfoEqualityComparer m_comparer = new MemberInfoEqualityComparer();
        
        private readonly Dictionary<Type, IReadOnlyList<MemberInfo>> m_membersDictionary = new Dictionary<Type, IReadOnlyList<MemberInfo>>(30);

        private readonly Dictionary<CheatProvider, List<ICheat>> m_providerChetDictionary = new Dictionary<CheatProvider, List<ICheat>>(30);
        public IReadOnlyDictionary<CheatProvider, List<ICheat>> ProviderChetDictionary => m_providerChetDictionary;
        private readonly Dictionary<int, ICheat> m_chetDictionary = new Dictionary<int, ICheat>(100);
        public IReadOnlyDictionary<int, ICheat> ChetDictionary => m_chetDictionary;
        
        private ICheatFactory m_cheatFactory = new CheatFactory();
        
        public event Action<CheatProvider, List<ICheat>> OnCheatsRegistered;
        public event Action<CheatProvider, List<int>> OnCheatsUnregistered;
        
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
                
                list.AddRange(members.Select(member =>
                {
                    var cheat = m_cheatFactory.Build(monoBehaviour, member);
                    m_chetDictionary.Add(cheat.ID, cheat);
                    return cheat;
                }));
            }
            
            m_providerChetDictionary.Add(cheatProvider, list);
            OnCheatsRegistered?.Invoke(cheatProvider, list);
        }

        public void Unregister(CheatProvider cheatProvider)
        {
            var idList = new List<int>();
            if (!m_providerChetDictionary.TryGetValue(cheatProvider, out var list)) return;
            foreach (var member in list)
            {
                m_chetDictionary.Remove(member.ID);
                idList.Add(member.ID);
            }
            m_providerChetDictionary.Remove(cheatProvider);
            OnCheatsUnregistered?.Invoke(cheatProvider, idList);
        }

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