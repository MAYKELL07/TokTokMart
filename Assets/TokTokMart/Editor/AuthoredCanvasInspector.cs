using UnityEditor;
using UnityEngine;
namespace TokTokMart.Editor
{
    [CustomEditor(typeof(AuthoredCanvas))]
    public class AuthoredCanvasInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var ui = (AuthoredCanvas)target;
            EditorGUILayout.HelpBox("Stop Play mode before editing. Open the layout editor to preview screens, select the menu fade and edit credits. Use the Rect tool (T) and Rect Transform for placement and size. Hide From Game keeps an element hidden during play.", MessageType.Info);
            if (GUILayout.Button("Open UI Layout Editor")) UILayoutEditor.Open(ui);
            if (!Application.isPlaying)
            {
                if (GUILayout.Button("Preview Main Menu")) UILayoutEditor.Preview(ui, "ArtMenu_0");
                if (GUILayout.Button("Preview Shift Briefing")) UILayoutEditor.Preview(ui, "ShiftBrief_0");
                if (GUILayout.Button("Preview Credits")) UILayoutEditor.Preview(ui, "DrawSettings5_0");
            }
            DrawDefaultInspector();
        }
    }
}
