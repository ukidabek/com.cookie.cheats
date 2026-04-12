using System.Reflection;

namespace cookie.Cheats
{
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
}