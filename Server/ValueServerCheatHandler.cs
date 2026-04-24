using System;
using System.Collections.Generic;
using System.Linq;

namespace cookie.Cheats.Server
{
    public class ValueServerCheatHandler : IServerCheatHandler
    {
        public IReadOnlyList<Type> CheatType { get; } = new[]
        {
            typeof(ValueCheat),
            typeof(MultipleValueCheat)
        };

        public void Handle(ICheat cheat, CheatPayload payload)
        {
            var fieldCheat = (ValueCheat)cheat;
            var value = payload.Parameters.First();
            if (value is IProxy proxy)
                value = proxy.Parse();
            fieldCheat.Set(value);
        }
    }
    
}