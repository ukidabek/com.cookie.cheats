using System;
using System.Linq;
using cookie.Cheats.Server;
using UnityEditor;
using UnityEngine;

namespace cookie.Cheats
{
    public class MultipleValueEditorCheatBuilder : IEditorCheatBuilder
    {
        private class MultipleValueEditorCheat : IEditorCheat
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

            private int m_valuesCount = 0;
            private float[] m_floatValues = null;
            private int[] m_intValues = null;

            public MultipleValueEditorCheat(ValueCheatData data)
            {
                ID = data.ID;
                Name = data.Name;
                Attributes = data.Attributes;
                m_flags = data.MemberFlags;
                ValueType = Type.GetType(data.ValueAssemblyQualifiedName);

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
                }

                if (!EditorGUI.EndChangeCheck()) return;
                {
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

                    Update.Invoke(new CheatPayload()
                    {
                        ID = ID,
                        Parameters = new object[]
                        {
                            new MultipleValueTypeProxy(instance)
                        },
                    });
                }
            }

            public void SetValue(object value)
            {
                var proxy = (MultipleValueTypeProxy)value;
                if (m_flags.HasFlag(MemberFlags.IsWholeNumber))
                {
                    m_intValues = proxy.Values
                        .Select(data => Convert.ChangeType(data, typeof(int)))
                        .OfType<int>()
                        .ToArray();
                }
                else
                {
                    m_floatValues = proxy.Values
                        .Select(data => Convert.ChangeType(data, typeof(float)))
                        .OfType<float>()
                        .ToArray();
                }
            }
        }

        public Type Type => typeof(MultipleValueCheat);

        public IEditorCheat Build(CheatData data) => new MultipleValueEditorCheat(data as ValueCheatData);
    }
}