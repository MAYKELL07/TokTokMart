using System;
using UnityEngine;
using UnityEngine.UI;

namespace GokilKisah
{
    sealed class PunchCardUI : MonoBehaviour
    {
        static readonly Color Navy = new Color32(17, 39, 47, 255);
        static readonly Color Teal = new Color32(31, 97, 95, 255);
        static readonly Color Cream = new Color32(255, 245, 213, 255);
        static readonly Color Gold = new Color32(255, 190, 55, 255);
        static readonly Color Coral = new Color32(167, 52, 76, 255);
        static readonly Color Muted = new Color32(181, 197, 193, 255);
        static readonly Color32 TileMuted = new Color32(42, 67, 72, 255);
        static readonly Color32 ResultRed = new Color32(199, 0, 0, 255);
        static readonly Color32 ResultCell = new Color32(229, 222, 194, 184);

        sealed class ShiftTile
        {
            public Image background;
            public Text title, status;
            public readonly RawImage[] stars = new RawImage[3];
            public Button button;
        }

        readonly ShiftTile[] tiles = new ShiftTile[20];
        readonly Text[] taskTotals = new Text[4];
        readonly Text[] starThresholds = new Text[3];
        readonly RawImage[] resultStars = new RawImage[3];
        Font display, body;
        RectTransform design, map, results, loading, loadingFill, mapCard;
        Text pageText, shiftLabel, shiftTitle, shiftHint, shiftTargets, shiftTime, punchLabel;
        Text resultHeading, primaryLabel, resultFailure, resultTimeLabel, resultTime;
        RawImage cardArt;
        Button previous, next, punch;
        CanvasGroup loadingGroup;
        float resultsOpened;

        public static PunchCardUI Create(Transform parent, Font titleFont, Font bodyFont, Action<int> select,
            Action<int> turnPage, Action start, Action back, Action<int> result)
        {
            var go = new GameObject("Punch Card UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(parent, false);
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 130;
            canvas.vertexColorAlwaysGammaSpace = true;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

            var view = go.AddComponent<PunchCardUI>();
            view.display = titleFont;
            view.body = bodyFont;
            view.design = Node(go.transform, "1600 × 900 design", 0, 0, 1600, 900);
            view.design.anchorMin = view.design.anchorMax = new Vector2(.5f, .5f);
            view.design.anchoredPosition = new Vector2(-800, 450);

            view.BuildMap(select, turnPage, start, back);
            view.BuildResults(result);
            view.BuildLoading();
            view.Show(false, false, -1);
            return view;
        }

        void BuildMap(Action<int> select, Action<int> turnPage, Action start, Action back)
        {
            map = Node(design, "Shift select", 0, 0, 1600, 900);
            Surface(map, Navy);
            DrawArt(map, "MenuBackground", 0, 0, 1600, 900, new Color(.18f, .30f, .32f));
            Box(map, "Grid panel", 53, 159, 892, 602, Navy);
            Write(map, "CHOOSE YOUR SHIFT", 76, 43, 1000, 92, 64, Cream, TextAnchor.MiddleLeft);
            AddButton(map, "MAIN MENU", 1234, 59, 300, 58, back);
            for (int i = 0; i < tiles.Length; i++)
            {
                int index = i;
                var slot = Node(map, "Shift slot " + (i + 1), 74 + i % 5 * 173, 182 + i / 5 * 142, 158, 121);
                var tile = new ShiftTile();
                tile.background = slot.gameObject.AddComponent<Image>();
                tile.button = MakeButton(slot, () => select(index));
                tile.title = Write(slot, "", 8, 6, 142, 48, 29, Navy);
                tile.status = Write(slot, "", 8, 91, 142, 25, 17, Navy, font: body);
                for (int star = 0; star < 3; star++)
                    tile.stars[star] = DrawArt(slot, "Star", 31 + star * 34, 58, 28, 27, Color.white);
                tiles[i] = tile;
            }

            var clock = DrawArt(map, "PunchClock", 1060, 148, 419, 143, Color.white);
            clock.uvRect = new Rect(1, 1, -1, -1);
            mapCard = Node(map, "Selected punch card", 1059, 238, 420, 575);
            cardArt = DrawArt(mapCard, "PunchCardNormal", 0, 0, 420, 575, Color.white);
            shiftLabel = Write(mapCard, "", 18, 12, 384, 67, 42, Navy);
            shiftTitle = Write(mapCard, "", 18, 84, 384, 50, 25, Teal);
            shiftHint = Write(mapCard, "", 34, 145, 352, 97, 24, Navy, font: body);
            shiftTargets = Write(mapCard, "", 72, 257, 292, 124, 23, Navy, TextAnchor.MiddleLeft, body);
            DrawArt(mapCard, "CheckoutIcon01", 32, 262, 26, 26, Color.white);
            DrawArt(mapCard, "StockIcon01", 32, 307, 26, 24, Color.white);
            DrawArt(mapCard, "PuddleIcon01", 36, 346, 19, 27, Color.white);
            shiftTime = Write(mapCard, "", 18, 398, 384, 49, 24, Teal);
            punch = AddButton(mapCard, "", 24, 470, 372, 69, start, "PunchSelectButton");
            punchLabel = punch.GetComponentInChildren<Text>();

            previous = AddButton(map, "PREVIOUS", 74, 788, 202, 57, () => turnPage(-1));
            pageText = Write(map, "", 302, 788, 390, 57, 24, Cream);
            next = AddButton(map, "NEXT", 720, 788, 202, 57, () => turnPage(1));
        }

        void BuildResults(Action<int> action)
        {
            results = Node(design, "Shift results", 0, 0, 1600, 900);
            Surface(results, new Color32(12, 47, 55, 255));
            var report = Node(results, "Stamped result card", 255, 22, 1090, 768);
            DrawArt(report, "PunchResultCard", 0, 0, 1090, 768, new Color32(252, 245, 237, 255));
            resultHeading = Write(report, "", 130, 16, 890, 95, 62, Navy);
            var rating = Node(report, "Star rating", 298, 128, 532, 138);
            for (int i = 0; i < resultStars.Length; i++)
            {
                int x = i * 187;
                var star = DrawArt(rating, "Star", x, 0, 158, 138, Color.white);
                star.rectTransform.pivot = new Vector2(.5f, .5f);
                star.rectTransform.anchoredPosition += new Vector2(79, -69);
                resultStars[i] = star;
                starThresholds[i] = Write(rating, "", x + 17, 34, 124, 87, 37, Navy);
                starThresholds[i].lineSpacing = .87f;
            }
            resultFailure = Write(report, "", 110, 267, 930, 30, 21, Navy, font: body);

            // Align the cells with the dividers in the artist's card texture.
            string[] taskNames = { "RESTOCK", "CLEAN", "HAZARD", "CHECKOUT" };
            string[] icons = { "StockIcon01", "PuddleIcon01", "HazardIcon01", "CoinJarIcon" };
            Rect[] iconRects =
            {
                new Rect(58, 13, 112, 103),
                new Rect(82, 14, 81, 105),
                new Rect(60, 16, 110, 105),
                new Rect(52, -34, 134, 181)
            };
            for (int i = 0; i < taskTotals.Length; i++)
            {
                var cell = Node(report, taskNames[i], 217 + i % 2 * 351, 306 + i / 2 * 175, 345, 169);
                Box(cell, "Cell background", 0, 0, 345, 169, ResultCell);
                var icon = iconRects[i];
                DrawArt(cell, icons[i], icon.x, icon.y, icon.width, icon.height, Color.white);
                taskTotals[i] = Write(cell, "", 174, 24, 125, 97, 84, ResultRed);
                Write(cell, taskNames[i], 0, 118, 338, 53, 35, Navy);
            }

            Box(report, "Time strip", 217, 653, 695, 69, ResultCell);
            resultTimeLabel = Write(report, "", 234, 657, 400, 60, 34, Navy, TextAnchor.MiddleRight);
            DrawArt(report, "TimerIcon01", 650, 670, 41, 41, Color.white);
            resultTime = Write(report, "", 700, 657, 155, 60, 36, ResultRed, TextAnchor.MiddleLeft);

            AddButton(results, "REPLAY", 226, 810, 285, 66, () => action(1), "PunchResultReplay");
            var primary = AddButton(results, "", 526, 799, 535, 88, () => action(0), "PunchResultPrimary");
            primaryLabel = primary.GetComponentInChildren<Text>();
            primaryLabel.fontSize = primaryLabel.resizeTextMaxSize = 32;
            AddButton(results, "LEVEL SELECT", 1075, 810, 306, 66, () => action(2), "PunchResultSecondary");
        }

        void BuildLoading()
        {
            loading = Node(design, "Shift transition", 0, 0, 1600, 900);
            Surface(loading, Navy);
            loadingGroup = loading.gameObject.AddComponent<CanvasGroup>();
            DrawArt(loading, "Logo", 572, 189, 456, 244, Color.white);
            Write(loading, "OPENING THE SHOP", 450, 470, 700, 64, 38, Cream);
            Box(loading, "Progress trough", 548, 569, 504, 16, Teal);
            loadingFill = Box(loading, "Progress", 550, 571, 500, 12, Gold).rectTransform;
        }

        public void Show(bool showMap, bool showResults, float progress)
        {
            map.gameObject.SetActive(showMap && progress < 0);
            bool visible = showResults && progress < 0;
            if (visible && !results.gameObject.activeSelf) resultsOpened = Time.unscaledTime;
            results.gameObject.SetActive(visible);
            if (visible)
            {
                for (int i = 0; i < resultStars.Length; i++)
                {
                    float t = Mathf.Clamp01((Time.unscaledTime - resultsOpened - .12f * i) * 5);
                    resultStars[i].rectTransform.localScale = Vector3.one * Mathf.SmoothStep(.1f, 1f, t);
                }
            }
            loading.gameObject.SetActive(progress >= 0);
            if (progress >= 0)
            {
                loadingFill.sizeDelta = new Vector2(500 * Mathf.Clamp01(progress * 1.25f), 12);
                loadingGroup.alpha = progress < .82f ? 1 : Mathf.Clamp01((1 - progress) / .18f);
            }
        }

        public void BindMap(int first, int selected, int unlocked, Func<int, int> stars, string hint, string boss)
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                int level = first + i;
                bool canPlay = level <= unlocked;
                var tile = tiles[i];
                tile.background.color = level == selected ? Gold : canPlay ? Cream : (Color)TileMuted;
                tile.button.interactable = canPlay;
                tile.title.color = canPlay ? Navy : Muted;
                tile.title.text = "SHIFT " + level.ToString("00");
                tile.status.text = !canPlay ? "LOCKED" : level % 5 == 0 ? "BOSS SHIFT" : "";
                tile.status.color = canPlay ? Coral : Muted;
                int earned = canPlay ? stars(level) : 0;
                for (int j = 0; j < tile.stars.Length; j++)
                    tile.stars[j].color = j < earned ? Color.white : new Color(.19f, .24f, .24f, .31f);
            }

            var target = new ShiftTargets(selected);
            cardArt.texture = GameArtLibrary.UI(selected % 5 == 0 ? "PunchCardBoss" : "PunchCardNormal");
            shiftLabel.text = "SHIFT " + selected.ToString("00");
            shiftTitle.text = target.Title;
            shiftHint.text = hint;
            shiftTargets.text = target.serve + " customers\n" + target.stock + " restock" + (target.stock == 1 ? "" : "s")
                + "\n" + target.clean + " spill" + (target.clean == 1 ? "" : "s");
            shiftTargets.lineSpacing = 1.42f;
            int seconds = Mathf.CeilToInt(target.duration);
            string closing = seconds / 60 + ":" + (seconds % 60).ToString("00");
            shiftTime.text = string.IsNullOrEmpty(boss) ? "CLOSES IN  " + closing : boss.ToUpperInvariant() + "  /  " + closing;
            punchLabel.text = "PUNCH IN";
            punch.interactable = selected <= unlocked;
            pageText.text = "SHIFTS " + first + " – " + (first + tiles.Length - 1);
            previous.interactable = first > 1;
            next.interactable = first + tiles.Length <= unlocked;
        }

        public void BindResults(int level, bool won, ShiftScore score, string failure,
            int stocked, int cleaned, int hazards, int served, float elapsed)
        {
            resultHeading.text = "SHIFT <color=#C70000><size=77>" + level.ToString("00")
                + "</size></color>" + (won ? " COMPLETE!" : " CLOSED");
            starThresholds[0].text = score.oneStar + "\nPTS";
            starThresholds[1].text = score.twoStars + "\nPTS";
            starThresholds[2].text = score.threeStars + "\nPTS";
            for (int i = 0; i < resultStars.Length; i++) resultStars[i].gameObject.SetActive(i < score.stars);
            taskTotals[0].text = stocked.ToString();
            taskTotals[1].text = cleaned.ToString();
            taskTotals[2].text = hazards.ToString();
            taskTotals[3].text = served.ToString();
            resultFailure.text = won ? "" : failure;
            resultTimeLabel.text = won ? "Complete shift in:" : "Time in shop:";
            int seconds = Mathf.Max(0, Mathf.FloorToInt(elapsed));
            resultTime.text = (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00");
            primaryLabel.text = won ? "NEXT SHIFT " + (level + 1) : "RETRY SHIFT " + level;
        }

        Button AddButton(Transform parent, string label, float x, float y, float w, float h, Action action, string art = null)
        {
            var holder = Node(parent, label.Length > 0 ? label : art, x, y, w, h);
            holder.gameObject.AddComponent<Image>().color = art == null ? Cream : Color.clear;
            if (art != null) DrawArt(holder, art, 0, 0, w, h, Color.white);
            Write(holder, label, 8, 0, w - 16, h, 25, Navy);
            return MakeButton(holder, action);
        }

        static Button MakeButton(RectTransform node, Action action)
        {
            var button = node.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.highlightedColor = new Color(1, .88f, .59f);
            colors.pressedColor = new Color(.89f, .82f, .65f);
            colors.disabledColor = new Color(.8f, .84f, .84f, .7f);
            button.colors = colors;
            button.targetGraphic = node.GetComponent<Image>();
            button.onClick.AddListener(() => action());
            return button;
        }

        Text Write(Transform parent, string value, float x, float y, float w, float h, int size, Color color,
            TextAnchor align = TextAnchor.MiddleCenter, Font font = null)
        {
            var text = Node(parent, "Text", x, y, w, h).gameObject.AddComponent<Text>();
            text.font = font ? font : display;
            text.fontSize = size;
            text.text = value;
            text.color = color;
            text.alignment = align;
            text.raycastTarget = false;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = size - Mathf.Min(7, size / 4);
            text.resizeTextMaxSize = size;
            return text;
        }

        static RawImage DrawArt(Transform parent, string art, float x, float y, float w, float h, Color tint)
        {
            var image = Node(parent, art, x, y, w, h).gameObject.AddComponent<RawImage>();
            image.texture = GameArtLibrary.UI(art);
            image.color = tint;
            image.raycastTarget = false;
            return image;
        }

        void Surface(RectTransform parent, Color color)
        {
            var surface = Box(parent, "Screen backdrop", 0, 0, 1600, 900, color);
            surface.rectTransform.anchorMin = Vector2.zero;
            surface.rectTransform.anchorMax = Vector2.one;
            surface.rectTransform.offsetMin = new Vector2(-1600, -900);
            surface.rectTransform.offsetMax = new Vector2(1600, 900);
            surface.raycastTarget = true;
        }

        static Image Box(Transform parent, string name, float x, float y, float w, float h, Color color)
        {
            var image = Node(parent, name, x, y, w, h).gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        static RectTransform Node(Transform parent, string name, float x, float y, float w, float h)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.up;
            rect.anchoredPosition = new Vector2(x, -y);
            rect.sizeDelta = new Vector2(w, h);
            return rect;
        }
    }
}
