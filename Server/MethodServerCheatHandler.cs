using System;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace cookie.Cheats.Server
{
    [Preserve]
    public class MethodServerCheatHandler : IServerCheatHandler
    {
        public IReadOnlyList<Type> CheatType { get; } = new[]
        {
            typeof(MethodCheat),
        };

        public void Handle(ICheat cheat, CheatPayload payload)
        {
            var fieldCheat = (MethodCheat)cheat;
            fieldCheat.Invoke(payload.Parameters);
        }
    }
}