using UnityEngine.Scripting;

namespace cookie.Cheats.Server
{
    [Preserve]
    public class CheatInvokeMessageHandler : MessageHandler<CheatPayload>
    {
        public CheatInvokeMessageHandler(CheatServer cheatServer) : base(cheatServer)
        {
        }
        
        public override int Id => CheatServer.SetPayload;
        protected override Message Handle(CheatPayload payload)
        {
            m_cheatServer.NewMethod(payload);
            return null;
        }
    }
}