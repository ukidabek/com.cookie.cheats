using System;
using System.Collections.Generic;
using System.Linq;
using cookie.Cheats.Network;
using UnityEngine;

namespace cookie.Cheats.Server
{
    public static class MessagesIDs
    {
        public const int CreateCheatInstance = 0;
    }

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
                .Where(type => cheatHandlerType.IsAssignableFrom(type))
                .Select(type => (IServerCheatHandler)Activator.CreateInstance(type))
                .SelectMany(handler => handler.CheatType.Select(type => (type, handler)))
                .ToDictionary(pair => pair.type, pair => pair.handler);
            
            m_itemProcessor = new ItemProcessor<int, ICheat>(CheatDatabase.Instance.ChetDictionary, 
                cheat => false,
                cheat => cheat.ID);

            Server = new Network.Server();
            var cheats = CheatDatabase.Instance.ChetDictionary.Values
                .Select(cheat => cheat.ToDataTransferObject())
                .Select(data => new Message(MessagesIDs.CreateCheatInstance, data));
            Server.SetHelloMessages(cheats);
            Server.Start();
        }

        private void Update() => m_itemProcessor.Process();

        private void OnDestroy()
        {
        }
    }
}