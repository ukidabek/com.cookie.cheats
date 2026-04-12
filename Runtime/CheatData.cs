using System;

namespace cookie.Cheats
{
    [Serializable]
    public class CheatData
    {
        public string Name;
        public float Min;
        public float Max;
        public object[] Parameters;

        public CheatData()
        {
        }

        public CheatData(CheatAttribute attribute)
        {
            Name = attribute.Name;
            Min = attribute.Min;
            Max = attribute.Max;
            Parameters = attribute.Parameters;
        }
    }
}