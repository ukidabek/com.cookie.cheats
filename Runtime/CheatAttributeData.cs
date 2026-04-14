using System;

namespace cookie.Cheats
{
    [Serializable]
    public class CheatAttributeData
    {
        public string Name;
        public float Min;
        public float Max;
        public object[] Parameters;

        public CheatAttributeData()
        {
        }

        public CheatAttributeData(CheatAttribute attribute)
        {
            Name = attribute.Name;
            Min = attribute.Min;
            Max = attribute.Max;
            Parameters = attribute.Parameters;
        }
    }
}