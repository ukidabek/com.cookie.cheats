using System.Linq;
using System.Reflection;

namespace cookie.Cheats
{
    public abstract class Cheat<T> : ICheat where T : MemberInfo
    {
        public int ID { get; }
        public readonly object Target;
        public string Name { get; }
        public readonly T MemberInfo = null;
        public CheatAttributeData[] Attributes { get; }
        
        protected Cheat(int id, object target, T memberInfo)
        {
            Target = target;
            MemberInfo = memberInfo;
            Name = memberInfo.Name;
            ID = id;
            Attributes = memberInfo.GetCustomAttributes<CheatAttribute>()
                .Select(attribute => new CheatAttributeData(attribute))
                .ToArray();
        }

        public abstract CheatData ToDataTransferObject();
    }
}