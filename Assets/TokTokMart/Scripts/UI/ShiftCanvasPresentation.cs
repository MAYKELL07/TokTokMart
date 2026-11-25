using System;
using System.Collections;
using UnityEngine;
namespace TokTokMart
{
    public sealed partial class ShiftController
    {
        [Header("Editable Canvas UI")]
        [Tooltip("Scene Canvas for editable artwork, layout, and text.")]
        public AuthoredCanvas editableUI;
        Vector2 nativeGroupOffset;
        bool NativeUiReady => editableUI && editableUI.IsPresenting && !drawingWorldOverlay && Event.current.type == EventType.Repaint;
        Matrix4x4 NativeMatrix => GUI.matrix * Matrix4x4.Translate(nativeGroupOffset);
        void EnsureEditableCanvas()
        {
            if (!editableUI) throw new InvalidOperationException("Assign the scene Canvas to Shift Controller.");
        }
        sealed class EmptyUiScope : IDisposable { public void Dispose() { } }
        static readonly IDisposable emptyUiScope = new EmptyUiScope();
        IDisposable NativeUiScope(string id, string title) => NativeUiReady ? editableUI.Scope(id, title) : emptyUiScope;
        void EditableLabel(Rect r, string value, GUIStyle style)
        {
            if (NativeUiReady) editableUI.Label(r, value, style, NativeMatrix);
            else if (drawingWorldOverlay) GUI.Label(r, value, style);
        }
        void EditableTexture(Rect r, Texture texture, ScaleMode mode = ScaleMode.StretchToFill, bool alpha = true)
        {
            if (NativeUiReady) editableUI.Texture(r, texture, new Rect(0, 0, 1, 1), GUI.color, NativeMatrix, mode);
            else if (drawingWorldOverlay) GUI.DrawTexture(r, texture, mode, alpha);
        }
        void EditableTextureUV(Rect r, Texture texture, Rect uv, bool alpha = true)
        {
            if (NativeUiReady) editableUI.Texture(r, texture, uv, GUI.color, NativeMatrix);
            else if (drawingWorldOverlay) GUI.DrawTextureWithTexCoords(r, texture, uv, alpha);
        }
    }
}
