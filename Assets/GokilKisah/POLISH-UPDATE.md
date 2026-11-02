# Gameplay and presentation update

Open **Gokil Kisah > Open Pitch Prototype** in Unity, press Play and choose Sam or Zack. The standalone Windows game is in `TokMartPrototype/TokMart.exe`.

Move with WASD or arrows. Press E to interact, Space to time a task and Escape to cancel a task or open Settings.

## What changed

- Movement accelerates and stops smoothly. Small collision steps prevent tunnelling during frame drops. Foot-level circular collision follows furniture bases and lets the player slide around corners.
- Shelves, checkout furniture, customers, characters and held items draw in foot-position order. A player behind a shelf is partially hidden; a player in front draws over it. Floor tiles repeat without atlas seams.
- The shop occupies a larger, uncluttered game view. A compact status bar, contextual interaction prompt and current objective replace the presentation dashboard. Lato regular and bold provide readable text.
- A human-composed, looping shop theme and fourteen artist-made sound effects cover movement, buttons, pickups, stocking, checkout scans, cleaning, failed timing, calls, switches, shots, kicks and impacts.
- Settings pauses the shift and blocks interactions with the game underneath it. Independent master/music/effects volumes, resolution presets, window/fullscreen mode, 30/60/120 fps, larger text, touch controls, name tags and focus-loss pause are available.
- **Settings > Pitch tools > Show pitch controls** reveals the replay button. It is hidden by default. Guided replays can also be started directly from that settings page and stopped safely.
- Settings preferences save locally. Encounter selection and restart live under Gameplay; artist and license credits are included in the game.

## Pitch media

The refreshed MP4s use actual Unity gameplay frames at **1920 × 1080, 30 fps**. Their audio mixes the same original music and sound clips at the timestamps logged by the game's sound events. GIFs are silent. The PDF and PNG storyboards explain each mechanic in four steps.

`Polish_Preview/` contains real screenshots of selection, audio/display/pitch settings, the timing task and both sides of shelf layering.

This remains a desktop pitch prototype with temporary artist-made character casting. It is not a completed mobile release or a guarantee that every possible bug has been eliminated. The supplied GDD's encounter counters remain unchanged.

Customers now take one to three real stocked items while browsing, queue at the register, pay and leave. Payment does not remove items a second time. Customer mishaps and Miss Salmah can cause spills. See CUSTOMER-UPDATE.md for the full behaviour and editable scene details.
