using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GokilKisah
{
    sealed class RestockPanel : MonoBehaviour
    {
        public const int PackCount = 3;
        static readonly Color Navy = new Color32(24, 47, 58, 255);
        static readonly Color Cream = new Color32(255, 248, 224, 255);
        static readonly Color Teal = new Color32(34, 106, 98, 255);
        static readonly Color Empty = new Color32(13, 48, 50, 255);

        readonly RestockDragItem[] packs = new RestockDragItem[PackCount];
        readonly Image[] slots = new Image[PackCount];
        readonly bool[] filled = new bool[PackCount];
        readonly RawImage[] placed = new RawImage[PackCount];
        RectTransform design;
        Text counter, instruction;
        RestockDragItem dragging;
        Func<bool> placePack;

        public static RestockPanel Create(Transform parent, Font font, Func<bool> place, Action cancel)
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
            design = Node(transform, "Restock design", 0, 0, 1600, 900);
            design.anchorMin = design.anchorMax = new Vector2(.5f, .5f);
            design.anchoredPosition = new Vector2(-800, 450);
            Surface(design, "Panel shadow", 326, 151, 955, 649, Navy);
            Surface(design, "Restock board", 317, 138, 956, 650, Cream);
            Surface(design, "Restock header", 317, 138, 956, 105, Teal);
            Picture(design, "Crate", GameArtLibrary.UI("StorageBox01"), 349, 156, 64, 69);
            Label(design, font, "RESTOCK SHELF", 436, 151, 460, 69, 42, Cream, TextAnchor.MiddleLeft);
            counter = Label(design, font, "", 1030, 160, 202, 60, 30, Cream);
            instruction = Label(design, font, "", 360, 257, 870, 48, 26, Navy);

            Surface(design, "Shelf", 388, 326, 824, 191, Teal);
            Surface(design, "Shelf lip", 388, 491, 824, 26, new Color32(21, 76, 73, 255));
            Surface(design, "Crate tray", 388, 551, 824, 124, new Color32(211, 172, 114, 255));
            for (int i = 0; i < PackCount; i++)
            {
                float x = 451 + 240 * i;
                slots[i] = Surface(design, "Drop slot " + (i + 1), x, 344, 173, 133, Empty);
                Label(slots[i].transform, font, "+", 0, 0, 173, 133, 45, new Color32(135, 200, 181, 255));
                placed[i] = Picture(slots[i].transform, "Placed stock", null, 29, 13, 115, 107);
                var grab = Surface(design, "Drag pack " + (i + 1), x, 555, 173, 116, new Color32(244, 208, 154, 255));
                var art = Picture(grab.transform, "Pack artwork", null, 27, 4, 119, 108);
                packs[i] = grab.gameObject.AddComponent<RestockDragItem>();
                packs[i].Bind(this, grab.rectTransform, art);
            }
            var button = Surface(design, "Cancel restocking", 647, 711, 306, 51, Navy);
            button.gameObject.AddComponent<Button>().onClick.AddListener(() => cancel());
            Label(button.transform, font, "CANCEL", 0, 0, 306, 51, 22, Cream);
        }

        public void Open(int shelf, int firstSlot, int count, bool allowDrag)
        {
            gameObject.SetActive(true);
            dragging = null;
            for (int i = 0; i < PackCount; i++)
            {
                filled[i] = false;
                slots[i].color = Empty;
                slots[i].gameObject.SetActive(i < count);
                placed[i].gameObject.SetActive(false);
                var product = ReleaseShopArt.Product(ReleaseShopArt.ShelfProduct(shelf, firstSlot + i));
                packs[i].ResetPack(i < count, product ? product.texture : null, allowDrag);
            }
            SetProgress(0, count);
            instruction.text = "Drag the stock from the crate to the shelf.";
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
            for (int i = 0; i < PackCount; i++)
            {
                if (filled[i] || !slots[i].gameObject.activeSelf ||
                    !RectTransformUtility.RectangleContainsScreenPoint(slots[i].rectTransform, screen, eventCamera)) continue;
                if (!placePack()) return;
                filled[i] = true;
                slots[i].color = Teal;
                SetArt(placed[i], item.Artwork);
                placed[i].gameObject.SetActive(true);
                item.gameObject.SetActive(false);
                return;
            }
            instruction.text = "Drop the stock into an empty shelf slot.";
        }

        internal RectTransform Design => design;

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

        internal void Bind(RestockPanel panel, RectTransform view, RawImage artwork)
        {
            owner = panel;
            rect = view;
            art = artwork;
            home = view.anchoredPosition;
            hitArea = GetComponent<Image>();
            trayColor = hitArea.color;
        }

        internal void ResetPack(bool visible, Texture texture, bool allowDrag)
        {
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
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(owner.Design, eventData.position,
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
        int packsToPlace, restockFirstSlot;

        void PrepareRestock()
        {
            restockFirstSlot = shelfStock[skillTarget];
            packsToPlace = Mathf.Clamp(ShelfCapacity(skillTarget) - restockFirstSlot, 1, RestockPanel.PackCount);
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
                restockPanel = RestockPanel.Create(transform, boldFont, PlaceRestockPack, CancelCurrentTask);
                if (!restockViewNeedsReset) PrepareRestock();
            }
            if (restockViewNeedsReset)
            {
                restockPanel.Open(skillTarget, restockFirstSlot, packsToPlace, !replay);
                restockViewNeedsReset = false;
            }
            else if (!restockPanel.gameObject.activeSelf) restockPanel.Resume();
            restockPanel.SetProgress(taskCombo, packsToPlace);
        }

        bool PlaceRestockPack()
        {
            if (skillType != 0 || !carryingCrate) return false;
            PlaySound("Stock");
            taskCombo++;
            skillScans = taskCombo;
            if (taskCombo < packsToPlace) return true;

            shelfStock[skillTarget] = ShelfCapacity(skillTarget);
            carryingCrate = false;
            stockActions++;
            SetStep("03 / CHECKOUT", "Add up the customer's receipt at the till.");
            FinishStoreTask("Shelf restocked");
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
