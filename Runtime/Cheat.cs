using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace cookie.Cheats
{
    [Serializable]
    public abstract class Cheat<T> : ICheat where T : MemberInfo
    {
        public int ID { get; }
        public object Target { get; }
        public string Name { get; }

        [NonSerialized] public readonly T MemberInfo = null;

        public CheatData[] Attributes { get; }

        protected Cheat(int id, object target, T memberInfo)
        {
            Target = target;
            MemberInfo = memberInfo;
            Name = memberInfo.Name;
            ID = id;
            Attributes = memberInfo.GetCustomAttributes<CheatAttribute>()
                .Select(attribute => new CheatData(attribute))
                .ToArray();
        }
    }
}