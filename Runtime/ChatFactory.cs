using System.Reflection;

namespace cookie.Cheats
{
    public interface IChatFactory
    {
        ICheat Build(object target, MemberInfo memberInfo);
    }
    
    public class ChatFactory : IChatFactory
    {
        private int m_nextCheatId = 0;
        public ICheat Build(object target, MemberInfo memberInfo)
        {
            switch (memberInfo)
            {
                case FieldInfo fieldInfo:
                    return new FieldCheat(m_nextCheatId++, target, fieldInfo);
                case PropertyInfo propertyInfo:
                    return new PropertyCheat(m_nextCheatId++, target, propertyInfo);
                case MethodInfo methodInfo:
                    return new MethodCheat(m_nextCheatId++, target, methodInfo);
            }
            
            return null;
        }
    }
}