using System;
using UnityEngine.Scripting;

namespace cookie.Cheats.Server
{
    [Preserve]
    public class MethodServerCheatHandler : IServerCheatHandler
    {
        public Type CheatType => typeof(MethodCheat);

        public void Handle(ICheat cheat, CheatPayload payload)
        {
            var fieldCheat = (MethodCheat)cheat;
            fieldCheat.Invoke(payload.Parameters);
        }
    }
}