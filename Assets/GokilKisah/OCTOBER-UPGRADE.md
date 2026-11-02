# October 12 upgrade

The active TokMartPitch scene now contains a floor 65% wider and 80% deeper, nine shelves in three aisles, smooth vector-style artwork and a closer camera.

The camera is stationary inside its center dead zone (48% width / 44% height), eases when the player crosses that boundary, and clamps to the floor. Tune Gameplay Zoom, Dead Zone Width/Height and Follow Speed on Store Game Controller.

Start from the title menu, choose Sam or Zack, and work with WASD/arrows and E. Restocking takes three successful placements; cleaning takes four strokes with an alternating target. Checkout shows the actual basket size and individual prices: calculate the total and press the on-screen keypad, number keys or physical numpad. Enter charges the customer, Backspace erases, Delete clears and Esc cancels. Incorrect totals keep the same receipt open. Settings includes Return to main menu and the existing encounter/replay options.

Smooth sprites have four facing directions and three walk frames. Editable SVG source artwork is in ArtSources; PNG exports are in Resources/SmoothArt. These are original programmatically drawn vector illustrations, not downloaded artist assets. Existing Kenney files remain for legacy capture helpers. Kenney audio, the Cleyton Kauffman music track and Lato font retain their original licenses.

Asset research: Kenney Mini Market (https://kenney.nl/assets/mini-market), CC0, offers non-pixel 3D shop models. It was reviewed but not imported because this scene uses 2D sprites. The current smooth artwork is original, so it adds no third-party art license dependency.

The scene migration is versioned and runs once. Backups before the upgrade are at Backups/2026-10-12. The Gokil Kisah menu includes Apply Larger Smooth Store Upgrade. Tools/create-smooth-art.cjs rebuilds the artwork.

Play Mode verification results are saved under Verification/upgrade-results.txt. Check this report for the final result; legacy checks assumed six shelves and three timed scans and are superseded by the upgrade verifier.

Final verification: 42 Play Mode checks passed, zero failures. Mouse keypad and physical numpad were also tested in the Game view, including rejection of an incorrect total and successful payment of Rp 38,500. See Verification/manual-ui-checks.txt. Encounter countdowns and progress now stay on the HUD while the camera moves.
