using System;

namespace cookie.Cheats.Server
{
    [Serializable]
    public class CheatPayload
    {
        public int ID;
        public object[] Parameters;
    }

    [Serializable]
    public class Message
    {
        public int ID;
        public object Payload;
    }
}