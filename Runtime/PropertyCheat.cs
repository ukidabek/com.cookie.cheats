using System;
using System.Reflection;

namespace cookie.Cheats
{
    public class PropertyCheat : ValueCheat<PropertyInfo>
    {
        public PropertyCheat(int id, object target, PropertyInfo propertyInfo) 
            : base(id, target, propertyInfo, propertyInfo.PropertyType, propertyInfo.CanRead, propertyInfo.CanWrite)
        {
        }

        public override object Get() => !CanRead ? null : MemberInfo.GetValue(Target);

        public override void Set(object value)
        {
            if (!CanWrite) return;
            value = Convert.ChangeType(value, ValueType);
            MemberInfo.SetValue(Target, value);
        }
    }
}