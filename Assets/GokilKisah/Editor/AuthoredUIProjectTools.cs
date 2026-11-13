using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace GokilKisah.Editor
{
    public static class AuthoredUIProjectTools
    {
        [MenuItem("TokTokMart/UI/Validate and prepare authored scene")]
        public static void Prepare()
        {
            if(EditorApplication.isPlaying)throw new System.InvalidOperationException("Stop Play Mode first.");
            const string prefabPath="Assets/GokilKisah/Resources/TokMartEditableUI.prefab";
            var contents=PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var canvas=contents.GetComponent<TokMartCanvas>();
                foreach(var element in canvas.GetComponentsInChildren<TokMartCanvasElement>(true))
                    if(element.transform.parent==canvas.designRoot&&element.key.StartsWith("DrawSettings"))element.transform.SetAsLastSibling();
                PrefabUtility.SaveAsPrefabAsset(contents,prefabPath);
            }
            finally {PrefabUtility.UnloadPrefabContents(contents);}
            if(!Object.FindFirstObjectByType<StoreGameController>())EditorSceneManager.OpenScene("Assets/GokilKisah/Scenes/TokMartPitch.unity");
            var game=Object.FindFirstObjectByType<StoreGameController>();
            if(!game||!game.editableUI)throw new System.InvalidOperationException("Assign the authored Canvas before validation.");
            if(!Object.FindFirstObjectByType<EventSystem>())
            {
                var events=new GameObject("UI EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
                events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
                Undo.RegisterCreatedObjectUndo(events,"Add authored UI EventSystem");
            }
            // Persist overlay stacking once in Edit Mode, so runtime never reorders authored objects.
            foreach(var element in game.editableUI.GetComponentsInChildren<TokMartCanvasElement>(true))
                if(element.transform.parent==game.editableUI.designRoot&&element.key.StartsWith("DrawSettings"))
                    element.transform.SetAsLastSibling();
            int missing=0;
            foreach(var t in game.gameObject.scene.GetRootGameObjects())
                foreach(var child in t.GetComponentsInChildren<Transform>(true))
                    missing+=GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject);
            if(missing>0)throw new System.InvalidOperationException("Missing scene scripts: "+missing);
            EditorSceneManager.MarkSceneDirty(game.gameObject.scene);
            EditorSceneManager.SaveScene(game.gameObject.scene);
            Debug.Log("Authored UI validated: scene EventSystem present; no missing scripts.");
        }
    }
}
