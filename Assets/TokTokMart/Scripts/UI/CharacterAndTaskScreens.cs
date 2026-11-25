using System.Collections.Generic;
using UnityEngine;

namespace TokTokMart
{
    public sealed partial class ShiftController
    {
        Font arcadeFont;
        readonly Dictionary<int, GUIStyle> arcadeStyles = new Dictionary<int, GUIStyle>();
        static readonly Color arcadeNavy = new Color(.10f, .19f, .23f), arcadeCream = new Color(1, .97f, .87f), arcadeCoral = new Color(.94f, .39f, .29f), arcadeGold = new Color(1, .79f, .27f), arcadeTeal = new Color(.13f, .57f, .51f);
        readonly Dictionary<string, Texture2D> arcadeIcons = new Dictionary<string, Texture2D>();
        void ArcadeText(Rect r, string value, int size, Color color, TextAnchor align = TextAnchor.MiddleLeft, bool outline = false, string uiKey = "")
        {
            using (NativeUiScope(uiKey, "ArcadeText: " + value))
            {
                if (!arcadeFont) arcadeFont = Resources.Load<Font>("Fonts/Bigbesty") ?? Resources.Load<Font>("Fonts/LilitaOne-Regular");
                int key = size * 10 + (int)align;
                if (!arcadeStyles.TryGetValue(key, out var style))
                { style = new GUIStyle { font = arcadeFont ? arcadeFont : boldFont, fontSize = size, alignment = align, wordWrap = false }; arcadeStyles[key] = style; }
                if (outline)
                {
                    style.normal.textColor = arcadeNavy;
                    foreach (var d in new[] { new Vector2(-3, 0), new Vector2(3, 0), new Vector2(0, -3), new Vector2(0, 4) }) EditableLabel(new Rect(r.position + d, r.size), value, style);
                }
                style.normal.textColor = color; EditableLabel(r, value, style);
            }
        }
        void Sticker(Rect r, Color color, bool shadow = true, float border = 4, string uiKey = "")
        {
            using (NativeUiScope(uiKey, "Sticker"))
            {
                if (shadow) Panel(new Rect(r.x + 1, r.y + 7, r.width, r.height), new Color(.04f, .10f, .13f, .28f), uiKey: "ArcadeInterface_1464");
                Panel(r, arcadeNavy, uiKey: "ArcadeInterface_1554"); Panel(new Rect(r.x + border, r.y + border, r.width - border * 2, r.height - border * 2), color, uiKey: "ArcadeInterface_1574");
            }
        }
        void Keycap(Rect r, string key, string uiKey = "")
        { using (NativeUiScope(uiKey, "Keycap")) { Sticker(r, arcadeCream, false, 3, uiKey: "ArcadeInterface_1713"); ArcadeText(r, key, 22, arcadeNavy, TextAnchor.MiddleCenter, uiKey: "ArcadeInterface_1744"); } }
        static float IconLine(Vector2 p, Vector2 a, Vector2 b, float width)
        { var d = b - a; float t = Mathf.Clamp01(Vector2.Dot(p - a, d) / Mathf.Max(.001f, d.sqrMagnitude)); return Mathf.Clamp01(width + .6f - (p - a - t * d).magnitude); }
        static float IconBox(Vector2 p, float x, float y, float w, float h)
        { return p.x >= x && p.x <= x + w && p.y >= y && p.y <= y + h ? 1 : 0; }
        Texture2D ArcadeIcon(string kind)
        {
            if (arcadeIcons.TryGetValue(kind, out var texture)) return texture;
            texture = new Texture2D(96, 96, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            for (int y = 0; y < 96; y++) for (int x = 0; x < 96; x++)
                {
                    var p = new Vector2(x, y); float a = 0;
                    if (kind == "clock")
                    {
                        a = Mathf.Clamp01(4.5f - Mathf.Abs((p - new Vector2(48, 46)).magnitude - 29));
                        a = Mathf.Max(a, IconLine(p, new Vector2(48, 46), new Vector2(48, 65), 3)); a = Mathf.Max(a, IconLine(p, new Vector2(48, 46), new Vector2(64, 37), 3));
                        a = Mathf.Max(a, IconBox(p, 38, 82, 20, 6));
                    }
                    else if (kind == "coin")
                    {
                        a = Mathf.Clamp01(4.5f - Mathf.Abs((p - new Vector2(48, 48)).magnitude - 32));
                        a = Mathf.Max(a, IconBox(p, 44, 25, 8, 46)); a = Mathf.Max(a, IconLine(p, new Vector2(61, 63), new Vector2(35, 57), 4)); a = Mathf.Max(a, IconLine(p, new Vector2(35, 57), new Vector2(61, 39), 4)); a = Mathf.Max(a, IconLine(p, new Vector2(61, 39), new Vector2(35, 32), 4));
                    }
                    else if (kind == "stock")
                    {
                        a = IconLine(p, new Vector2(18, 23), new Vector2(78, 23), 4); a = Mathf.Max(a, IconLine(p, new Vector2(18, 23), new Vector2(18, 72), 4)); a = Mathf.Max(a, IconLine(p, new Vector2(78, 23), new Vector2(78, 72), 4)); a = Mathf.Max(a, IconLine(p, new Vector2(18, 72), new Vector2(78, 72), 4)); a = Mathf.Max(a, IconBox(p, 42, 51, 12, 21)); a = Mathf.Max(a, IconBox(p, 29, 34, 19, 5));
                    }
                    else if (kind == "clean")
                    {
                        a = IconLine(p, new Vector2(37, 31), new Vector2(66, 79), 4); a = Mathf.Max(a, IconLine(p, new Vector2(18, 20), new Vector2(51, 39), 7)); a = Mathf.Max(a, IconLine(p, new Vector2(20, 15), new Vector2(55, 34), 4));
                        a = Mathf.Max(a, IconLine(p, new Vector2(72, 20), new Vector2(72, 42), 2)); a = Mathf.Max(a, IconLine(p, new Vector2(62, 31), new Vector2(82, 31), 2));
                    }
                    else if (kind == "serve")
                    {
                        a = IconLine(p, new Vector2(18, 65), new Vector2(78, 65), 4); a = Mathf.Max(a, IconLine(p, new Vector2(25, 65), new Vector2(30, 35), 4)); a = Mathf.Max(a, IconLine(p, new Vector2(30, 35), new Vector2(69, 35), 4)); a = Mathf.Max(a, IconLine(p, new Vector2(69, 35), new Vector2(77, 65), 4));
                        a = Mathf.Max(a, Mathf.Clamp01(7 - (p - new Vector2(34, 20)).magnitude)); a = Mathf.Max(a, Mathf.Clamp01(7 - (p - new Vector2(67, 20)).magnitude)); a = Mathf.Max(a, IconLine(p, new Vector2(18, 65), new Vector2(10, 77), 4));
                    }
                    else if (kind == "pause") a = Mathf.Max(IconBox(p, 27, 23, 13, 50), IconBox(p, 56, 23, 13, 50));
                    else if (kind == "check") a = Mathf.Max(IconLine(p, new Vector2(18, 46), new Vector2(38, 26), 6), IconLine(p, new Vector2(38, 26), new Vector2(78, 72), 6));
                    texture.SetPixel(x, y, new Color(1, 1, 1, a));
                }
            texture.Apply(); arcadeIcons[kind] = texture; return texture;
        }
        void Icon(Rect r, string kind, Color color, string uiKey = "") { using (NativeUiScope(uiKey, "Icon: " + kind)) { GUI.color = color; EditableTexture(r, ArcadeIcon(kind)); GUI.color = Color.white; } }
        void Portrait(Rect area, bool isZack, string uiKey = "")
        {
            using (NativeUiScope(uiKey, "Portrait"))
            {
                var sprite = CharacterPoses.Load(isZack ? "Zack_3_0" : "Sam_3_0"); if (!sprite) return;
                var rect = sprite.rect; float scale = Mathf.Min(area.width / rect.width, area.height / rect.height);
                var fit = new Rect(area.center.x - rect.width * scale / 2, area.yMax - rect.height * scale, rect.width * scale, rect.height * scale);
                EditableTextureUV(fit, sprite.texture, new Rect(rect.x / sprite.texture.width, rect.y / sprite.texture.height, rect.width / sprite.texture.width, rect.height / sprite.texture.height));
            }
        }
        void DrawArcadeSelection()
        {
            using (NativeUiScope("DrawArcadeSelection", "DrawArcadeSelection"))
            {
                Shade(uiKey: "ArcadeInterface_13070"); ArcadeText(new Rect(300, 72, 1000, 85), "WHO'S ON SHIFT?", 63, arcadeCream, TextAnchor.MiddleCenter, true, uiKey: "ArcadeInterface_13078");
                ArcadeText(new Rect(300, 157, 1000, 35), "Pick your keeper. The other has your back.", 24, arcadeCream, TextAnchor.MiddleCenter, uiKey: "ArcadeInterface_13190");
                foreach (bool pickZack in new[] { false, true })
                {
                    var r = new Rect(pickZack ? 840 : 360, 244, 400, 460); var color = pickZack ? new Color(.38f, .61f, .87f) : arcadeCoral;
                    Sticker(r, arcadeCream, true, 6, uiKey: "ArcadeInterface_13518"); Panel(new Rect(r.x + 6, r.y + 6, r.width - 12, 222), color, uiKey: "ArcadeInterface_13548");
                    Portrait(new Rect(r.x + 103, r.y - 24, 194, 252), pickZack, uiKey: "ArcadeInterface_13615");
                    ArcadeText(new Rect(r.x + 25, r.y + 243, 350, 53), pickZack ? "ZACK" : "SAM", 43, arcadeNavy, TextAnchor.MiddleCenter, uiKey: "ArcadeInterface_13684");
                    ArcadeText(new Rect(r.x + 25, r.y + 303, 350, 28), pickZack ? "READY FOR THE RUSH" : "YOUR NEIGHBOURHOOD KEEPER", 17, arcadeNavy, TextAnchor.MiddleCenter, uiKey: "ArcadeInterface_13805");
                    if (UiButton(new Rect(r.x + 28, r.y + 363, 344, 64), pickZack ? "PICK ZACK" : "PICK SAM", color, 28, uiKey: "ArcadeInterface_13965")) QueueShiftTransition(() => Choose(pickZack));
                }
                if (MobileInterface)
                    ArcadeText(new Rect(300, 768, 1000, 64), "DRAG JOYSTICK TO MOVE   /   HOLD SPRINT   /   TAP INTERACT", 24, arcadeCream, TextAnchor.MiddleCenter, uiKey: "ArcadeInterface_14128");
                else
                {
                    Keycap(new Rect(449, 778, 94, 44), "WASD", uiKey: "ArcadeInterface_14310"); ArcadeText(new Rect(554, 779, 120, 42), "MOVE", 19, arcadeCream, uiKey: "ArcadeInterface_14349");
                    Keycap(new Rect(709, 778, 85, 44), "SHIFT", uiKey: "ArcadeInterface_14421"); ArcadeText(new Rect(807, 779, 125, 42), "SPRINT", 19, arcadeCream, uiKey: "ArcadeInterface_14461");
                    Keycap(new Rect(967, 778, 48, 44), "E", uiKey: "ArcadeInterface_14535"); ArcadeText(new Rect(1028, 779, 150, 42), "INTERACT", 19, arcadeCream, uiKey: "ArcadeInterface_14571");
                }
            }
        }
        void DrawSkill()
        {
            if (skillType == 0) return; // The EventSystem handles drag and drop on the restock panel.
            using (NativeUiScope("DrawSkill" + skillType, "DrawSkill"))
            {
                if (skillType == 1) { DrawCashier(); return; }
                Shade(uiKey: "ArcadeInterface_16708"); Sticker(new Rect(392, 227, 816, 443), arcadeCream, true, 6, uiKey: "ArcadeInterface_16716");
                Panel(new Rect(398, 233, 804, 90), arcadeCoral, uiKey: "ArcadeInterface_16783");
                Icon(new Rect(424, 248, 62, 62), "clean", arcadeCream, uiKey: "ArcadeInterface_16864");
                ArcadeText(new Rect(505, 245, 650, 67), "MAKE IT SPARKLE!", 36, arcadeCream, uiKey: "ArcadeInterface_16948");
                DrawTaskCountdown(new Rect(439, 325, 722, 22)); DrawTaskDrive();
                const int required = 4;
                for (int i = 0; i < required; i++)
                {
                    var r = new Rect(698 + i * 58 - required * 29, 354, 46, 46); Sticker(r, i < taskCombo ? arcadeTeal : new Color(.79f, .84f, .78f), false, 3, uiKey: "ArcadeInterface_17219");
                    if (i < taskCombo) Icon(new Rect(r.x + 6, r.y + 6, 34, 34), "check", arcadeCream, uiKey: "ArcadeInterface_17319");
                    else ArcadeText(r, (i + 1).ToString(), 22, arcadeNavy, TextAnchor.MiddleCenter, uiKey: "ArcadeInterface_17395");
                }
                ArcadeText(new Rect(428, 408, 744, 34), "Tap DO IT or press E / SPACE inside the green zone.", 23, arcadeNavy, TextAnchor.MiddleCenter, uiKey: "ArcadeInterface_17491");
                Sticker(new Rect(439, 465, 722, 43), new Color(.75f, .79f, .71f), false, 4, uiKey: "ArcadeInterface_17631");
                Panel(new Rect(446 + 708 * (SkillZoneCenter() - SkillZoneHalf()), 471, 708 * SkillZoneHalf() * 2, 31), arcadeTeal, uiKey: "ArcadeInterface_17712");
                Sticker(new Rect(446 + SkillMarker() * 708 - 7, 454, 14, 65), Time.unscaledTime < skillFlash ? arcadeCoral : arcadeGold, false, 3, uiKey: "ArcadeInterface_17826");
                if (UiButton(new Rect(439, 563, 487, 64), "SPACE  /  DO IT!", arcadeGold, 28, uiKey: "ArcadeInterface_17988")) AttemptSkill();
                if (UiButton(new Rect(946, 563, 215, 64), "CANCEL", arcadeCream, 24, uiKey: "ArcadeInterface_18087")) CancelCurrentTask();
                GUI.enabled = true;
                if (taskFeedback.Length > 0) Text(new Rect(445, 524, 710, 28), taskFeedback, 18, taskFeedbackFailed ? arcadeCoral : arcadeTeal, FontStyle.Bold, TextAnchor.MiddleCenter, uiKey: "ArcadeInterface_18257");
            }
        }
        void DrawCashier()
        {
            using (NativeUiScope("DrawCashier", "DrawCashier"))
            {
                Shade(uiKey: "ArcadeInterface_18414"); Sticker(new Rect(325, 139, 950, 622), arcadeNavy, true, 6, uiKey: "ArcadeInterface_18422");
                Sticker(new Rect(343, 158, 397, 582), arcadeCream, false, 3, uiKey: "ArcadeInterface_18488");
                Icon(new Rect(497, 172, 72, 72), "serve", arcadeTeal, uiKey: "ArcadeInterface_18556");
                ArcadeText(new Rect(368, 242, 347, 47), "CUSTOMER RECEIPT", 29, arcadeNavy, TextAnchor.MiddleCenter, uiKey: "ArcadeInterface_18618");
                ArcadeText(new Rect(368, 289, 347, 28), WholeRinggitCheckout ? "ADD UP THE WHOLE RINGGIT" : "ADD UP THE RINGGIT AND SEN", 16, arcadeTeal, TextAnchor.MiddleCenter, uiKey: "ArcadeInterface_18725");
                for (int x = 372; x < 710; x += 17) Fill(new Rect(x, 331, 8, 2), new Color(.60f, .65f, .59f), uiKey: "ArcadeInterface_18869");
                Text(new Rect(372, 347, 224, 197), receiptItemsText, 21, arcadeNavy, uiKey: "ArcadeInterface_19023");
                Text(new Rect(599, 347, 112, 197), receiptPricesText, 21, arcadeNavy, FontStyle.Normal, TextAnchor.UpperRight, uiKey: "ArcadeInterface_19106");
                DrawTaskCountdown(new Rect(372, 558, 338, 16));
                if (taskFeedback.Length > 0) Text(new Rect(371, 608, 340, 66), taskFeedback, 19, arcadeCoral, FontStyle.Bold, uiKey: "ArcadeInterface_19263");
                if (UiButton(new Rect(372, 678, 338, 43), "CANCEL  /  ESC", new Color(.83f, .87f, .79f), 19, uiKey: "ArcadeInterface_19353")) skillType = -1;
                Sticker(new Rect(762, 158, 492, 582), new Color(.33f, .65f, .60f), false, 3, uiKey: "ArcadeInterface_19460");
                ArcadeText(new Rect(785, 174, 446, 42), "TOKTOK TILL", 30, arcadeCream, TextAnchor.MiddleCenter, uiKey: "ArcadeInterface_19542");
                Sticker(new Rect(785, 228, 446, 83), Time.unscaledTime < skillFlash ? arcadeCoral : arcadeNavy, false, 4, uiKey: "ArcadeInterface_19645");
                ArcadeText(new Rect(800, 240, 413, 59), "RM " + (priceEntry.Length == 0 ? (WholeRinggitCheckout ? "0" : "0.00") : priceEntry), 40, arcadeGold, TextAnchor.MiddleRight, uiKey: "ArcadeInterface_19752");
                string[] keys = { "7", "8", "9", "4", "5", "6", "1", "2", "3", WholeRinggitCheckout ? "-" : ".", "0", "DEL" };
                for (int i = 0; i < 12; i++)
                {
                    GUI.enabled = (i != 9 || !WholeRinggitCheckout);
                    if (UiButton(new Rect(785 + i % 3 * 152, 331 + i / 3 * 72, 141, 60), keys[i], i == 11 ? arcadeCoral : arcadeCream, 29, uiKey: "ArcadeInterface_20009"))
                    { if (i == 9) EnterPricePoint(); else if (i == 11) { if (priceEntry.Length > 0) priceEntry = priceEntry.Substring(0, priceEntry.Length - 1); } else EnterPriceDigit(keys[i][0]); }
                }
                if (UiButton(new Rect(785, 637, 446, 68), "CHARGE  /  ENTER", arcadeGold, 28, uiKey: "ArcadeInterface_20284")) SubmitPrice();
                GUI.enabled = true;
            }
        }
    }
}
