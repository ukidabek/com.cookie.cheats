using System;
using System.Linq;

namespace cookie.Cheats.Server
{
    public abstract class ValueServerCheatHandler<T> : IServerCheatHandler where T : ValueCheat
    {
        public Type CheatType => typeof(T);

        public void Handle(ICheat cheat, CheatPayload payload)
        {
            var fieldCheat = (T)cheat;
            var value = payload.Parameters.First();
            if (value is IProxy proxy)
                value = proxy.Parse();
            fieldCheat.Set(value);
        }
    }
    
}