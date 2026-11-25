using UnityEngine;

namespace TokTokMart
{
    public sealed partial class ShiftController
    {
        void ArtImage(Rect area, string name, string uiKey = "")
        { using (NativeUiScope(uiKey, "ArtImage: " + name)) { var art = ShopArtwork.UI(name); if (art) EditableTexture(area, art, ScaleMode.ScaleToFit, true); } }
        bool MenuSign(Rect r, string art, string label, string id)
        {
            using (NativeUiScope(id, "UiButton: " + label))
            {
                ArtImage(r, art); Text(new Rect(r.x, r.y, r.width, 0), label, 1, Color.clear);
                bool clicked = InterfaceButton(r); if (clicked) PlaySound("Click", .55f); return clicked;
            }
        }
        void DrawNovemberMenu()
        {
            using (NativeUiScope("ArtMenu", "Main menu"))
            {
                if (levelMapOpen) return;
                var bg = ShopArtwork.UI("MenuBackground"); if (bg) EditableTexture(new Rect(0, 0, 1600, 900), bg, ScaleMode.ScaleAndCrop);
                Fill(new Rect(0, 0, 630, 900), new Color(.06f, .16f, .18f, .70f));
                ArtImage(new Rect(74, 35, 485, 248), "Logo");
                ArcadeText(new Rect(65, 289, 515, 42), "A LITTLE SHOP. A LOT OF CHAOS.", 21, arcadeCream, TextAnchor.MiddleCenter);
                ArtImage(new Rect(294, 377, 45, 440), "MenuPole");
                GUI.enabled = !settingsOpen;
                if (MenuSign(new Rect(100, 355, 440, 124), "StartButton", SavedLevel > 1 ? "BACK TO SHIFT " + SavedLevel : "OPEN THE SHOP", "Start")) { mainMenu = false; selected = false; }
                if (MenuSign(new Rect(100, 496, 440, 108), "SettingsButton", "SETTINGS", "Settings")) SetSettingsOpen(true);
                if (MenuSign(new Rect(100, 623, 440, 108), "LevelsButton", "CHOOSE A LEVEL", "Levels")) OpenLevelMap();
                if (UiButton(new Rect(178, 771, 284, 55), "CLOCK OUT", arcadeCream, 23, "Quit")) { if (!Application.isEditor) Application.Quit(); else Toast("Quit is available in the built demo."); }
                GUI.enabled = true;
                if (settingsOpen) DrawSettings();
            }
        }
    }
}
