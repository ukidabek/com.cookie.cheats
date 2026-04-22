using System.Linq;
using System.Reflection;

namespace cookie.Cheats
{
    public abstract class Cheat: ICheat
    {
        public int ID { get; }
        
        public readonly object Target;
        
        public string Name { get; }
        
        public readonly MemberInfo MemberInfo = null;
        
        public CheatAttributeData[] Attributes { get; }
        
        protected Cheat(int id, object target, MemberInfo memberInfo)
        {
            Target = target;
            MemberInfo = memberInfo;
            Name = MemberInfo.Name;
            ID = id;
            Attributes = memberInfo.GetCustomAttributes<CheatAttribute>()
                .Select(attribute => new CheatAttributeData(attribute))
                .ToArray();
        }

        public abstract CheatData ToDataTransferObject();
    }
}