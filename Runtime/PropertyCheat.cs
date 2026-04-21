using System.Reflection;

namespace cookie.Cheats
{
    public class PropertyCheat : ValueCheat
    {
        public PropertyCheat(int id, object target, PropertyInfo propertyInfo)
            : base(id, target, propertyInfo)
        {
        }
    }
}