using System.Collections.Generic;

namespace cookie.Cheats
{
    public interface ICheat
    {
        int ID { get; }
        object Target { get; }
        string Name { get; }
        public IReadOnlyList<CheatAttribute> Attributes { get; }
    }
}