using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MineSweeperRipeoff
{
    public class CreditsUi : MonoBehaviour
    {
        [SerializeField] Button closeButton;

        public TMP_TextEventHandler TextEventHandler;

        //private List<string> urls = new List<string>()
        //{
        //    "https://freesound.org/people/Connum/sounds/33262/",
        //    "https://freesound.org/"
        //};

        private Canvas _canvas;

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
        }

        private void Start()
        {
            closeButton.onClick.AddListener(Hide);
        }

        public void Show() => _canvas.enabled = true;
        public void Hide() => _canvas.enabled = false;

        private void OnEnable()
        {
            if (TextEventHandler != null)
            {
                TextEventHandler.onLinkSelection.AddListener(OnLinkSelection);
                Debug.Log(TextEventHandler.onLinkSelection.GetPersistentEventCount());
            }
        }

        private void OnDisable()
        {
            if (TextEventHandler != null)
            {
                TextEventHandler.onLinkSelection.RemoveListener(OnLinkSelection);
            }
        }

        void OnLinkSelection(string linkID, string linkText, int linkIndex)
        {
            Application.OpenURL(linkText);
            Debug.Log("Link Index: " + linkIndex + " with ID [" + linkID + "] and Text \"" + linkText + "\" has been selected.");
        }
    }
}
