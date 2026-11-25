using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TokTokMart
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas))]
    public sealed class AuthoredCanvas : MonoBehaviour
    {
        public RectTransform designRoot;
        public AudioSource starAudio;

        readonly Dictionary<string, RawImage> controlImages = new();
        readonly Dictionary<string, CanvasBinding> elements = new();
        readonly Dictionary<string, int> occurrences = new();
        readonly HashSet<CanvasBinding> visible = new();
        readonly HashSet<string> missing = new();
        readonly Stack<string> scopes = new();
        readonly HashSet<string> clicks = new();
        readonly Dictionary<string, float> sliderChanges = new();

        Matrix4x4 screenToDesign;
        public bool IsPresenting { get; private set; }

        public static Color GuiColor(Color color)
        {
            color.a = 1 - Mathf.Pow(1 - Mathf.Clamp01(color.a), 2.2f);
            return color;
        }

        void Awake()
        {
            foreach (var element in designRoot.GetComponentsInChildren<CanvasBinding>(true))
            {
                if (!elements.TryAdd(element.key, element))
                {
                    ReportMissing("duplicate " + element.key);
                    continue;
                }

                element.Connect(this);
                if (element.key.StartsWith("DrawMobileControls_", StringComparison.Ordinal))
                {
                    var image = element.GetComponent<RawImage>();
                    if (image && image.texture)
                        controlImages[image.texture.name] = image;
                }
            }
        }

        public void Begin(Matrix4x4 matrix)
        {
            screenToDesign = matrix.inverse;
            visible.Clear();
            occurrences.Clear();
            scopes.Clear();
            scopes.Push("");
            IsPresenting = true;
        }

        public void End()
        {
            foreach (var element in elements.Values)
            {
                if (element)
                {
                    bool show = !element.hideFromGame && visible.Contains(element);
                    if (element.gameObject.activeSelf != show)
                        element.gameObject.SetActive(show);
                    if (!show)
                    {
                        clicks.Remove(element.key);
                        sliderChanges.Remove(element.key);
                    }
                }
            }

            IsPresenting = false;
        }

        void ReportMissing(string key)
        {
            if (missing.Add(key))
                Debug.LogWarning("Check the authored UI binding for " + key + ".", this);
        }

        string NextKey(string id)
        {
            string prefix = scopes.Peek();
            string stem = prefix.Length == 0 ? id : prefix + "/" + id;
            occurrences.TryGetValue(stem, out int count);
            occurrences[stem] = count + 1;
            return stem + "_" + count;
        }

        CanvasBinding Find(string key)
        {
            if (!elements.TryGetValue(key, out var element) || !element)
            {
                ReportMissing(key);
                return null;
            }

            element.gameObject.SetActive(!element.hideFromGame);
            visible.Add(element);
            return element;
        }

        public IDisposable Scope(string id, string title)
        {
            bool useRoot = id.StartsWith("DrawSettings", StringComparison.Ordinal);
            if (useRoot)
                scopes.Push("");

            string key = NextKey(id);
            Find(key);
            scopes.Push(key);
            return new ScopeLease(this, useRoot);
        }

        sealed class ScopeLease : IDisposable
        {
            AuthoredCanvas canvas;
            readonly bool useRoot;

            public ScopeLease(AuthoredCanvas canvas, bool useRoot)
            {
                this.canvas = canvas;
                this.useRoot = useRoot;
            }

            public void Dispose()
            {
                if (!canvas)
                    return;

                canvas.scopes.Pop();
                if (useRoot)
                    canvas.scopes.Pop();
                canvas = null;
            }
        }

        CanvasBinding Visual(string id, Rect rect, Matrix4x4 matrix)
        {
            var element = Find(NextKey(id));
            if (!element)
                return null;

            var transform = screenToDesign * matrix;
            var min = transform.MultiplyPoint3x4(rect.min);
            var max = transform.MultiplyPoint3x4(rect.max);
            element.Layout(Rect.MinMaxRect(min.x, min.y, max.x, max.y));
            return element;
        }

        public void Label(Rect rect, string value, GUIStyle style, Matrix4x4 matrix)
        {
            var element = Visual("Label", rect, matrix);
            if (!element)
                return;

            if (!element.TryGetComponent<Text>(out _) && !element.TryGetComponent<TMP_Text>(out _))
            {
                ReportMissing(element.key + "/Text or TMP");
                return;
            }

            int size = Mathf.RoundToInt(style.fontSize * (screenToDesign * matrix).GetColumn(0).magnitude);
            element.SetText(value, style.normal.textColor, size);
        }

        public void Texture(Rect rect, Texture texture, Rect uv, Color tint, Matrix4x4 matrix, ScaleMode mode = ScaleMode.StretchToFill)
        {
            if (!texture)
                return;

            float ratio = (float)texture.width / texture.height;
            float areaRatio = rect.width / rect.height;
            if (mode == ScaleMode.ScaleToFit && areaRatio > ratio)
            {
                float width = rect.height * ratio;
                rect.x += (rect.width - width) * .5f;
                rect.width = width;
            }
            else if (mode == ScaleMode.ScaleToFit)
            {
                float height = rect.width / ratio;
                rect.y += (rect.height - height) * .5f;
                rect.height = height;
            }
            else if (mode == ScaleMode.ScaleAndCrop && areaRatio > ratio)
            {
                float height = ratio / areaRatio;
                uv = new Rect(0, (1 - height) * .5f, 1, height);
            }
            else if (mode == ScaleMode.ScaleAndCrop)
            {
                float width = areaRatio / ratio;
                uv = new Rect((1 - width) * .5f, 0, width, 1);
            }

            var element = Visual("Image", rect, matrix);
            if (!element)
                return;

            if (!element.TryGetComponent<RawImage>(out var image))
            {
                ReportMissing(element.key + "/Raw Image");
                return;
            }

            element.SetImage(texture, uv, tint);
            if (element.key.StartsWith("DrawMobileControls_", StringComparison.Ordinal))
                controlImages[texture.name] = image;
        }

        public void Panel(Rect rect, Color tint, Matrix4x4 matrix)
        {
            var element = Visual("Panel", rect, matrix);
            if (element && element.TryGetComponent<Image>(out _))
                element.SetPanel(tint);
            else if (element)
                ReportMissing(element.key + "/Image");
        }

        public bool Button(Rect rect, bool enabled, Matrix4x4 matrix)
        {
            var element = Visual("Button", rect, matrix);
            if (!element)
                return false;

            if (!element.TryGetComponent<Button>(out var button))
            {
                ReportMissing(element.key + "/Button");
                return false;
            }

            button.interactable = enabled;
            bool clicked = clicks.Remove(element.key);
            return enabled && clicked;
        }

        public float Slider(Rect rect, float value, bool enabled, Matrix4x4 matrix)
        {
            var element = Visual("Slider", rect, matrix);
            if (!element)
                return value;

            if (!element.TryGetComponent<Slider>(out var slider))
            {
                ReportMissing(element.key + "/Slider");
                return value;
            }

            slider.interactable = enabled;
            if (sliderChanges.Remove(element.key, out var changed) && enabled)
                value = changed;
            slider.SetValueWithoutNotify(value);
            return value;
        }

        public RawImage ControlImage(string name) => controlImages.GetValueOrDefault(name);
        public void Click(string key) => clicks.Add(key);
        public void Slide(string key, float value) => sliderChanges[key] = value;
    }
}
