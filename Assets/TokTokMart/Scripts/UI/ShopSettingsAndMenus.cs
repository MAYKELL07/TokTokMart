using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TokTokMart
{
    public sealed partial class ShiftController
    {
        Font boldFont;
        Texture2D roundedPanel;
        GUIStyle panelStyle;
        Vector2 playerVelocity;
        bool settingsOpen, autoPaused, focused = true, showTouchControls, pauseOnFocusLoss = true;
        int settingsTab, frameCap = 60, resolutionIndex = 1;
        float masterVolume = .8f, musicVolume = .35f, effectsVolume = .7f, skillFlash, textScale = 1;
        [SerializeField] AudioSource musicSource, effectsSource;
        readonly Dictionary<string, AudioClip> sounds = new Dictionary<string, AudioClip>();
        readonly HashSet<KeyCode> heldKeys = new HashSet<KeyCode>();
        int actionInputFrame = -1, escapeInputFrame = -1;
        readonly Color paper = new Color(.97f, .95f, .89f), surface = new Color(.15f, .23f, .23f), teal = new Color(.13f, .67f, .53f);
        readonly string[] scenarioTitles = { "Store shift", "Goblins", "K-Kung", "Miss Salmah", "Bo & the gang" };
        readonly string[] scenarioDescriptions ={
            "Keep the shelves stocked, serve your customers and clean up spills.",
            "Call Ray. Shoo goblins to stun them so he can land his punches.",
            "Restore three circuits. Dodge the marked areas while relighting the shop.",
            "Dodge her rush, move behind her and tap Interact to fire the nail gun.",
            "Return three Ikan Iblis cans. Beat each timer and dodge the throws."};
        void InitializePolish()
        {
            LoadControlSettings();
            masterVolume = PlayerPrefs.GetFloat("TokMart.Master", .8f); musicVolume = PlayerPrefs.GetFloat("TokMart.Music", .35f); effectsVolume = PlayerPrefs.GetFloat("TokMart.Effects", .7f);
            showTouchControls = UnityEngine.Device.Application.isMobilePlatform || PlayerPrefs.GetInt("TokMart.Touch", 0) == 1;
            textScale = PlayerPrefs.GetFloat("TokMart.Text", 1); frameCap = PlayerPrefs.GetInt("TokMart.FPS", 60);
            pauseOnFocusLoss = PlayerPrefs.GetInt("TokMart.Focus", 1) == 1;
            sounds.Clear();
            foreach (string key in new[] { "Click", "Confirm", "Pickup", "Stock", "Scan", "Mop", "Miss", "Call", "Switch", "Shot", "Kick", "Impact", "Success", "Footstep", "Splash", "Clatter", "ClockIn" })
            { var clip = Resources.Load<AudioClip>("Audio/" + key); if (clip) sounds.Add(key, clip); else Debug.LogError("Missing sound: " + key); }
            QualitySettings.vSyncCount = 0; Application.targetFrameRate = frameCap;
            ApplyAudio();
            if (musicSource.clip && !musicSource.isPlaying) musicSource.Play();
        }
        void PlaySound(string key, float gain = 1)
        {
            if (key == "Mop" || key == "Stock" || key == "Scan") DeviceVibration.Pulse(18);
            else if (key == "Miss" || key == "Impact") DeviceVibration.Pulse(55);
            if (!effectsSource || !sounds.TryGetValue(key, out var clip)) return;
            effectsSource.PlayOneShot(clip, Mathf.Clamp01(gain));
        }
        void ApplyAudio()
        {
            if (editableUI && editableUI.starAudio) editableUI.starAudio.volume = masterVolume * effectsVolume;
            if (musicSource) musicSource.volume = masterVolume * musicVolume * musicBed;
            if (effectsSource) effectsSource.volume = masterVolume * effectsVolume;
        }
        void SaveSettings()
        {
            SaveControlSettings();
            PlayerPrefs.SetFloat("TokMart.Master", masterVolume); PlayerPrefs.SetFloat("TokMart.Music", musicVolume); PlayerPrefs.SetFloat("TokMart.Effects", effectsVolume);
            PlayerPrefs.SetInt("TokMart.Touch", showTouchControls ? 1 : 0);
            PlayerPrefs.SetInt("TokMart.Focus", pauseOnFocusLoss ? 1 : 0); PlayerPrefs.SetFloat("TokMart.Text", textScale); PlayerPrefs.SetInt("TokMart.FPS", frameCap); PlayerPrefs.Save(); ApplyAudio();
        }
        void SetSettingsOpen(bool open)
        { settingsOpen = open; bufferedActionUntil = -1; ReleaseMobileControls(); playerVelocity = Vector2.zero; Time.timeScale = open || autoPaused || shiftBriefOpen ? 0 : 1; if (!open) SaveSettings(); }
        float musicBed = 1;
        void UpdatePolish()
        {
            autoPaused = !focused && pauseOnFocusLoss;
            Time.timeScale = settingsOpen || autoPaused || shiftBriefOpen || scoreGuideOpen ? 0 : 1;
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                EscapeInput();
            }
            float musicTarget = settingsOpen || autoPaused ? .33f : skillType >= 0 ? .56f : levelResults ? .68f : 1f;
            musicBed = Mathf.MoveTowards(musicBed, musicTarget, Time.unscaledDeltaTime * 2);
            ApplyAudio();
            RefreshReleaseUI();
            RefreshRestockPanel();
        }
        void OnApplicationFocus(bool focus) { focused = focus; if (!focus) { bufferedActionUntil = -1; heldKeys.Clear(); ReleaseMobileControls(); playerVelocity = Vector2.zero; } }
        void OnApplicationPause(bool paused) { if (paused) { ReleaseMobileControls(); bufferedActionUntil = -1; heldKeys.Clear(); playerVelocity = Vector2.zero; SaveSettings(); } focused = !paused; }
        void EscapeInput()
        {
            if (shiftTransition != null) return;
            if (escapeInputFrame == Time.frameCount) return; escapeInputFrame = Time.frameCount;
            if (scoreGuideOpen) { scoreGuideOpen = false; return; }
            if (levelMapOpen && mainMenu) { levelMapOpen = false; return; }
            if (skillType >= 0 && !settingsOpen) CancelCurrentTask();
            else SetSettingsOpen(!settingsOpen);
        }
        void ActionInput()
        {
            if (shiftTransition != null) return;
            if (actionInputFrame == Time.frameCount || settingsOpen || autoPaused || !selected || mainMenu || gameOver || levelResults || shiftBriefOpen || scoreGuideOpen) return;
            actionInputFrame = Time.frameCount;
            if (skillType >= 0) { bufferedActionUntil = -1; Interact(); }
            else { bufferedActionUntil = Time.unscaledTime + .14f; if (InteractionInReach()) { bufferedActionUntil = -1; Interact(); } }
        }
        void GUIKeyboard()
        {
            // IMGUI events also support keyboard access through desktop automation and accessibility tools.
            var e = Event.current;
            if (e.type == EventType.KeyUp) { heldKeys.Remove(e.keyCode); return; }
            if (e.type != EventType.KeyDown || !heldKeys.Add(e.keyCode)) return;
            if (e.keyCode == KeyCode.Escape) { EscapeInput(); e.Use(); }
            else if ((e.keyCode == KeyCode.E || e.keyCode == KeyCode.Space) && selected && !settingsOpen) { ActionInput(); e.Use(); }
        }
        void Panel(Rect r, Color c, bool shadow = false, string uiKey = "")
        {
            using (NativeUiScope(uiKey, "Panel"))
            {
                if (drawingWorldOverlay && !WorldOverlayVisible(r)) return;
                if (!roundedPanel)
                {
                    roundedPanel = new Texture2D(32, 32, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
                    for (int y = 0; y < 32; y++) for (int x = 0; x < 32; x++)
                        {
                            float dx = Mathf.Max(8 - x, Mathf.Max(0, x - 23)), dy = Mathf.Max(8 - y, Mathf.Max(0, y - 23));
                            roundedPanel.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(8.5f - Mathf.Sqrt(dx * dx + dy * dy))));
                        }
                    roundedPanel.Apply(); panelStyle = new GUIStyle { normal = { background = roundedPanel }, border = new RectOffset(10, 10, 10, 10) };
                }
                if (Event.current.type != EventType.Repaint) return;
                if (NativeUiReady) { if (shadow) editableUI.Panel(new Rect(r.x, r.y + 5, r.width, r.height), new Color(0, 0, 0, .18f), NativeMatrix); editableUI.Panel(r, c, NativeMatrix); return; }
                if (shadow) { GUI.color = new Color(0, 0, 0, .18f); panelStyle.Draw(new Rect(r.x, r.y + 5, r.width, r.height), false, false, false, false); }
                GUI.color = c; panelStyle.Draw(r, false, false, false, false); GUI.color = Color.white;
            }
        }
        bool UiButton(Rect r, string label, Color c, int size = 18, string uiKey = "")
        {
            using (NativeUiScope(uiKey, "UiButton: " + label))
            {
                bool hover = r.Contains(Event.current.mousePosition) && GUI.enabled;
                Sticker(r, !GUI.enabled ? Color.Lerp(c, paper, .45f) : hover ? Color.Lerp(c, Color.white, .17f) : c, uiKey: "StoreInterface_8183");
                ArcadeText(r, label, size, c == surface || c == arcadeCoral ? arcadeCream : arcadeNavy, TextAnchor.MiddleCenter, uiKey: "StoreInterface_8284");
                bool clicked = InterfaceButton(r); if (clicked) PlaySound("Click", .55f); return clicked;
            }
        }
        void Eyebrow(Rect r, string label, Color c) { Text(r, label, 14, c, FontStyle.Bold, uiKey: "StoreInterface_8540"); }
        void Shade(string uiKey = "") { using (NativeUiScope(uiKey, "Shade")) { Fill(new Rect(0, 0, 1600, 900), new Color(.04f, .10f, .10f, .75f), uiKey: "StoreInterface_8598"); } }
        void OnGUI()
        {
            if (Event.current.type == EventType.Repaint)
            {
                EnsureEditableCanvas();
                float nativeScale = Mathf.Min(Screen.width / 1600f, Screen.height / 900f);
                var nativeMatrix = Matrix4x4.TRS(new Vector3((Screen.width - 1600 * nativeScale) / 2, (Screen.height - 900 * nativeScale) / 2, 0), Quaternion.identity, new Vector3(nativeScale, nativeScale, 1));
                editableUI.Begin(nativeMatrix);
            }
            try { RenderInterface(); }
            finally { if (Event.current.type == EventType.Repaint && editableUI) editableUI.End(); }
        }
        void RenderInterface()
        {
            CashierKeyboard(); GUIKeyboard();
            float scale = Mathf.Min(Screen.width / 1600f, Screen.height / 900f);
            GUI.matrix = Matrix4x4.identity;
            GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1600 * scale) / 2, (Screen.height - 900 * scale) / 2, 0), Quaternion.identity, new Vector3(scale, scale, 1));
            interfaceCanvasMatrix = GUI.matrix;
            if (shiftTransition != null && !shiftBriefOpen) { GUI.matrix = Matrix4x4.identity; return; }
            if (mainMenu) { DrawNovemberMenu(); GUI.matrix = Matrix4x4.identity; return; }
            if (levelResults) { if (settingsOpen) DrawSettings(); GUI.matrix = Matrix4x4.identity; return; }
            var canvas = GUI.matrix; GUI.matrix = SceneOverlayMatrix(); drawingWorldOverlay = true;
            if (skillType < 0 && !settingsOpen && !autoPaused && !shiftBriefOpen && !scoreGuideOpen && !gameOver)
            {
                DrawStore();
                if (selected) DrawShopperFeedback();
            }
            drawingWorldOverlay = false; GUI.matrix = canvas;
            GUI.enabled = !settingsOpen;
            bool gameplayHud = selected && skillType < 0 && !settingsOpen && !autoPaused && !shiftBriefOpen && !scoreGuideOpen && !gameOver;
            if (gameplayHud) DrawNovemberHud();
            GUI.enabled = !settingsOpen;
            if (showTouchControls && gameplayHud) DrawMobileControls();
            GUI.enabled = true;
            if (!selected && !settingsOpen) DrawArcadeSelection();
            else if (gameOver && !settingsOpen) DrawGameOver();
            else if (skillType >= 0 && !settingsOpen && !autoPaused) DrawSkill();
            if (shiftBriefOpen && !settingsOpen) DrawShiftBriefing();
            if (autoPaused && !settingsOpen) { Shade(uiKey: "StoreInterface_10361"); ArcadeText(new Rect(450, 380, 700, 80), "ON A BREAK", 56, arcadeCream, TextAnchor.MiddleCenter, true, uiKey: "StoreInterface_10369"); ArcadeText(new Rect(450, 470, 700, 40), "Return to the shop to carry on.", 23, arcadeCream, TextAnchor.MiddleCenter, uiKey: "StoreInterface_10463"); }
            if (settingsOpen) DrawSettings();
            if (scoreGuideOpen) DrawScoreGuide();
            GUI.matrix = Matrix4x4.identity;
        }
        string InteractionLabel()
        {
            if (skillType >= 0) return "Time the task";
            string encounterHint = ActiveInteractionLabel(); if (encounterHint != null) return encounterHint;
            if (NearestDroppedItem() >= 0) return "Pick up fallen stock";
            if (NearestSpill() >= 0) return "Mop the spill";
            if (StationInReach(shop.stockroom)) return "Collect stock";
            int index = NearestShelf(); if (index >= 0) return shelfStock[index] >= ShelfCapacity(index) ? "Shelf is full" : carryingCrate ? "Restock shelf" : "Needs a stock crate";
            if (StationInReach(shop.register)) return CustomerAtRegister() != null ? "Serve customer" : "Waiting for a shopper";
            return "Move near a station";
        }
        void DrawGameOver()
        {
            using (NativeUiScope("DrawGameOver", "DrawGameOver"))
            {
                Shade(uiKey: "StoreInterface_15871"); Panel(new Rect(450, 285, 700, 336), paper, true, uiKey: "StoreInterface_15879");
                Eyebrow(new Rect(490, 316, 620, 22), "SHIFT INTERRUPTED", red);
                Text(new Rect(490, 361, 620, 56), "Bo beat the countdown.", 34, ink, FontStyle.Bold, uiKey: "StoreInterface_16007");
                Text(new Rect(490, 430, 620, 66), "Find the scattered Ikan Iblis and kick it at his car before he throws his can.", 23, new Color(.30f, .41f, .39f), uiKey: "StoreInterface_16098");
                if (Button(new Rect(490, 533, 620, 51), "Try again", gold, 22, uiKey: "StoreInterface_16255")) ResetRun(Encounter.Bo);
            }
        }
        float Slider(Rect r, float value, string uiKey = "")
        {
            using (NativeUiScope(uiKey, "Slider"))
            {
                if (NativeUiReady) value = editableUI.Slider(r, value, GUI.enabled, NativeMatrix);
                Panel(new Rect(r.x, r.y + 12, r.width, 8), new Color(.70f, .76f, .68f), uiKey: "StoreInterface_16979");
                Panel(new Rect(r.x, r.y + 12, r.width * value, 8), teal, uiKey: "StoreInterface_17056");
                Panel(new Rect(r.x + r.width * value - 10, r.y + 6, 20, 20), ink, uiKey: "StoreInterface_17118"); return value;
            }
        }
        void VolumeRow(float y, string label, ref float value, string uiKey = "")
        { using (NativeUiScope(uiKey, "VolumeRow")) { Text(new Rect(641, y, 265, 35), label, 23, ink, FontStyle.Bold, uiKey: "StoreInterface_17266"); value = Slider(new Rect(934, y + 1, 320, 36), value, uiKey: "StoreInterface_17329"); Text(new Rect(1273, y, 80, 36), Mathf.RoundToInt(value * 100) + "%", 21, ink, uiKey: "StoreInterface_17368"); } }
        void ToggleRow(float y, string title, string detail, ref bool value, string uiKey = "")
        {
            using (NativeUiScope(uiKey, "ToggleRow"))
            {
                Text(new Rect(641, y, 560, 32), title, 23, ink, FontStyle.Bold, uiKey: "StoreInterface_17534"); Text(new Rect(641, y + 36, 570, 54), detail, 18, new Color(.30f, .41f, .39f), uiKey: "StoreInterface_17591");
                if (Button(new Rect(1245, y + 5, 90, 42), value ? "On" : "Off", value ? mint : new Color(.79f, .83f, .76f), 18, uiKey: "StoreInterface_17675")) value = !value;
            }
        }
        void DrawSettings()
        {
            using (NativeUiScope("DrawSettings" + settingsTab, "DrawSettings"))
            {
                Shade(uiKey: "StoreInterface_17839"); Sticker(new Rect(240, 102, 1120, 694), paper, true, 6, uiKey: "StoreInterface_17847");
                ArcadeText(new Rect(278, 133, 590, 55), "SHOP SETTINGS", 38, arcadeNavy, uiKey: "StoreInterface_17909");
                if (Button(new Rect(1187, 133, 133, 46), "Back", surface, 17, uiKey: "StoreInterface_17992")) SetSettingsOpen(false);
                Fill(new Rect(278, 206, 1042, 1), new Color(.77f, .82f, .75f), uiKey: "StoreInterface_18080");
                string[] tabs = { "Audio", "Display", "Gameplay", "", "Controls", "Credits" };
                for (int i = 0; i < tabs.Length; i++) if (i != 3 && Button(new Rect(278, 235 + i * 62, 270, 52), tabs[i], settingsTab == i ? mint : new Color(.86f, .89f, .82f), 21, uiKey: "StoreInterface_18276")) settingsTab = i;
                Fill(new Rect(582, 235, 1, 475), new Color(.77f, .82f, .75f), uiKey: "StoreInterface_18398");
                if (settingsTab == 0)
                {
                    Text(new Rect(641, 235, 650, 45), "Audio", 30, ink, FontStyle.Bold, uiKey: "StoreInterface_18516");
                    VolumeRow(322, "Master volume", ref masterVolume, uiKey: "StoreInterface_18594"); VolumeRow(403, "Music", ref musicVolume, uiKey: "StoreInterface_18642"); VolumeRow(484, "Sound effects", ref effectsVolume, uiKey: "StoreInterface_18681");
                    if (Button(new Rect(641, 592, 325, 47), "Preview sound", mint, 20, uiKey: "StoreInterface_18750")) PlaySound("Success");
                    Text(new Rect(641, 659, 650, 48), "Shop music by Cleyton Kauffman · Sound effects by Kenney", 18, new Color(.30f, .41f, .39f), uiKey: "StoreInterface_18845");
                }
                else if (settingsTab == 1)
                {
                    Text(new Rect(641, 235, 650, 45), "Display & readability", 30, ink, FontStyle.Bold, uiKey: "StoreInterface_19045");
                    GUI.enabled = !UnityEngine.Device.Application.isMobilePlatform;
                    Text(new Rect(641, 306, 650, 32), "Window resolution", 23, ink, FontStyle.Bold, uiKey: "StoreInterface_19217");
                    string[] presets = { "1280 × 720", "1920 × 1080", "2560 × 1440" };
                    for (int i = 0; i < 3; i++) if (Button(new Rect(641 + i * 229, 350, 215, 47), presets[i], resolutionIndex == i ? mint : new Color(.84f, .88f, .81f), 18, uiKey: "StoreInterface_19407")) { resolutionIndex = i; if (!Application.isEditor) Screen.SetResolution(i == 0 ? 1280 : i == 1 ? 1920 : 2560, i == 0 ? 720 : i == 1 ? 1080 : 1440, Screen.fullScreenMode); else Toast("Resolution applies in the standalone game."); }
                    if (Button(new Rect(641, 423, 690, 46), Screen.fullScreen ? "Switch to windowed" : "Switch to fullscreen", new Color(.84f, .88f, .81f), 20, uiKey: "StoreInterface_19725")) { if (!Application.isEditor) Screen.fullScreen = !Screen.fullScreen; else Toast("Fullscreen applies in the standalone game."); }
                    GUI.enabled = true;
                    Text(new Rect(641, 496, 650, 32), "Frame rate", 23, ink, FontStyle.Bold, uiKey: "StoreInterface_20021");
                    int[] rates = { 30, 60, 120 }; for (int i = 0; i < 3; i++) if (Button(new Rect(641 + i * 229, 537, 215, 45), rates[i] + " fps", frameCap == rates[i] ? mint : new Color(.84f, .88f, .81f), 19, uiKey: "StoreInterface_20151")) { frameCap = rates[i]; Application.targetFrameRate = frameCap; }
                    Text(new Rect(641, 617, 270, 35), "Text size", 23, ink, FontStyle.Bold, uiKey: "StoreInterface_20333");
                    if (Button(new Rect(940, 612, 183, 45), "Standard", textScale < 1.1f ? mint : new Color(.84f, .88f, .81f), 18, uiKey: "StoreInterface_20418")) textScale = 1;
                    if (Button(new Rect(1139, 612, 192, 45), "Larger", textScale > 1.1f ? mint : new Color(.84f, .88f, .81f), 18, uiKey: "StoreInterface_20543")) textScale = 1.15f;
                }
                else if (settingsTab == 2)
                {
                    bool vibration = DeviceVibration.Enabled; ToggleRow(539, "Vibration", "Mobile feedback for taps, tasks and stars.", ref vibration, uiKey: "MobileVibration"); if (vibration != DeviceVibration.Enabled) { DeviceVibration.Enabled = vibration; if (vibration) DeviceVibration.Pulse(35); }
                    ToggleRow(236, "Touch controls", "Joystick + hold Sprint + tap Interact. Customize in Controls.", ref showTouchControls, uiKey: "StoreInterface_20732");
                    ToggleRow(438, "Pause when unfocused", "Freeze the shift when you switch windows.", ref pauseOnFocusLoss, uiKey: "StoreInterface_20965");
                    if (campaignActive)
                    {
                        Text(new Rect(641, 646, 650, 40), "SHIFT " + CurrentLevel + "  /  " + Targets.Title, 22, ink, FontStyle.Bold, uiKey: "ShiftType");
                        if (Button(new Rect(641, 717, 690, 44), "Restart level " + CurrentLevel, gold, 19, uiKey: "StoreInterface_21472")) { SetSettingsOpen(false); BeginCampaignLevel(CurrentLevel, campaignPersistence); }
                    }
                }
                else if (settingsTab == 4) DrawControlSettings();
                else
                {
                    Text(new Rect(641, 235, 650, 45), "TokTokMart credits", 30, ink, FontStyle.Bold, uiKey: "StoreInterface_23154");
                    Text(new Rect(641, 292, 650, 335), "Lead Developer & Project Manager\nMichaellie Ferrel Yoandike (TP087326)\nDeveloper & Design\nVaughn Avery Mar Mier (TP101735)\nLead Artist\nAnastasia Clarita Budiyanto (TP092119)\nTechnical Art & Producer\nTan Wei Hao (TP071285)\nLead Animator\nCherepanova Viktoriia (TP080433)", 23, new Color(.25f, .38f, .34f), uiKey: "StoreInterface_23246");
                    Text(new Rect(641, 650, 650, 60), "Additional art and SFX · Kenney (CC0)\nBigbesty · Khurasan   /   Lato · Łukasz Dziedzic", 19, new Color(.30f, .41f, .39f), uiKey: "StoreInterface_23631");
                }
                if (Button(new Rect(267, 730, 290, 44), "Main menu", surface, 19, uiKey: "StoreInterface_23832")) ReturnToMainMenu();
                if (Event.current.isMouse) Event.current.Use();
            }
        }
    }
}
