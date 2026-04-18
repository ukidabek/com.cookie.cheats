using System;

namespace cookie.Cheats
{
    [Serializable]
    public class ValueCheatData : CheatData
    {
        public MemberFlags MemberFlags;
        public string ValueAssemblyQualifiedName;
    }
}