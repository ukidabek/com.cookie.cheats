using System;
using System.Text;
using UnityEngine;

namespace cookie.Cheats.Network
{
    [Serializable]
    public class Message
    {
        public int ID { get; set; }
        public string PayloadAssemblyQualifiedName {get; set;}
        public byte[] Payload { get; set; }
        public Type PayloadType => Type.GetType(PayloadAssemblyQualifiedName);
        
        public Message(int id, object payload)
        {
            ID = id;
            PayloadAssemblyQualifiedName = payload.GetType().AssemblyQualifiedName;
            var json = JsonUtility.ToJson(payload);
            Payload = Encoding.UTF8.GetBytes(json);
        }

        public object GetPayload()
        {
            var json = Encoding.UTF8.GetString(Payload);
            var type = Type.GetType(PayloadAssemblyQualifiedName);
            var payload = JsonUtility.FromJson(json, type);
            return payload;
        }

        public T GetPayload<T>()
        {
            var type = typeof(T);
            if (type != PayloadType)
                throw new InvalidOperationException();
            return (T)GetPayload();
        }
    }
}