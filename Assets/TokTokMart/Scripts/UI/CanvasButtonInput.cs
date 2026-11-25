using UnityEngine;

namespace TokTokMart
{
    public sealed partial class ShiftController
    {
        int clickHandledFrame = -1;

        bool InterfaceButton(Rect rect)
        {
            if (!NativeUiReady)
                return false;

            bool clicked = editableUI.Button(rect, GUI.enabled, NativeMatrix);
            if (!clicked || clickHandledFrame == Time.frameCount)
                return false;

            clickHandledFrame = Time.frameCount;
            return true;
        }
    }
}
