using System.Reflection;

namespace cookie.Cheats
{
    public interface ICheatFactory
    {
        ICheat Build(object target, MemberInfo memberInfo);
    }
    
    public class CheatFactory : ICheatFactory
    {
        private int m_nextCheatId = 0;
        
        public ICheat Build(object target, MemberInfo memberInfo)
        {
            return memberInfo switch
            {
                FieldInfo fieldInfo => TypeGroups.MultiValueTypes.Contains(fieldInfo.FieldType)
                    ? new MultipleValueCheat(m_nextCheatId++, target, fieldInfo)
                    : new ValueCheat(m_nextCheatId++, target, fieldInfo),
                PropertyInfo propertyInfo => TypeGroups.MultiValueTypes.Contains(propertyInfo.PropertyType)
                    ? new MultipleValueCheat(m_nextCheatId++, target, propertyInfo)
                    : new ValueCheat(m_nextCheatId++, target, propertyInfo),
                MethodInfo methodInfo => new MethodCheat(m_nextCheatId++, target, methodInfo),
                _ => null
            };
        }
    }
}