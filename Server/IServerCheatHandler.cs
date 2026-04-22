using System;

namespace cookie.Cheats.Server
{
    public interface IServerCheatHandler
    {
        Type CheatType { get; }
        void Handle(ICheat cheat, CheatPayload payload);
    }
}