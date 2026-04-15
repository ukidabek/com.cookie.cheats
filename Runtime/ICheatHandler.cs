using System;

namespace cookie.Cheats.Server
{
    public interface ICheatHandler
    {
        Type CheatType { get; }
        void Handle(ICheat cheat, CheatPayload payload);
    }
}