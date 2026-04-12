using System;
using System.Reflection;

namespace cookie.Cheats
{
    [Serializable]
    public class MethodCheat : Cheat<MethodInfo>
    {
        public MethodCheat(int id, object target, MethodInfo memberInfo) : base(id, target, memberInfo)
        {
        }

        public void Invoke(object[] parameters)
        {
            MemberInfo.Invoke(Target, parameters);
        }
    }
}