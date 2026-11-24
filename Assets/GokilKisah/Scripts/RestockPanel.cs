using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GokilKisah
{
    sealed class RestockPanel : MonoBehaviour
    {
        public const int PacksPerRow = 5, MaxRows = 3, MaxPacks = PacksPerRow * MaxRows;
        const float ShelfTop = 141, ShelfRowHeight = 103, TrayRowHeight = 75;
        static readonly Color Navy = new Color32(24, 47, 58, 255);
        static readonly Color Cream = new Color32(255, 248, 224, 255);
        static readonly Color Teal = new Color32(34, 106, 98, 255);
        static readonly Color Empty = new Color32(13, 48, 50, 255);

        readonly RestockDragItem[] packs = new RestockDragItem[MaxPacks];
        readonly Image[] slots = new Image[MaxPacks];
        readonly Text[] slotHints = new Text[MaxPacks];
        readonly RawImage[] placed = new RawImage[MaxPacks];
        readonly Image[] shelfRows = new Image[MaxRows];
        readonly Image[] trayRows = new Image[MaxRows];
        RectTransform board, shadow, cancelButton;
        Text counter, instruction;
        RestockDragItem dragging;
        Func<int, int, bool> placePack;

        public static RestockPanel Create(Transform parent, Font font, Func<int, int, bool> place, Action cancel)
        {
            var obj = new GameObject("Restock minigame", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            obj.transform.SetParent(parent, false);
            var canvas = obj.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 120;
            canvas.vertexColorAlwaysGammaSpace = true;
            var scaler = obj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            var panel = obj.AddComponent<RestockPanel>();
            panel.placePack = place;
            panel.Build(font, cancel);
            obj.SetActive(false);
            return panel;
        }

        void Build(Font font, Action cancel)
        {
            var dim = Surface(transform, "Restock backdrop", 0, 0, 1600, 900, new Color(.03f, .09f, .10f, .78f));
            dim.rectTransform.anchorMin = Vector2.zero;
            dim.rectTransform.anchorMax = Vector2.one;
            dim.rectTransform.offsetMin = dim.rectTransform.offsetMax = Vector2.zero;
            var design = Node(transform, "Restock design", 0, 0, 1600, 900);
            design.anchorMin = design.anchorMax = new Vector2(.5f, .5f);
            design.anchoredPosition = new Vector2(-800, 450);
            shadow = Surface(design, "Panel shadow", 261, 0, 1096, 1, Navy).rectTransform;
            board = Surface(design, "Restock board", 252, 0, 1096, 1, Cream).rectTransform;
            Surface(board, "Restock header", 0, 0, 1096, 85, Teal);
            Picture(board, "Crate", GameArtLibrary.UI("StorageBox01"), 31, 10, 53, 64);
            Label(board, font, "RESTOCK SHELF", 106, 8, 510, 66, 40, Cream, TextAnchor.MiddleLeft);
            counter = Label(board, font, "", 865, 12, 195, 61, 30, Cream);
            instruction = Label(board, font, "", 40, 91, 1016, 42, 24, Navy);

            for (int row = 0; row < MaxRows; row++)
            {
                shelfRows[row] = Surface(board, "Shelf row " + (row + 1), 45, ShelfTop + row * ShelfRowHeight, 1006, 93, Teal);
                Surface(shelfRows[row].transform, "Shelf lip", 0, 85, 1006, 8, new Color32(21, 76, 73, 255));
                trayRows[row] = Surface(board, "Crate row " + (row + 1), 45, 0, 1006, 68, new Color32(211, 172, 114, 255));
            }
            for (int i = 0; i < MaxPacks; i++)
            {
                int row = i / PacksPerRow;
                float x = 61 + i % PacksPerRow * 200;
                slots[i] = Surface(board, "Drop slot " + (i + 1), x, ShelfTop + 5 + row * ShelfRowHeight, 178, 77, Empty);
                slotHints[i] = Label(slots[i].transform, font, "+", 0, 0, 178, 77, 34, new Color32(135, 200, 181, 255));
                placed[i] = Picture(slots[i].transform, "Placed stock", null, 32, 3, 114, 71);
                var grab = Surface(board, "Drag pack " + (i + 1), x, 0, 178, 62, new Color32(244, 208, 154, 255));
                var art = Picture(grab.transform, "Pack artwork", null, 32, 2, 114, 58);
                packs[i] = grab.gameObject.AddComponent<RestockDragItem>();
                packs[i].Bind(this, i, grab.rectTransform, art);
            }
            var button = Surface(board, "Cancel restocking", 395, 0, 306, 47, Navy);
            cancelButton = button.rectTransform;
            button.gameObject.AddComponent<Button>().onClick.AddListener(() => cancel());
            Label(button.transform, font, "CANCEL", 0, 0, 306, 47, 22, Cream);
        }

        public void Open(int shelf, int firstSlot, int rows, bool allowDrag)
        {
            gameObject.SetActive(true);
            dragging = null;
            int count = rows * PacksPerRow;
            float trayTop = ShelfTop + rows * ShelfRowHeight + 6;
            float cancelTop = trayTop + rows * TrayRowHeight + 7;
            float height = cancelTop + 63;
            float top = (900 - height) * .5f;
            board.anchoredPosition = new Vector2(252, -top);
            board.sizeDelta = new Vector2(1096, height);
            shadow.anchoredPosition = new Vector2(261, -top - 10);
            shadow.sizeDelta = board.sizeDelta;
            cancelButton.anchoredPosition = new Vector2(395, -cancelTop);
            for (int row = 0; row < MaxRows; row++)
            {
                shelfRows[row].gameObject.SetActive(row < rows);
                trayRows[row].gameObject.SetActive(row < rows);
                trayRows[row].rectTransform.anchoredPosition = new Vector2(45, -trayTop - row * TrayRowHeight);
            }
            for (int i = 0; i < MaxPacks; i++)
            {
                bool visible = i < count;
                slots[i].color = Empty;
                slots[i].gameObject.SetActive(visible);
                slotHints[i].gameObject.SetActive(true);
                placed[i].gameObject.SetActive(false);
                var product = visible ? ReleaseShopArt.Product(ReleaseShopArt.ShelfProduct(shelf, firstSlot + i)) : null;
                packs[i].ResetPack(visible, product ? product.texture : null, allowDrag,
                    new Vector2(61 + i % PacksPerRow * 200, -trayTop - 3 - i / PacksPerRow * TrayRowHeight));
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

        static RectTransform Node(Transform parent, string name, float x, float y, float width, float height)
        {
            var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            rt.anchorMin = rt.anchorMax = rt.pivot = Vector2.up;
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(width, height);
            return rt;
        }

        static Text Label(Transform parent, Font font, string content, float x, float y, float w, float h,
            int size, Color color, TextAnchor alignment = TextAnchor.MiddleCenter)
        {
            var label = Node(parent, content, x, y, w, h).gameObject.AddComponent<Text>();
            label.text = content;
            label.font = font;
            label.fontSize = size;
            label.color = color;
            label.alignment = alignment;
            label.raycastTarget = false;
            return label;
        }

        static Image Surface(Transform parent, string name, float x, float y, float w, float h, Color color)
        {
            var image = Node(parent, name, x, y, w, h).gameObject.AddComponent<Image>();
            image.color = color;
            return image;
        }

        static RawImage Picture(Transform parent, string name, Texture texture, float x, float y, float w, float h)
        {
            var frame = Node(parent, name, x, y, w, h);
            var image = Node(frame, "Texture", 0, 0, w, h).gameObject.AddComponent<RawImage>();
            image.rectTransform.anchorMin = image.rectTransform.anchorMax = image.rectTransform.pivot = new Vector2(.5f, .5f);
            image.rectTransform.anchoredPosition = Vector2.zero;
            image.raycastTarget = false;
            SetArt(image, texture);
            return image;
        }

        internal static void SetArt(RawImage image, Texture texture)
        {
            image.texture = texture;
            if (!texture) return;
            var frame = (RectTransform)image.transform.parent;
            float scale = Mathf.Min(frame.rect.width / texture.width, frame.rect.height / texture.height);
            image.rectTransform.sizeDelta = new Vector2(texture.width, texture.height) * scale;
        }
    }

    sealed class RestockDragItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        RestockPanel owner;
        RectTransform rect;
        RawImage art;
        Vector2 home;
        int? pointer;
        Image hitArea;
        Color trayColor;
        public Texture Artwork => art.texture;
        public int Index { get; private set; }

        internal void Bind(RestockPanel panel, int index, RectTransform view, RawImage artwork)
        {
            owner = panel;
            Index = index;
            rect = view;
            art = artwork;
            hitArea = GetComponent<Image>();
            trayColor = hitArea.color;
        }

        internal void ResetPack(bool visible, Texture texture, bool allowDrag, Vector2 homePosition)
        {
            home = homePosition;
            ResetDrag();
            RestockPanel.SetArt(art, texture);
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

    public sealed partial class StoreGameController
    {
        RestockPanel restockPanel;
        bool restockViewNeedsReset;
        readonly int[] restockPackBySlot = new int[RestockPanel.MaxPacks];
        int packsToPlace, restockFirstSlot, restockRows, usedRestockPacks;

        static int RestockRowsForShift(int shift) => shift >= 10 ? 3 : shift >= 5 ? 2 : 1;

        void PrepareRestock()
        {
            restockFirstSlot = shelfStock[skillTarget];
            restockRows = RestockRowsForShift(campaignActive ? CurrentLevel : 1);
            packsToPlace = restockRows * RestockPanel.PacksPerRow;
            usedRestockPacks = 0;
            Array.Fill(restockPackBySlot, -1);
            restockViewNeedsReset = true;
        }

        void RefreshRestockPanel()
        {
            bool visible = skillType == 0 && selected && !mainMenu && !settingsOpen && !autoPaused &&
                !levelResults && !shiftBriefOpen && !scoreGuideOpen && !gameOver;
            if (!visible)
            {
                if (restockPanel && restockPanel.gameObject.activeSelf) restockPanel.Close();
                return;
            }
            if (!restockPanel)
            {
                restockPanel = RestockPanel.Create(transform, boldFont, TryPlaceRestockPack, CancelCurrentTask);
                if (!restockViewNeedsReset) PrepareRestock();
            }
            if (restockViewNeedsReset)
            {
                restockPanel.Open(skillTarget, restockFirstSlot, restockRows, !replay);
                for (int slot = 0; slot < packsToPlace; slot++)
                    if (restockPackBySlot[slot] >= 0) restockPanel.ShowPlacement(restockPackBySlot[slot], slot);
                restockPanel.SetProgress(taskCombo, packsToPlace);
                restockViewNeedsReset = false;
            }
            else if (!restockPanel.gameObject.activeSelf) restockPanel.Resume();
        }

        // Scripted replays use the same validation as drag-and-drop.
        bool PlaceRestockPack()
        {
            int pack = 0, slot = 0;
            while (pack < packsToPlace && (usedRestockPacks & (1 << pack)) != 0) pack++;
            while (slot < packsToPlace && restockPackBySlot[slot] >= 0) slot++;
            return TryPlaceRestockPack(pack, slot);
        }

        bool TryPlaceRestockPack(int pack, int slot)
        {
            if (skillType != 0 || !carryingCrate || pack < 0 || pack >= packsToPlace ||
                slot < 0 || slot >= packsToPlace || restockPackBySlot[slot] >= 0 ||
                (usedRestockPacks & (1 << pack)) != 0) return false;

            usedRestockPacks |= 1 << pack;
            restockPackBySlot[slot] = pack;
            taskCombo++;
            skillScans = taskCombo;
            PlaySound("Stock");
            if (restockPanel && !restockViewNeedsReset)
            {
                restockPanel.ShowPlacement(pack, slot);
                restockPanel.SetProgress(taskCombo, packsToPlace);
            }
            if (taskCombo < packsToPlace) return true;

            shelfStock[skillTarget] = ShelfCapacity(skillTarget);
            carryingCrate = false;
            stockActions++;
            SetStep("03 / CHECKOUT", "Add up the customer's receipt at the till.");
            FinishStoreTask("Shelf restocked");
            if (restockPanel) restockPanel.Close();
            return true;
        }

        void CancelCurrentTask()
        {
            skillType = -1;
            taskCombo = 0;
            playerVelocity = Vector2.zero;
            Toast("Task cancelled");
        }

        void FinishStoreTask(string feedback)
        {
            skillType = -1;
            chaos = Mathf.Max(0, chaos - (campaignActive && Targets.focus == ShiftFocus.Chaos ? 12 : 6));
            Toast(feedback);
            MobileHaptics.Pulse(35);
            EnsureShiftCleanupWork();
        }
    }
}
