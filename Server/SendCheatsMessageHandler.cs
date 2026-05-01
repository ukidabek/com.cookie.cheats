using System.Linq;
using UnityEngine.Scripting;

namespace cookie.Cheats.Server
{
    [Preserve]
    public class SendCheatsMessageHandler : MessageHandler
    {
        public SendCheatsMessageHandler(CheatServer cheatServer) : base(cheatServer)
        {
        }

        public override int Id => CheatServer.SynchronizeCheats;
        
        public override Message Handle(object payload)
        {
            var cheats = m_cheatServer.Cheats
                .Select(cheat => cheat.ToDataTransferObject())
                .ToArray();

            return new Message()
            {
                ID = -1,
                Payload = cheats
            };
        }
    }
}