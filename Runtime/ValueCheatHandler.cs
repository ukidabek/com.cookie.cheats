using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace cookie.Cheats.UI
{
    public abstract class ValueCheatHandler<T> : CheatHandler
    {
        [SerializeField] private TMP_Text m_label = null;

        protected ValueCheat m_cheat = null;
        private static readonly Type ValueCheatType = typeof(T);
        public Type ValueType => m_cheat.ValueType;
        
        protected abstract UnityEvent<T> OnValueChanged { get; }

        public override bool CanHandle(ICheat cheat)
        {
            if (cheat is not ValueCheat valueCheat) return false;
            return valueCheat.ValueType == ValueCheatType;
        }

        public override void Initialize(ICheat cheat)
        {
            m_cheat = (ValueCheat)cheat;
            var attribute = cheat.Attributes.First();
            m_label.text = string.IsNullOrEmpty(attribute.Name) ? m_cheat.Name : attribute.Name;
            OnValueChanged.AddListener(UpdateValue);
        }
        
        protected virtual void UpdateValue(T value) => m_cheat.Set(value);
    }
}