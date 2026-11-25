using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TokTokMart
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class CanvasBinding : MonoBehaviour
    {
        [HideInInspector] public string key;
        [HideInInspector] public Rect originalRect;
        [HideInInspector] public bool hasLayout;
        [HideInInspector] public string originalText;
        [HideInInspector] public Color originalColor;
        [HideInInspector] public int originalFontSize;
        [HideInInspector] public string sourceTextureName;
        [HideInInspector] public Rect originalUV;

        [Tooltip("Enable for counters, timers, feedback, and other values that change during gameplay.")]
        public bool bindGameData = true;
        [Tooltip("Enable for gauges, controls, and markers that move or resize during gameplay.")]
        public bool animateLayout = true;
        [Tooltip("Keep this element hidden in UI previews and during gameplay.")]
        public bool hideFromGame;

        AuthoredCanvas owner;
        RectTransform rect;
        Text text;
        TMP_Text tmp;
        RawImage rawImage;
        Image panel;

        Vector2 authoredPosition;
        Vector2 authoredSize;
        string authoredText;
        Color authoredColor;
        float authoredFontSize;
        Texture authoredTexture;
        string initialTextureName;
        Rect authoredUV;

        public void Connect(AuthoredCanvas canvas)
        {
            owner = canvas;
            rect = GetComponent<RectTransform>();
            authoredPosition = rect.anchoredPosition;
            authoredSize = rect.sizeDelta;
            text = GetComponent<Text>();
            tmp = GetComponent<TMP_Text>();
            rawImage = GetComponent<RawImage>();
            panel = GetComponent<Image>();

            if (text)
            {
                authoredText = text.text;
                authoredColor = text.color;
                authoredFontSize = text.fontSize;
            }
            else if (tmp)
            {
                authoredText = tmp.text;
                authoredColor = tmp.color;
                authoredFontSize = tmp.fontSize;
            }
            else if (rawImage)
            {
                authoredTexture = rawImage.texture;
                authoredColor = rawImage.color;
                authoredUV = rawImage.uvRect;
                initialTextureName = sourceTextureName;
            }
            else if (panel)
            {
                authoredColor = panel.color;
            }

            if (TryGetComponent<Button>(out var button))
            {
                button.onClick.RemoveListener(OnClick);
                button.onClick.AddListener(OnClick);
            }

            if (TryGetComponent<Slider>(out var slider))
            {
                slider.onValueChanged.RemoveListener(OnSlide);
                slider.onValueChanged.AddListener(OnSlide);
            }
        }

        public void Layout(Rect source)
        {
            if (!animateLayout || !hasLayout)
                return;

            var offset = source.position - originalRect.position;
            rect.anchoredPosition = authoredPosition + new Vector2(offset.x, -offset.y);
            rect.sizeDelta = authoredSize + source.size - originalRect.size;
        }

        public void SetText(string value, Color color, int size)
        {
            if (!bindGameData)
                return;

            string displayText = value == originalText ? authoredText : value;
            Color displayColor = DisplayColor(color);
            float displaySize = authoredFontSize * size / Mathf.Max(1, originalFontSize);
            if (text)
            {
                text.text = displayText;
                text.color = displayColor;
                text.fontSize = Mathf.Max(1, Mathf.RoundToInt(displaySize));
            }
            else if (tmp)
            {
                tmp.text = displayText;
                tmp.color = displayColor;
                tmp.fontSize = displaySize;
            }
        }

        public void SetImage(Texture texture, Rect uv, Color color)
        {
            if (!bindGameData)
                return;

            if (string.IsNullOrEmpty(initialTextureName))
                initialTextureName = texture.name;

            rawImage.texture = texture.name == initialTextureName && authoredTexture ? authoredTexture : texture;
            rawImage.uvRect = uv == originalUV ? authoredUV : uv;
            rawImage.color = DisplayColor(color);
        }

        public void SetPanel(Color color)
        {
            if (bindGameData)
                panel.color = DisplayColor(color);
        }

        Color DisplayColor(Color color) => color == originalColor ? authoredColor : AuthoredCanvas.GuiColor(color);

        void OnClick() => owner.Click(key);
        void OnSlide(float value) => owner.Slide(key, value);
    }
}
