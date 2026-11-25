# TokTokMart

TokTokMart is a shop-management game. Serve customers, restock shelves, and clear hazards before closing. Every fifth shift brings a boss encounter.

Open the project in **Unity 6.3 LTS (6000.3.9f1)**. Open [the TokTokMart scene](Assets/TokTokMart/Scenes/TokTokMart.unity) and press Play. Use **WASD** or the arrow keys to move, **Shift** to sprint, **E** or **Space** to interact, and **Esc** for settings. Touch controls are available under **Settings → Gameplay**.

## Editing

- **TokTokMart → UI → Open UI Layout Editor** previews the menu, punch cards, HUD, settings, and minigames. See [the UI editing guide](UI-ADJUSTMENTS.md).
- Edit the shop in the Unity Hierarchy. The **Shop** object contains fixtures, colliders, interaction points, and reusable characters and hazards. The **Shift Controller** connects the authored scene and screens to gameplay.
- The main scene and punch-card prefab are versioned with their Unity metadata. Commit each moved asset together with its `.meta` file.

Use **File → Build Profiles** to build for the configured platform. See [the art and audio credits](Assets/TokTokMart/ASSET-CREDITS.md) for ownership and license information.
