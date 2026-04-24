using System.Reflection;

namespace cookie.Cheats
{
    public class MultipleValueCheat : ValueCheat
    {
        public MultipleValueCheat(int id, object target, PropertyInfo propertyInfo) : base(id, target, propertyInfo)
        {
        }

        public MultipleValueCheat(int id, object target, FieldInfo fieldInfo) : base(id, target, fieldInfo)
        {
        }

        public override object ToSerializableObject() => new MultipleValueTypeProxy(Get());
    }
}