using System.Reflection;

namespace cookie.Cheats
{
    public interface IChatFactory
    {
        ICheat Build(object target, MemberInfo memberInfo);
    }
    
    public class ChatFactory : IChatFactory
    {
        public ICheat Build(object target, MemberInfo memberInfo)
        {
            switch (memberInfo)
            {
                case FieldInfo fieldInfo:
                    return new FieldCheat(target, fieldInfo);
                case PropertyInfo propertyInfo:
                    return new PropertyCheat(target, propertyInfo);
                case MethodInfo methodInfo:
                    return new MethodCheat(target, methodInfo);
            }
            
            return null;
        }
    }
}