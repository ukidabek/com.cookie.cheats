using System;
using System.Linq;
using cookie.Cheats.Server;
using UnityEditor;
using UnityEngine;

namespace cookie.Cheats
{
    public class FieldCheatBuilder : IEditorCheatBuilder
    {
        private class EditorFieldCheat : IEditorCheat
        {
            public int ID { get; }
            public string Name { get; }
            public bool IsDirty => false;
            public CheatAttributeData[] Attributes { get; }
            
            public Type ValueType { get; }

            public CheatData ToDataTransferObject() => null;

            public event Action<CheatPayload> Update;
            
            private MemberFlags m_flags = MemberFlags.None;
            private readonly MemberFlags m_mask = MemberFlags.IsNumeric | MemberFlags.IsWholeNumber | MemberFlags.IsEnum | MemberFlags.IsMultipleValue;
            private readonly MemberFlags m_maskedFlags;
            
            private Array m_enumValues = null;
            private string[] m_enumValuesNames = null;
            private int m_index = 0;
            
            private int m_intValue = 0;
            private float m_floatValue = 0;
            private bool m_boolValue = false;
            
            private int m_valuesCount = 0;
            private float[] m_floatValues = null;
            private int[] m_intValues = null;

            public EditorFieldCheat(ValueCheatData data)
            {
                ID = data.ID;
                Name = data.Name;
                Attributes = data.Attributes;
                m_flags = data.MemberFlags;
                ValueType = Type.GetType(data.ValueAssemblyQualifiedName);

                if (m_flags.HasFlag(MemberFlags.IsEnum))
                {
                    m_enumValues = Enum.GetValues(ValueType);
                    m_enumValuesNames = Enum.GetNames(ValueType);
                }

                if (m_flags.HasFlag(MemberFlags.IsMultipleValue))
                {
                    m_valuesCount = TypeGroups.ValuesCountDictionary[ValueType];
                    if (m_flags.HasFlag(MemberFlags.IsWholeNumber))
                        m_intValues = new int[m_valuesCount];
                    else
                        m_floatValues = new float[m_valuesCount];
                }

                m_maskedFlags = m_flags & m_mask;
            }
            
            public void OnGUI()
            {
                EditorGUI.BeginChangeCheck(); 
                var attribute = Attributes.First();
                var name = string.IsNullOrEmpty(attribute.Name) ? Name : attribute.Name;
             
                switch (m_maskedFlags)
                {
                    case MemberFlags.IsNumeric | MemberFlags.IsWholeNumber:
                        m_intValue = EditorGUILayout.IntSlider(name, m_intValue, (int)attribute.Min, (int)attribute.Max);
                        break;
                    case MemberFlags.IsNumeric:
                        m_floatValue = EditorGUILayout.Slider(name, m_floatValue, attribute.Min, attribute.Max);
                        break;
                    case MemberFlags.IsEnum:
                        m_index = EditorGUILayout.Popup(name, m_index, m_enumValuesNames);
                        break;
                    case MemberFlags.IsMultipleValue | MemberFlags.IsWholeNumber:
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(name, GUILayout.Width(EditorGUIUtility.labelWidth));
                        for (var i = 0; i < m_valuesCount; i++) 
                            m_intValues[i] = EditorGUILayout.IntSlider(m_intValues[i], (int)attribute.Min, (int)attribute.Max);
                        GUILayout.EndHorizontal();
                        break;
                    case MemberFlags.IsMultipleValue:
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(name, GUILayout.Width(EditorGUIUtility.labelWidth));
                        for (var i = 0; i < m_valuesCount; i++) 
                            m_floatValues[i] = EditorGUILayout.Slider(m_floatValues[i], attribute.Min, attribute.Max);
                        GUILayout.EndHorizontal();
                        break;
                    default:
                        m_boolValue = EditorGUILayout.Toggle(name, m_boolValue);
                        break;
                }
                
                if (EditorGUI.EndChangeCheck())
                {
                   
                    object[] parameters;

                    switch (m_maskedFlags)
                    {
                        case MemberFlags.IsNumeric | MemberFlags.IsWholeNumber:
                            parameters = new object[] { m_intValue };
                            break;
                        case MemberFlags.IsNumeric:
                            parameters = new object[] { m_floatValue };
                            break;
                        case MemberFlags.IsEnum:
                            parameters = new object[] { m_enumValues.GetValue(m_index) };
                            break;
                        case MemberFlags.IsMultipleValue :
                            var instance = Activator.CreateInstance(ValueType);
                            var valuesCount = TypeGroups.ValuesCountDictionary[ValueType];
                            var m_setterMethodInfo = ValueType.GetMethod("set_Item");
                            var _parameters = new object[] { 0, 0 };
                            for (var i = 0; i < valuesCount; i++)
                            {
                                _parameters[0] = i;
                                var whole = m_flags.HasFlag(MemberFlags.IsWholeNumber);
                                _parameters[1] = whole ? m_intValues[i] : m_floatValues[i];
                                m_setterMethodInfo.Invoke(instance, _parameters);
                            }
                            parameters = new object[] { new MultipleValueTypeProxy(instance) };
                            break;
                        default:
                            parameters = new object[] { m_boolValue };
                            break;
                    }
                    
                    Update.Invoke(new CheatPayload(ID, parameters));}
            }
            
            public void SetValue(object value)
            {
                switch (m_maskedFlags)
                {
                    case MemberFlags.IsNumeric | MemberFlags.IsWholeNumber:
                        m_intValue = (int)value;
                        break;
                    case MemberFlags.IsNumeric:
                        m_floatValue = (float)value;
                        break;
                    case MemberFlags.IsEnum:
                        m_index = Array.IndexOf(m_enumValues, value);
                        break;
                    case MemberFlags.IsMultipleValue | MemberFlags.IsWholeNumber:
                    case MemberFlags.IsMultipleValue:
                        var proxy = (MultipleValueTypeProxy)value;
                        if (m_flags.HasFlag(MemberFlags.IsWholeNumber))
                            m_intValues = proxy.Values.OfType<int>().ToArray();
                        else
                            m_floatValues = proxy.Values.OfType<float>().ToArray();
                        break;
                    default:
                        m_boolValue = (bool)value;
                        break;
                }
            }
        }

        public virtual Type Type => typeof(FieldCheat);

        public IEditorCheat Build(CheatData data) => new EditorFieldCheat(data as ValueCheatData);
    }
}