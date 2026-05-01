using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace cookie.Cheats.UI
{
    public class Separator : UIBehaviour
    {
        [SerializeField] private TMP_Text m_label = null;
        public string Text
        {
            get => m_label.text;
            set => m_label.text = value;
        }
    }
}