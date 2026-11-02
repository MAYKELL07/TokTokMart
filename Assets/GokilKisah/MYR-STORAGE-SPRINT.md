# Ringgit, storage and sprint update

- Checkout uses Malaysian ringgit (MYR), displayed as RM with two decimal places. Prices are fictional gameplay values: Milk RM 3.50, Bread RM 2.50, Rice RM 5.00, Tea RM 1.50, Eggs RM 4.50, Noodles RM 2.00. Amounts are stored as integer sen and totaled from the products the shopper collected.
- Enter totals normally (11 or 11.00 both mean RM 11). The keypad and physical numpad support a decimal point. Back erases, Clear resets, Enter charges and Esc cancels. More than two decimal places are rejected.
- A separate storage room connects to the left side of the shop through an open doorway. Walk into the room and press E near the stock basket to collect stock, then return to a shelf. The partner uses the same route. Walls restrict entry to the doorway.
- Hold either Shift key while moving to sprint at 1.65 times walking speed. The stamina bar appears in the top HUD. Full stamina lasts about five seconds of continuous sprinting, then recovers after a short rest. Exhaustion requires 25% recovery before sprinting can resume. Standing or pressing against a wall consumes no stamina. Tasks and paused screens block sprinting. Touch controls include a sprint toggle.
- Camera following and navigation cover both the shop and storage room.
- Playable art now comes from licensed free Kenney online packs. Full credits and source links are in ASSET-CREDITS.md. Original imported models and selected character poses are retained in ThirdParty.

Tune Sprint Multiplier, Max Stamina, Stamina Drain, Stamina Recovery and Recovery Delay on Store Game Controller. The storage-room floor, doorway and obstacles remain editable in the scene.

The scene migration is versioned as schema 6 and avoids creating duplicate rooms. The prior scene and script backups are in Backups/2026-10-12-second-update. Editor commands onlinebake and onlineapply prepare licensed sprites and apply the scene upgrade.

Verification: Verification/myr-storage-sprint-results.txt contains the gameplay checks. Current screenshots are in Verification/MYR-Previews.

Final validation: 60 Play Mode checks passed with zero failures. The on-screen decimal key and physical numpad were manually tested by charging RM 11.00 for Milk + Bread + Rice. The active-scene source audit contains only OnlineArt sprite resources. Unity is left at the main menu.
