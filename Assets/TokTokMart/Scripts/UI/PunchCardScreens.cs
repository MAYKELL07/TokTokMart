using System;
using UnityEngine;
using UnityEngine.UI;

namespace TokTokMart
{
    public sealed class PunchCardScreens : MonoBehaviour
    {
        public const string ResourcePath = "TokMartPunchCardUI";

        static readonly Color Navy = new Color32(17, 39, 47, 255);
        static readonly Color Teal = new Color32(31, 97, 95, 255);
        static readonly Color Cream = new Color32(255, 245, 213, 255);
        static readonly Color Gold = new Color32(255, 190, 55, 255);
        static readonly Color Coral = new Color32(167, 52, 76, 255);
        static readonly Color Muted = new Color32(181, 197, 193, 255);
        static readonly Color32 TileMuted = new Color32(42, 67, 72, 255);

        [Serializable]
        sealed class ShiftTile
        {
            public Image background;
            public Text title, status;
            public RawImage[] stars = new RawImage[3];
            public Button button;
        }

        [SerializeField] ShiftTile[] tiles = new ShiftTile[20];
        [SerializeField] Text[] taskTotals = new Text[4];
        [SerializeField] Text[] starThresholds = new Text[3];
        [SerializeField] RawImage[] resultStars = new RawImage[3];
        [SerializeField] RectTransform map, results, loading, loadingFill;
        [SerializeField] Text pageText, shiftLabel, shiftTitle, shiftHint, shiftTargets, shiftTime;
        [SerializeField] Text resultHeading, primaryLabel, resultFailure, resultTimeLabel, resultTime;
        [SerializeField] RawImage cardArt;
        [SerializeField] Button previous, next, punch, back, replay, primary, levelSelect;
        [SerializeField] CanvasGroup loadingGroup;
        readonly Vector3[] starScales = new Vector3[3];
        float loadingWidth;
        Texture regularCardArt;
        float resultsOpened;

        internal void Connect(Action<int> select, Action<int> turnPage, Action start, Action closeMap, Action<int> result)
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                int index = i;
                tiles[i].button.onClick.AddListener(() => select(index));
            }
            previous.onClick.AddListener(() => turnPage(-1));
            next.onClick.AddListener(() => turnPage(1));
            punch.onClick.AddListener(() => start());
            back.onClick.AddListener(() => closeMap());
            primary.onClick.AddListener(() => result(0));
            replay.onClick.AddListener(() => result(1));
            levelSelect.onClick.AddListener(() => result(2));

            regularCardArt = cardArt.texture;
            loadingWidth = loadingFill.rect.width;
            for (int i = 0; i < resultStars.Length; i++)
                starScales[i] = resultStars[i].rectTransform.localScale;
            Show(false, false, -1);
        }

#if UNITY_EDITOR
        public RectTransform PreviewScreen(int screen)
        {
            map.gameObject.SetActive(screen == 0);
            results.gameObject.SetActive(screen == 1);
            loading.gameObject.SetActive(screen == 2);
            return screen == 0 ? map : screen == 1 ? results : loading;
        }
#endif

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
                    resultStars[i].rectTransform.localScale = starScales[i] * Mathf.SmoothStep(.1f, 1f, t);
                }
            }
            loading.gameObject.SetActive(progress >= 0);
            if (progress >= 0)
            {
                loadingFill.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, loadingWidth * Mathf.Clamp01(progress * 1.25f));
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

            var target = new ShiftGoals(selected);
            cardArt.texture = selected % 5 == 0 ? ShopArtwork.UI("PunchCardBoss") : regularCardArt;
            shiftLabel.text = "SHIFT " + selected.ToString("00");
            shiftTitle.text = target.Title;
            shiftHint.text = hint;
            shiftTargets.text = target.serve + " customers\n" + target.stock + " restock" + (target.stock == 1 ? "" : "s")
                + "\n" + target.clean + " spill" + (target.clean == 1 ? "" : "s");
            int seconds = Mathf.CeilToInt(target.duration);
            string closing = seconds / 60 + ":" + (seconds % 60).ToString("00");
            shiftTime.text = string.IsNullOrEmpty(boss) ? "CLOSES IN  " + closing : boss.ToUpperInvariant() + "  /  " + closing;
            punch.interactable = selected <= unlocked;
            pageText.text = "SHIFTS " + first + " – " + (first + tiles.Length - 1);
            previous.interactable = first > 1;
            next.interactable = first + tiles.Length <= unlocked;
        }

        public void BindResults(int level, bool won, ShiftRating score, string failure,
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

    }
}
