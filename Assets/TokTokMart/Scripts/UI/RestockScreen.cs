using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TokTokMart
{
    public sealed class RestockScreen : MonoBehaviour
    {
        public const int PacksPerRow = 5, MaxRows = 3, MaxPacks = PacksPerRow * MaxRows;
        const float ShelfTop = 141, ShelfRowHeight = 103, TrayRowHeight = 75;
        static readonly Color Navy = new Color32(24, 47, 58, 255);
        static readonly Color Cream = new Color32(255, 248, 224, 255);
        static readonly Color Teal = new Color32(34, 106, 98, 255);
        static readonly Color Empty = new Color32(13, 48, 50, 255);

        [SerializeField] RestockDragItem[] packs = new RestockDragItem[MaxPacks];
        [SerializeField] Image[] slots = new Image[MaxPacks];
        [SerializeField] Text[] slotHints = new Text[MaxPacks];
        [SerializeField] RawImage[] placed = new RawImage[MaxPacks];
        [SerializeField] Image[] shelfRows = new Image[MaxRows];
        [SerializeField] Image[] trayRows = new Image[MaxRows];
        [SerializeField] RectTransform board, shadow, cancelButton;
        [SerializeField] Button cancelControl;
        Vector2 boardPosition, boardSize, shadowPosition, shadowSize, cancelPosition;
        readonly Vector2[] trayPositions = new Vector2[MaxRows];
        [SerializeField] Text counter, instruction;
        RestockDragItem dragging;
        Func<int, int, bool> placePack;

        public void Connect(Func<int, int, bool> place, Action cancel)
        {
            placePack = place;
            cancelControl.onClick.AddListener(() => cancel());
            boardPosition = board.anchoredPosition;
            boardSize = board.sizeDelta;
            shadowPosition = shadow.anchoredPosition;
            shadowSize = shadow.sizeDelta;
            cancelPosition = cancelButton.anchoredPosition;
            for (int row = 0; row < MaxRows; row++)
                trayPositions[row] = trayRows[row].rectTransform.anchoredPosition;
            foreach (var pack in packs)
                pack.CaptureDesign();
        }

        public void Open(int shelf, int firstSlot, int rows, bool allowDrag)
        {
            gameObject.SetActive(true);
            dragging = null;
            int count = rows * PacksPerRow;
            float missingShelfHeight = (MaxRows - rows) * ShelfRowHeight;
            float missingHeight = missingShelfHeight + (MaxRows - rows) * TrayRowHeight;
            board.anchoredPosition = boardPosition - Vector2.up * (missingHeight / 2);
            board.sizeDelta = boardSize - Vector2.up * missingHeight;
            shadow.anchoredPosition = shadowPosition - Vector2.up * (missingHeight / 2);
            shadow.sizeDelta = shadowSize - Vector2.up * missingHeight;
            cancelButton.anchoredPosition = cancelPosition + Vector2.up * missingHeight;
            for (int row = 0; row < MaxRows; row++)
            {
                shelfRows[row].gameObject.SetActive(row < rows);
                trayRows[row].gameObject.SetActive(row < rows);
                trayRows[row].rectTransform.anchoredPosition = trayPositions[row] + Vector2.up * missingShelfHeight;
            }
            for (int i = 0; i < MaxPacks; i++)
            {
                bool visible = i < count;
                slots[i].color = Empty;
                slots[i].gameObject.SetActive(visible);
                slotHints[i].gameObject.SetActive(true);
                placed[i].gameObject.SetActive(false);
                var product = visible ? ShopProducts.Product(ShopProducts.ShelfProduct(shelf, firstSlot + i)) : null;
                packs[i].ResetPack(visible, product ? product.texture : null, allowDrag,
                    missingShelfHeight);
            }
            SetProgress(0, count);
            instruction.text = "Drag stock into the empty shelf slots.";
        }

        public void ShowPlacement(int pack, int slot)
        {
            slots[slot].color = Teal;
            slotHints[slot].gameObject.SetActive(false);
            SetArt(placed[slot], packs[pack].Artwork);
            placed[slot].gameObject.SetActive(true);
            packs[pack].gameObject.SetActive(false);
        }

        public void SetProgress(int done, int count) => counter.text = done + " / " + count;

        public void Resume() => gameObject.SetActive(true);

        public void Close()
        {
            if (dragging) dragging.ResetDrag();
            dragging = null;
            gameObject.SetActive(false);
        }

        internal bool BeginDrag(RestockDragItem item)
        {
            if (dragging) return false;
            dragging = item;
            return true;
        }

        internal void EndDrag(RestockDragItem item, Vector2 screen, Camera eventCamera)
        {
            if (dragging != item) return;
            dragging = null;
            for (int slot = 0; slot < MaxPacks; slot++)
            {
                if (slots[slot].gameObject.activeSelf &&
                    RectTransformUtility.RectangleContainsScreenPoint(slots[slot].rectTransform, screen, eventCamera) &&
                    placePack(item.Index, slot)) return;
            }
            instruction.text = "Drop the stock into an empty shelf slot.";
        }

        internal RectTransform DragPlane => board;

        internal static void SetArt(RawImage image, Texture texture)
        {
            image.texture = texture;
            if (!texture) return;
            var frame = (RectTransform)image.transform.parent;
            float scale = Mathf.Min(frame.rect.width / texture.width, frame.rect.height / texture.height);
            image.rectTransform.sizeDelta = new Vector2(texture.width, texture.height) * scale;
        }
    }
}
