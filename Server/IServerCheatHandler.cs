using System;
using System.Collections.Generic;

namespace cookie.Cheats.Server
{
    public interface IServerCheatHandler
    {
        IReadOnlyList<Type> CheatType { get; }
        void Handle(ICheat cheat, CheatPayload payload);
    }
}