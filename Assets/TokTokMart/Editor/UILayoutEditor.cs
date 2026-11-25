using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace TokTokMart.Editor
{
    public sealed class UILayoutEditor : EditorWindow
    {
        const string PunchCardPrefabPath = "Assets/TokTokMart/UI/TokMartPunchCardUI.prefab";
        static readonly string[] ScreenKeys = { "ArtMenu_0", "DrawArcadeSelection_0", "ShiftBrief_0", "ShiftHud_0", "DrawMobileControls_0", "DrawSkill1_0", "DrawSkill2_0", "StarHint_0", "DrawGameOver_0", "DrawSettings0_0", "DrawSettings1_0", "DrawSettings2_0", "DrawSettings4_0", "DrawSettings5_0" };
        static readonly string[] ScreenLabels = { "Main Menu", "Character Select", "Shift Briefing", "Shift HUD", "Touch Controls", "Checkout Task", "Cleaning Task", "Star Guide", "Game Over", "Settings - Audio", "Settings - Display", "Settings - Gameplay", "Settings - Controls", "Credits" };
        static readonly string[] PunchCardLabels = { "Choose a Shift", "Shift Results", "Shift Transition" };
        [SerializeField] AuthoredCanvas canvas;
        Vector2 scroll;

        [MenuItem("TokTokMart/UI/Open UI Layout Editor")]
        public static void OpenMenu() => Open(SceneCanvas());

        static AuthoredCanvas SceneCanvas()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded) return null;
            foreach (var root in scene.GetRootGameObjects())
            {
                var controller = root.GetComponentInChildren<ShiftController>(true);
                if (controller && controller.editableUI) return controller.editableUI;
            }
            return null;
        }
        public static void Open(AuthoredCanvas ui)
        {
            var window = GetWindow<UILayoutEditor>("UI Layout Editor");
            window.canvas = ui; window.minSize = new Vector2(310, 440); window.Show();
        }
        public static CanvasBinding Find(AuthoredCanvas ui, string key) => ui ? ui.GetComponentsInChildren<CanvasBinding>(true).FirstOrDefault(e => e.key == key || e.key.EndsWith("/" + key, StringComparison.Ordinal)) : null;
        static bool IsScreen(CanvasBinding element) => ScreenKeys.Any(key => element.key == key || element.key.EndsWith("/" + key, StringComparison.Ordinal));

        public static void Preview(AuthoredCanvas ui, string key)
        {
            if (Application.isPlaying || !ui || !ui.designRoot) return;
            var selected = Find(ui, key); if (!selected) return;
            var restock = UnityEngine.Object.FindFirstObjectByType<RestockScreen>(FindObjectsInactive.Include);
            if (restock && restock.gameObject.activeSelf)
            {
                Undo.RecordObject(restock.gameObject, "Switch UI preview");
                restock.gameObject.SetActive(false);
            }
            Undo.RegisterFullObjectHierarchyUndo(ui.gameObject, "Preview UI screen");
            foreach (var e in ui.GetComponentsInChildren<CanvasBinding>(true)) e.gameObject.SetActive(false);
            void Show(CanvasBinding screen)
            {
                foreach (var e in screen.GetComponentsInChildren<CanvasBinding>(true))
                {
                    bool nestedScreen = false;
                    for (var t = e.transform; t && t != screen.transform; t = t.parent)
                    { var ancestor = t.GetComponent<CanvasBinding>(); if (ancestor && IsScreen(ancestor)) { nestedScreen = true; break; } }
                    e.gameObject.SetActive(!nestedScreen && !e.hideFromGame);
                }
            }
            // Settings appear on the normal menu; nested level-map groups stay hidden.
            if (key.StartsWith("DrawSettings", StringComparison.Ordinal)) { var menu = Find(ui, "ArtMenu_0"); if (menu) Show(menu); }
            Show(selected);
            for (var ancestor = selected.transform; ancestor && ancestor != ui.designRoot; ancestor = ancestor.parent)
            { var e = ancestor.GetComponent<CanvasBinding>(); ancestor.gameObject.SetActive(!e || !e.hideFromGame); }
            Selection.activeGameObject = selected.gameObject;
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView) { sceneView.in2DMode = true; sceneView.Frame(new Bounds(ui.designRoot.position, new Vector3(1600, 900, 1) * ui.designRoot.lossyScale.x), true); }
            EditorUtility.SetDirty(ui);
            if (ui.gameObject.scene.IsValid()) EditorSceneManager.MarkSceneDirty(ui.gameObject.scene);
            SceneView.RepaintAll();
        }

        void OnGUI()
        {
            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
            {
                scroll = EditorGUILayout.BeginScrollView(scroll);
                EditorGUILayout.LabelField("Punch-card screens", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox("Choose a screen, edit its objects in Prefab Mode, then save with Ctrl+S. The game fills in shift names, scores and progress.", MessageType.Info);
                var stage = PrefabStageUtility.GetCurrentPrefabStage();
                for (int i = 0; i < PunchCardLabels.Length; i++)
                    if (GUILayout.Button(PunchCardLabels[i])) PreviewPunchCard(i);
                if (stage != null)
                {
                    if (GUILayout.Button("Return to Scene Canvas")) StageUtility.GoToMainStage();
                    EditorGUILayout.EndScrollView();
                    return;
                }

                canvas = SceneCanvas();
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Scene screens", EditorStyles.boldLabel);
                using (new EditorGUI.DisabledScope(true))
                    EditorGUILayout.ObjectField("Game Canvas", canvas, typeof(AuthoredCanvas), true);
                if (!canvas)
                {
                    EditorGUILayout.HelpBox("Open the gameplay scene with an Editable UI assigned to Store Game Controller.", MessageType.Warning);
                    EditorGUILayout.EndScrollView();
                    return;
                }
                EditorGUILayout.HelpBox("Choose a screen, select a group in the Hierarchy, and use the Rect tool (T) or Rect Transform to edit its layout. Save the scene with Ctrl+S.", MessageType.Info);
                var restock = UnityEngine.Object.FindFirstObjectByType<RestockScreen>(FindObjectsInactive.Include);
                if (restock && GUILayout.Button("Restock Task"))
                {
                    Undo.RecordObject(restock.gameObject, "Preview Restock");
                    restock.gameObject.SetActive(true);
                    Selection.activeGameObject = restock.gameObject;
                    EditorSceneManager.MarkSceneDirty(restock.gameObject.scene);
                }
                for (int i = 0; i < ScreenKeys.Length; i++)
                    if (Find(canvas, ScreenKeys[i]) && GUILayout.Button(ScreenLabels[i])) Preview(canvas, ScreenKeys[i]);
                EditorGUILayout.Space(); EditorGUILayout.LabelField("Main menu fade", EditorStyles.boldLabel);
                var fade = canvas.GetComponentInChildren<HorizontalMenuFade>(true);
                if (fade)
                {
                    var binding = fade.GetComponent<CanvasBinding>();
                    bool hidden = EditorGUILayout.Toggle("Hide menu fill", binding.hideFromGame);
                    if (hidden != binding.hideFromGame) { Undo.RecordObject(binding, "Toggle menu fill"); binding.hideFromGame = hidden; SaveChange(binding); Preview(canvas, "ArtMenu_0"); }
                    var serialized = new SerializedObject(fade); serialized.Update();
                    EditorGUILayout.PropertyField(serialized.FindProperty("leftOpacity"));
                    EditorGUILayout.PropertyField(serialized.FindProperty("rightOpacity"));
                    if (serialized.ApplyModifiedProperties()) { fade.GetComponent<Graphic>().SetVerticesDirty(); SaveChange(fade); }
                    if (GUILayout.Button("Select Fill - Position / Size / Color")) { Preview(canvas, "ArtMenu_0"); Selection.activeGameObject = fade.gameObject; }
                }
                EditorGUILayout.Space(); EditorGUILayout.LabelField("Credits text", EditorStyles.boldLabel);
                EditCredit("Heading", "StoreInterface_23154_0", false);
                EditCredit("Names, roles and sources", "StoreInterface_23246_0", true);
                EditCredit("Footer", "StoreInterface_23631_0", true);
                if (GUILayout.Button("Preview Credits")) Preview(canvas, "DrawSettings5_0");
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("To remove a decorative item from view, enable Hide From Game on its Tok Mart Canvas Element. Hide a whole group to include its children. Move an entire button group so artwork and its hit area stay together. Edit outside Play mode so changes persist.", MessageType.None);
                EditorGUILayout.EndScrollView();
            }
        }
        static void PreviewPunchCard(int screen)
        {
            if (!AssetDatabase.LoadAssetAtPath<GameObject>(PunchCardPrefabPath))
            {
                EditorUtility.DisplayDialog("Punch Card UI missing", "Restore the TokMartPunchCardUI prefab to open this screen.", "OK");
                return;
            }
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage == null || stage.assetPath != PunchCardPrefabPath)
                stage = PrefabStageUtility.OpenPrefab(PunchCardPrefabPath);
            if (stage == null) return;

            var view = stage.prefabContentsRoot.GetComponent<PunchCardScreens>();
            Undo.RegisterFullObjectHierarchyUndo(view.gameObject, "Preview punch-card screen");
            var selected = view.PreviewScreen(screen);
            Selection.activeGameObject = selected.gameObject;
            EditorApplication.delayCall += () =>
            {
                if (!selected || PrefabStageUtility.GetCurrentPrefabStage() != stage) return;
                EditorGUIUtility.PingObject(selected.gameObject);
                var sceneView = SceneView.lastActiveSceneView;
                if (!sceneView) return;
                sceneView.in2DMode = true;
                var corners = new Vector3[4];
                selected.GetWorldCorners(corners);
                var bounds = new Bounds(corners[0], Vector3.zero);
                foreach (var corner in corners) bounds.Encapsulate(corner);
                sceneView.pivot = bounds.center;
                float aspect = sceneView.position.width / Mathf.Max(1, sceneView.position.height);
                sceneView.size = Mathf.Max(bounds.extents.x / aspect, bounds.extents.y) * 1.2f;
                sceneView.Repaint();
            };
        }

        void EditCredit(string label, string key, bool multiline)
        {
            var screen = Find(canvas, "DrawSettings5_0"); if (!screen) return;
            var group = screen.GetComponentsInChildren<CanvasBinding>(true).FirstOrDefault(e => e.key.EndsWith("/" + key, StringComparison.Ordinal));
            var text = group ? group.GetComponentInChildren<Text>(true) : null;
            var tmp = group ? group.GetComponentInChildren<TMP_Text>(true) : null;
            if (!text && !tmp) return;
            string before = text ? text.text : tmp.text;
            EditorGUILayout.LabelField(label);
            string after = multiline ? EditorGUILayout.TextArea(before, new GUIStyle(EditorStyles.textArea) { wordWrap = true }, GUILayout.Width(Mathf.Max(240, position.width - 30)), GUILayout.MinHeight(label == "Footer" ? 58 : 180)) : EditorGUILayout.TextField(before);
            if (before == after) return;
            var component = text ? (Component)text : tmp; Undo.RecordObject(component, "Edit credits");
            if (text) text.text = after; else tmp.text = after;
            var binding = component.GetComponent<CanvasBinding>(); Undo.RecordObject(binding, "Keep authored credits"); binding.bindGameData = false;
            SaveChange(binding); SaveChange(component);
        }
        static void SaveChange(UnityEngine.Object obj)
        {
            EditorUtility.SetDirty(obj);
            if (PrefabUtility.IsPartOfPrefabInstance(obj)) PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
            if (obj is Component c && c.gameObject.scene.IsValid()) EditorSceneManager.MarkSceneDirty(c.gameObject.scene);
        }

    }
}
