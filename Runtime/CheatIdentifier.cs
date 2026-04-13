using System;

namespace cookie.Cheats
{
    [Serializable]
    public class CheatIdentifier
    {
        public int ID { get; }
        public string Name { get; }
        public CheatData[] Attributes { get; }
        public string AssemblyQualifiedName { get; }

        public CheatIdentifier(ICheat cheat)
        {
            ID = cheat.ID;
            Name = cheat.Name;
            Attributes = cheat.Attributes;
            AssemblyQualifiedName = cheat.GetType().AssemblyQualifiedName;
        }
    }
}