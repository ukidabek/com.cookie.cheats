using System;
using System.Reflection;

namespace cookie.Cheats
{
    [Serializable]
    public class MethodCheat : Cheat
    {
        private readonly MethodInfo m_methodInfo = null;
        
        public MethodCheat(int id, object target, MethodInfo memberInfo) : base(id, target, memberInfo)
        {
            m_methodInfo = memberInfo;
        }

        public void Invoke(object[] parameters)
        {
            m_methodInfo.Invoke(Target, parameters);
        }

        public override CheatData ToDataTransferObject()
        {
            return new CheatData()
            {
                ID = ID,
                Name = Name,
                AssemblyQualifiedName = GetType().AssemblyQualifiedName,
                Attributes = Attributes,
            };
        }
    }
}