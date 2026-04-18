using System;
using System.Collections.Generic;

namespace cookie.Cheats.Server
{
    public class ItemProcessor<KeyT,ValueT>
    {
        private readonly IReadOnlyDictionary<KeyT, ValueT> m_dictionary;
        private readonly Queue<ValueT> m_queue = new Queue<ValueT>(30);
        private readonly Func<ValueT, bool> m_processItem;
        private readonly Func<ValueT, KeyT> m_getID;
        
        private readonly HashSet<KeyT> m_queuedIds = new HashSet<KeyT>(30);
        private int m_lastKnownCount = 0;
        
        public ItemProcessor(IReadOnlyDictionary<KeyT, ValueT> dictionary, Func<ValueT, bool> processItem, Func<ValueT, KeyT> getID)
        {
            m_dictionary = dictionary;
            SyncQueueIfDictionaryGrew();
            m_processItem = processItem;
            m_getID = getID;
        }

        public void Process()
        {
            SyncQueueIfDictionaryGrew();

            if (m_queue.Count == 0) return;

            var item = m_queue.Dequeue();
            
            if (!m_dictionary.ContainsKey(m_getID(item))) return;
            
            if (m_processItem(item)) return;
            
            m_queue.Enqueue(item);
        }

        private void SyncQueueIfDictionaryGrew()
        {
            var currentCount = m_dictionary.Count;

            if (currentCount <= m_lastKnownCount) return;
            
            m_queuedIds.Clear();
            foreach (var item in m_queue)
                m_queuedIds.Add(m_getID(item));
            
            foreach (var item in m_dictionary.Values)
            {
                if (m_queuedIds.Contains(m_getID(item))) continue;
                m_queue.Enqueue(item);
            }

            m_lastKnownCount = currentCount;
        }
    }
}