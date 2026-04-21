using System.Reflection;

namespace cookie.Cheats
{
    public class FieldCheat : ValueCheat
    {
        public FieldCheat(int id, object target, FieldInfo memberInfo) 
            : base(id, target, memberInfo)
        {
        }
    }
}