using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TokTokMart
{
    public sealed class RestockDragItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] RestockScreen owner;
        [SerializeField] RectTransform rect;
        [SerializeField] RawImage art;
        Vector2 home;
        int? pointer;
        [SerializeField] Image hitArea;
        Color trayColor;
        [SerializeField] int index;
        Vector2 authoredPosition;
        public Texture Artwork => art.texture;
        public int Index => index;

        internal void Bind(RestockScreen panel, int index, RectTransform view, RawImage artwork)
        {
            owner = panel;
            this.index = index;
            rect = view;
            art = artwork;
            hitArea = GetComponent<Image>();
            CaptureDesign();
        }

        internal void CaptureDesign()
        {
            trayColor = hitArea.color;
            authoredPosition = rect.anchoredPosition;
        }

        internal void ResetPack(bool visible, Texture texture, bool allowDrag, float verticalOffset)
        {
            home = authoredPosition + Vector2.up * verticalOffset;
            ResetDrag();
            RestockScreen.SetArt(art, texture);
            hitArea.raycastTarget = allowDrag;
            gameObject.SetActive(visible);
        }

        public void OnPointerDown(PointerEventData eventData) => CapturePointer(eventData);
        public void OnBeginDrag(PointerEventData eventData) => CapturePointer(eventData);

        void CapturePointer(PointerEventData eventData)
        {
            if (pointer.HasValue || !hitArea.raycastTarget ||
                eventData.button != PointerEventData.InputButton.Left || !owner.BeginDrag(this)) return;
            pointer = eventData.pointerId;
            hitArea.color = Color.clear;
            transform.SetAsLastSibling();
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (pointer != eventData.pointerId) return;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(owner.DragPlane, eventData.position,
                eventData.pressEventCamera, out var point))
                rect.anchoredPosition = point + new Vector2(-rect.rect.width * .5f, rect.rect.height * .5f);
        }

        public void OnPointerUp(PointerEventData eventData) => ReleasePointer(eventData);
        public void OnEndDrag(PointerEventData eventData) => ReleasePointer(eventData);

        void ReleasePointer(PointerEventData eventData)
        {
            if (pointer != eventData.pointerId) return;
            ResetDrag();
            owner.EndDrag(this, eventData.position, eventData.pressEventCamera);
        }

        internal void ResetDrag()
        {
            pointer = null;
            rect.anchoredPosition = home;
            hitArea.color = trayColor;
        }
    }
}
