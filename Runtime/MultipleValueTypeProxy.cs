using System;
using Newtonsoft.Json;

namespace cookie.Cheats
{
    [Serializable]
    public class MultipleValueTypeProxy : IProxy
    {
        public string AssemblyQualifiedName = string.Empty;
        public object[] Values = null;

        [JsonConstructor]
        public MultipleValueTypeProxy(string assemblyQualifiedName, object[] values)
        {
            AssemblyQualifiedName = assemblyQualifiedName;
            Values = values;
        }
        
        public MultipleValueTypeProxy(object value)
        {
            var type = value.GetType();
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
            var instance = Activator.CreateInstance(type);
            var valuesCount = TypeGroups.ValuesCountDictionary[type];
            var m_setterMethodInfo = type.GetMethod("set_Item");
            var parameters = new object[] { 0, 0 };
            for (var i = 0; i < valuesCount; i++)
            {
                parameters[0] = i;
                parameters[1] = Values[i];
                m_setterMethodInfo.Invoke(instance, parameters);
            }

            return instance;
        }
    }
}