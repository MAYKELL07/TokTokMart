using UnityEditor;
using UnityEngine;
using System.Linq;
namespace GokilKisah.Editor
{
    [CustomEditor(typeof(TokMartCanvas))]
    public class TokMartCanvasInspector:UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();var ui=(TokMartCanvas)target;
            EditorGUILayout.HelpBox("Edit outside Play Mode. Expand Design, preview a screen below, then move groups with the Rect tool. Edit Text, Image and RawImage components directly. Game counters still update during play.",MessageType.Info);
            if(Application.isPlaying)return;
            if(!ui.designRoot)return;
            EditorGUILayout.LabelField("Preview one screen",EditorStyles.boldLabel);
            var names=new[]{"Main menu","Choose a level","Scoring rules","Shift targets and star requirements","Shift score and stars","Shift HUD"};
            var screens=ui.GetComponentsInChildren<TokMartCanvasElement>(true).Where(e=>e.kind=="Group"&&(names.Contains(e.name)||e.transform.parent==ui.designRoot&&e.key.StartsWith("Draw"))).ToArray();
            foreach(var screen in screens)
            {
                string name=screen.name;
                if(screen.key.StartsWith("DrawSettings"))name+=" - "+new[]{"Audio","Display","Gameplay","Pitch tools","Controls","Credits"}[Mathf.Clamp(int.Parse(screen.key.Substring("DrawSettings".Length,1)),0,5)];
                if(GUILayout.Button(name))
                {
                    Undo.RegisterFullObjectHierarchyUndo(ui.gameObject,"Preview UI screen");
                    foreach(var e in ui.GetComponentsInChildren<TokMartCanvasElement>(true))e.gameObject.SetActive(false);
                    foreach(var e in screen.GetComponentsInChildren<TokMartCanvasElement>(true))e.gameObject.SetActive(true);
                    for(var ancestor=screen.transform;ancestor&&ancestor!=ui.designRoot;ancestor=ancestor.parent)ancestor.gameObject.SetActive(true);
                    Selection.activeGameObject=screen.gameObject;SceneView.lastActiveSceneView?.FrameSelected();EditorUtility.SetDirty(ui);
                }
            }
        }
    }
}
