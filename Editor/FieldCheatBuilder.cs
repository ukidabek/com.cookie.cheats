using System;
using System.Linq;
using cookie.Cheats.Server;
using UnityEditor;

namespace cookie.Cheats
{
    public class FieldCEditorCheatBuilder : IEditorCheatBuilder
    {
        private class FieldCEditorCheat : IEditorCheat
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

            public FieldCEditorCheat(ValueCheatData data)
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
                    default:
                        m_boolValue = EditorGUILayout.Toggle(name, m_boolValue);
                        break;
                }

                if (!EditorGUI.EndChangeCheck()) return;

                Update.Invoke(new CheatPayload(ID, m_maskedFlags switch
                {
                    MemberFlags.IsNumeric | MemberFlags.IsWholeNumber => new object[] { m_intValue },
                    MemberFlags.IsNumeric => new object[] { m_floatValue },
                    MemberFlags.IsEnum => new object[] { m_enumValues.GetValue(m_index) },
                    _ => new object[] { m_boolValue },
                }));
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
                    default:
                        m_boolValue = (bool)value;
                        break;
                }
            }
        }

        public virtual Type Type => typeof(ValueCheat);

        public IEditorCheat Build(CheatData data) => new FieldCEditorCheat(data as ValueCheatData);
    }
}