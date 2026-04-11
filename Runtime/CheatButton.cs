using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace cookie.Cheats
{
    public class CheatButton : UIBehaviour
    {
        [SerializeField] private TMP_Text m_text;

        private object m_target;
        private MethodInfo m_methodInfo;
        private object[] m_parameters;

        public void Initialize(string text, object target, MethodInfo methodInfo, object[] parameters)
        {
            m_text.text = text;
            m_target = target;
            m_methodInfo = methodInfo;
            m_parameters = parameters;
        }

        public void InvokeMethod() => m_methodInfo.Invoke(m_target, m_parameters);
    }
}