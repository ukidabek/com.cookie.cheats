using System;

namespace cookie.Cheats
{
    [Serializable]
    public class CheatData
    {
        public int ID;
        public string Name;
        public CheatAttributeData[] Attributes;
        public string AssemblyQualifiedName;
    }
}