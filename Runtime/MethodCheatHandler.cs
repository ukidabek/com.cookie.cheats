using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Scripting;

namespace cookie.Cheats.UI
{
    public class MethodCheatHandler : CheatHandler<MethodCheat>
    {
        [SerializeField] private CheatButton m_cheatButtonPrefab = null;
        [SerializeField] private Transform m_parent = null;

        protected override void Awake()
        {
            base.Awake();
            m_cheatButtonPrefab.gameObject.SetActive(false);
        }

        public override void Initialize(ICheat cheat)
        {
            base.Initialize(cheat);
            foreach (var attribute in m_cheat.Attributes)
            {
                var button = Instantiate(m_cheatButtonPrefab, m_parent);
                var name = string.IsNullOrEmpty(attribute.Name) ? m_cheat.Name : attribute.Name;
                button.gameObject.SetActive(true);
                button.Initialize(name, m_cheat.Target, m_cheat.MemberInfo, attribute.Parameters);
            }
        }
    }
}

namespace cookie.Cheats.Server
{
    [Preserve]
    public class MethodCheatHandler : ICheatHandler
    {
        public Type CheatType => typeof(MethodCheat);
        public void Handle(ICheat cheat, CheatPayload payload)
        {
            var fieldCheat = (MethodCheat)cheat;
            fieldCheat.Invoke(payload.Parameters);
        }

        public Task Update(ICheat cheat, Socket socket)
        {
            throw new NotImplementedException();
        }
    }
}