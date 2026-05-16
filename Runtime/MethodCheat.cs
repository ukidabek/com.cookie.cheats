using System;
using System.Linq;
using System.Reflection;

namespace cookie.Cheats
{
    [Serializable]
    public class MethodCheat : Cheat
    {
        private readonly MethodInfo m_methodInfo = null;
        private readonly Type[] m_parameterTypes = null;
        
        public MethodCheat(int id, object target, MethodInfo memberInfo) : base(id, target, memberInfo)
        {
            m_methodInfo = memberInfo;
            m_parameterTypes = memberInfo.GetParameters().Select(parameterInfo => parameterInfo.ParameterType).ToArray();
        }

        public void Invoke(object[] parameters)
        {
            if (parameters != null && parameters.Any())
            {
                var lenght = parameters.Length;
                for (var i = 0; i < lenght; i++)
                    parameters[i] = Convert.ChangeType(parameters[i], m_parameterTypes[i]);
            }

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