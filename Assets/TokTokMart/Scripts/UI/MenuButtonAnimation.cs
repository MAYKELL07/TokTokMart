using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace TokTokMart
{
    // Targets and pivots are authored by ReleasePolishTools, never generated during play.
    public sealed class MenuButtonAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISubmitHandler
    {
        public RectTransform target;
        [HideInInspector] public bool centeredLayout;
        bool hovered, pressed;
        float scale = 1, velocity, pop;
        Button button;
        void Awake() { button = GetComponent<Button>(); }
        void OnEnable() { hovered = pressed = false; scale = 1; velocity = pop = 0; if (target) target.localScale = Vector3.one; }
        void OnDisable() { hovered = pressed = false; if (target) target.localScale = Vector3.one; }
        public void OnPointerEnter(PointerEventData e) { hovered = true; }
        public void OnPointerExit(PointerEventData e) { hovered = pressed = false; }
        public void OnPointerDown(PointerEventData e) { if (button && button.IsInteractable()) { pressed = true; DeviceVibration.Pulse(12); } }
        public void OnPointerUp(PointerEventData e) { if (pressed) pop = .12f; pressed = false; }
        public void OnSubmit(BaseEventData e) { if (button && button.IsInteractable()) { pop = .14f; DeviceVibration.Pulse(15); } }
        void Update()
        {
            if (!target) return;
            bool active = button && button.IsInteractable();
            float wanted = !active ? 1 : pressed ? .93f : hovered ? 1.045f : 1;
            pop = Mathf.MoveTowards(pop, 0, Time.unscaledDeltaTime * .75f);
            scale = Mathf.SmoothDamp(scale, wanted + pop, ref velocity, .055f, Mathf.Infinity, Time.unscaledDeltaTime);
            target.localScale = Vector3.one * scale;
        }
    }
}
