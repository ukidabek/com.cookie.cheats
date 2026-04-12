using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace cookie.Cheats
{
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
}