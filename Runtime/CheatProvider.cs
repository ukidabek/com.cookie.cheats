using System.Collections.Generic;
using UnityEngine;

namespace cookie.Cheats
{
    public class CheatProvider : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour[] m_component;
        public IReadOnlyList<MonoBehaviour> Components => m_component;
        
        private void OnEnable() => CheatDatabase.Instance.Register(this);

        private void OnDisable() => CheatDatabase.Instance.Unregister(this);
    }
}