using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MineSweeperRipeoff
{
    public class LinkHandler : MonoBehaviour, IPointerClickHandler
    {
        [Serializable]
        public class LinkSelectionEvent : UnityEvent<string, string, int> { }

        private TextMeshProUGUI m_TextMeshPro;
        private int m_selectedLink = -1;

        private Canvas m_Canvas;
        private Camera m_Camera;

        public LinkSelectionEvent onLinkSelection
        {
            get { return m_OnLinkSelection; }
            set { m_OnLinkSelection = value; }
        }
        [SerializeField]
        private LinkSelectionEvent m_OnLinkSelection = new LinkSelectionEvent();

        private void Awake()
        {
            m_TextMeshPro = gameObject.GetComponent<TextMeshProUGUI>();
            m_Canvas = gameObject.GetComponentInParent<Canvas>();
            
            if (m_Canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                m_Camera = null;
            else
                m_Camera = m_Canvas.worldCamera;
        }

        public void CheckLinkIntersection()
        {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(m_TextMeshPro, Input.mousePosition, m_Camera);

            // Handle new Link selection.
            if (linkIndex != -1 && linkIndex != m_selectedLink)
            {
                m_selectedLink = linkIndex;

                // Get information about the link.
                TMP_LinkInfo linkInfo = m_TextMeshPro.textInfo.linkInfo[linkIndex];

                // Debug.Log("Link ID: \"" + linkInfo.GetLinkID() + "\"   Link Text: \"" + linkInfo.GetLinkText() + "\""); // Example of how to retrieve the Link ID and Link Text.

                // Send the event to any listeners.
                SendOnLinkSelection(linkInfo.GetLinkID(), linkInfo.GetLinkText(), linkIndex);
            }
        }

        private void SendOnLinkSelection(string linkID, string linkText, int linkIndex)
        {
            if (onLinkSelection != null)
                onLinkSelection.Invoke(linkID, linkText, linkIndex);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            CheckLinkIntersection();
        }
    }
}
