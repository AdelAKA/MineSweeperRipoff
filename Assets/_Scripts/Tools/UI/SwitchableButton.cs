using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MineSweeperRipeoff
{
    public class SwitchableButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image ouline;

        public void Initialize(UnityAction action)
        {
            button.onClick.AddListener(action);
        }

        public void OnSelect()
        {
            ouline.enabled = true;
        }

        public void OnDeselect()
        {
            ouline.enabled = false;
        }
    }
}
