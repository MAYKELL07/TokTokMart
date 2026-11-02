using UnityEngine;
using UnityEngine.InputSystem;

namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        // Input System does not send runtime IMGUI input events. Poll the primary pointer for
        // menu buttons/sliders while the gameplay controls track all fingers independently.
        Vector2 uiPointerDown,uiPointerPosition;
        bool uiPointerHeld;
        int uiReleasedFrame=-1,uiClickConsumedFrame=-1;
        void TrackInterfacePointer(Vector2 position,bool down,bool held,bool released)
        {
            uiPointerPosition=position;
            if(down)uiPointerDown=position;
            uiPointerHeld=held;
            if(released)uiReleasedFrame=Time.frameCount;
        }
        void UpdateInterfacePointer()
        {
            var touch=Touchscreen.current?.primaryTouch;
            if(touch!=null&&(touch.press.isPressed||touch.press.wasReleasedThisFrame))
            {
                TrackInterfacePointer(ScreenToCanvas(touch.position.ReadValue()),touch.press.wasPressedThisFrame,touch.press.isPressed,touch.press.wasReleasedThisFrame);
                return;
            }
            var mouse=Mouse.current;
            if(mouse!=null)TrackInterfacePointer(ScreenToCanvas(mouse.position.ReadValue()),mouse.leftButton.wasPressedThisFrame,mouse.leftButton.isPressed,mouse.leftButton.wasReleasedThisFrame);
            else uiPointerHeld=false;
        }
        bool InterfaceButton(Rect rect)
        {
            bool native=GUI.Button(rect,GUIContent.none,GUIStyle.none);
            bool pointer=Event.current.type==EventType.Repaint&&uiReleasedFrame==Time.frameCount&&rect.Contains(uiPointerDown)&&rect.Contains(uiPointerPosition);
            if(!GUI.enabled||uiClickConsumedFrame==Time.frameCount||(!native&&!pointer))return false;
            uiClickConsumedFrame=Time.frameCount;return true;
        }
        float InterfaceSlider(Rect rect,float value)
        {
            if(GUI.enabled&&(uiPointerHeld||uiReleasedFrame==Time.frameCount)&&rect.Contains(uiPointerDown))
                return Mathf.Clamp01((uiPointerPosition.x-rect.x)/rect.width);
            return value;
        }
    }
}
