using System;
using System.Reflection;

namespace cookie.Cheats
{
    public class FieldCheat : ValueCheat<FieldInfo>
    {
        public FieldCheat(object target, FieldInfo memberInfo) 
            : base(target, memberInfo, memberInfo.FieldType, true, true)
        {
        }

        public override object Get() => MemberInfo.GetValue(Target);

        public override void Set(object value)
        {
            value = Convert.ChangeType(value, ValueType);
            MemberInfo.SetValue(Target, value);
        }
    }
}