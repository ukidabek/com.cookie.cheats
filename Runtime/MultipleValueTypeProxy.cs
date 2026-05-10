using System;

namespace cookie.Cheats
{
    [Serializable]
    public class MultipleValueTypeProxy : IProxy
    {
        public string AssemblyQualifiedName;
        public object[] Values = null;
        public bool IsWholeNumber = false;

        public MultipleValueTypeProxy()
        {
        }
        
        public MultipleValueTypeProxy(object value)
        {
            var type = value.GetType();

            IsWholeNumber = TypeGroups.WholeNumberVectorTypes.Contains(type);
            
            AssemblyQualifiedName = type.AssemblyQualifiedName;
            var valuesCount = TypeGroups.ValuesCountDictionary[type];
            Values = new object[valuesCount];

            var m_getterMethodInfo = type.GetMethod("get_Item");
            var parameters = new object[] { 0 };
            for (var i = 0; i < valuesCount; i++)
            {
                parameters[0] = i;
                Values[i] = m_getterMethodInfo.Invoke(value, parameters);
            }
        }

        public object Parse()
        {
            var type = Type.GetType(AssemblyQualifiedName);
            var valueType = IsWholeNumber ? typeof(int) : typeof(float);
            var instance = Activator.CreateInstance(type);
            var valuesCount = TypeGroups.ValuesCountDictionary[type];
            var m_setterMethodInfo = type.GetMethod("set_Item");
            var parameters = new object[] { 0, 0 };
            for (var i = 0; i < valuesCount; i++)
            {
                parameters[0] = i;
                parameters[1] = Convert.ChangeType(Values[i], valueType);
                m_setterMethodInfo.Invoke(instance, parameters);
            }

            return instance;
        }
    }
}