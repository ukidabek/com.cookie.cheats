using System;
using System.Reflection;

namespace cookie.Cheats
{
    public class FieldCheat : ValueCheat<FieldInfo>
    {
        public FieldCheat(int id, object target, FieldInfo memberInfo) 
            : base(id, target, memberInfo, memberInfo.FieldType, true, true)
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