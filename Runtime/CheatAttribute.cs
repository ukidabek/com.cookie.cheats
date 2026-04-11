using System;

namespace cookie.Cheats
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Class,
        AllowMultiple = true)]
    public class CheatAttribute : Attribute
    {
        public string Name = string.Empty;
        public float Min = -10f;
        public float Max = 10f;
        public object[] Parameters = null;
    }
}