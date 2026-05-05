using System;

namespace cookie.Cheats.Network
{
    [Serializable]
    public class Message
    {
        public int ID { get; set; }
        public object Payload { get; set; }

        public Message()
        {
        }
        
        public Message(int id, object payload)
        {
            ID = id;
            Payload = payload;
        }
    }
}