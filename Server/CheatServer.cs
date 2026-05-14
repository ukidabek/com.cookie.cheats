using System;
using System.Collections.Generic;
using System.Linq;
using cookie.Cheats.Network;
using UnityEngine;

namespace cookie.Cheats.Server
{
    public class CheatServer : MonoBehaviour
    {
        [SerializeField] private int m_discoverPort = 2137;
        [SerializeField] private int m_listenPort = 2138;
        [SerializeField, Min(1)] private int m_connectionCount = 1;
        
        private Dictionary<Type, IServerCheatHandler> m_cheatChandlerDictionary;

        private ItemProcessor<int, ICheat> m_itemProcessor = null;

        public Network.Server Server { get; private set; } = null;
        
        private void Start()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsClass && !type.IsAbstract)
                .ToArray();

            var cheatHandlerType = typeof(IServerCheatHandler);
            m_cheatChandlerDictionary = types
                .Where(cheatHandlerType.IsAssignableFrom)
                .Select(type => (IServerCheatHandler)Activator.CreateInstance(type))
                .SelectMany(handler => handler.CheatType.Select(type => (type, handler)))
                .ToDictionary(pair => pair.type, pair => pair.handler);
            
            m_itemProcessor = new ItemProcessor<int, ICheat>(CheatDatabase.Instance.ChetDictionary, 
                cheat =>
                {
                    if (cheat is not IValueCheat valueCheat) return false;
                    // if (!valueCheat.IsDirty) return false;

                    var message = new Message(MessagesIDs.UpdateCheat, new []
                    {
                        cheat.ID,
                        valueCheat.ToSerializableObject()
                    });
                    
                    Server.Send(message);

                    return false;
                },
                cheat => cheat.ID);

            Server = new Network.Server(name, m_discoverPort, m_listenPort, m_connectionCount);
            var cheats = CheatDatabase.Instance.ChetDictionary.Values
                .Select(cheat => cheat.ToDataTransferObject())
                .Select(data => new Message(MessagesIDs.CreateCheatInstance, data));
            Server.SetHelloMessages(cheats);
            Server.Start();
        }

        private void Update()
        {
            var queue = Server.ReceiveQueue;
            if (queue.Any() && queue.TryDequeue(out var message))
            {
                switch (message.ID)
                {
                    case MessagesIDs.UpdateCheat:
                        if (message.Payload is not CheatPayload cheatPayload || 
                            !m_itemProcessor.TryGetValue(cheatPayload.ID, out var cheat) || 
                            !m_cheatChandlerDictionary.TryGetValue(cheat.GetType(), out var cheatHandler))
                        {
                            break;
                        }
                        
                        cheatHandler.Handle(cheat, cheatPayload);
                        break;
                }
            }
            m_itemProcessor.Process();
        }

        private void OnDestroy() => Server?.Dispose();
    }
}