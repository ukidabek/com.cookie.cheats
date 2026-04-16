using System;
using cookie.Cheats.Server;
using UnityEditor;
using UnityEngine;

namespace cookie.Cheats
{
    public class MethodCheatBuilder : IEditorCheatBuilder
    {
        private class EditorMethodCheat : IEditorCheat
        {
            public int ID { get; }
            public string Name { get; }
            public CheatAttributeData[] Attributes { get; }

            public CheatData ToDataTransferObject() => null;

            public event Action<CheatPayload> Update;

            public void OnGUI()
            {
                EditorGUILayout.BeginHorizontal();
                foreach (var attribute in Attributes)
                {
                    var name = string.IsNullOrEmpty(attribute.Name) ? Name : attribute.Name;
                    if (GUILayout.Button(name)) 
                        Update?.Invoke(new CheatPayload(ID, attribute.Parameters));
                }

                EditorGUILayout.EndHorizontal();
            }

            public EditorMethodCheat(CheatData data)
            {
                ID = data.ID;
                Name = data.Name;
                Attributes = data.Attributes;
            }
        }
        
        public Type Type => typeof(MethodCheat);
        
        public IEditorCheat Build(CheatData data) => new EditorMethodCheat(data);
    }
}