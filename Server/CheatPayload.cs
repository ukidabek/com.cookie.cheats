using System;

namespace cookie.Cheats.Server
{
    [Serializable]
    public class CheatPayload
    {
        public readonly int ID;
        public readonly object[] Parameters;

        public CheatPayload(int id, object[] parameters)
        {
            ID = id;
            Parameters = parameters;
        }
    }

    [Serializable]
    public class Message
    {
        public readonly int ID;
        public readonly object Payload;
        
        public Message(int id, object payload)
        {
            ID = id;
            Payload = payload;
        }
    }
}