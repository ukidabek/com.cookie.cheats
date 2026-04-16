using System;
using System.Linq;
using cookie.Cheats.Server;
using UnityEditor;

namespace cookie.Cheats
{
    public class FieldCheatBuilder : IEditorCheatBuilder
    {
        private class EditorFieldCheat : IEditorCheat
        {
            public int ID { get; }
            public string Name { get; }
            public CheatAttributeData[] Attributes { get; }

            public CheatData ToDataTransferObject() => null;

            public event Action<CheatPayload> Update;

            private bool m_isNumericValue = false;
            private bool m_isWholeNumber = false;
            private float m_numericValue = 0;
            private bool m_booleValue = false;

            public void OnGUI()
            {
                EditorGUI.BeginChangeCheck();
                var attribute = Attributes.First();
                var name = string.IsNullOrEmpty(attribute.Name) ? Name : attribute.Name;

                if (m_isNumericValue)
                {
                    if (m_isWholeNumber)
                        m_numericValue = EditorGUILayout.IntSlider(name, (int)m_numericValue, (int)attribute.Min, (int)attribute.Max);
                    else
                        m_numericValue = EditorGUILayout.Slider(name, m_numericValue, attribute.Min, attribute.Max);
                }
                else
                    m_booleValue = EditorGUILayout.Toggle(Name, m_booleValue);

                if (EditorGUI.EndChangeCheck())
                    Update.Invoke(new CheatPayload(ID, new object[]
                    {
                        m_isNumericValue ? m_numericValue : m_booleValue
                    }));
            }

            public EditorFieldCheat(ValueCheatData data)
            {
                ID = data.ID;
                Name = data.Name;
                Attributes = data.Attributes;
                m_isNumericValue = data.IsNumeric;
                m_isWholeNumber = data.IsWholeNumber;
                
                var value = data.Value;
                if (m_isNumericValue)
                    m_numericValue = (float)Convert.ChangeType(value, typeof(float));
                else
                    m_booleValue = (bool)Convert.ChangeType(value, typeof(bool));
            }
        }

        public virtual Type Type => typeof(FieldCheat);

        public IEditorCheat Build(CheatData data)
        {
            return new EditorFieldCheat(data as ValueCheatData);
        }
    }
}