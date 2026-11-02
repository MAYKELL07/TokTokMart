# TokTokMart mobile demo

Unity 6.3 LTS **6000.3.9f1**. Open `Assets/GokilKisah/Scenes/TokMartPitch.unity` and press Play.

The November demo includes the supplied storefront illustration, logo, UI, Sam and Zack portraits and walk cycles, and Toyol encounter animation. The existing shop, storage room, customer traffic, checkout, restocking, cleaning and encounters are retained.

## Android demo

The installable demo targets **Android 8.0+ (API 26), ARM64, landscape**. It uses a local demo/debug signature and is intended for direct device testing, rather than store submission. Build output: `Builds/Android/TokTokMart-Demo.apk`.

- Drag the left joystick to move.
- Hold Sprint while moving; tap Interact to pick up stock or begin a nearby task.
- Tap the task buttons/keypad to restock, clean and complete checkout.
- Tap Pause, then **Controls**, to change joystick size, action-button size, separate left/right padding and both control heights. The preview updates immediately. Tap **Back** to save. **Reset control layout** restores defaults.
- Desktop preview: WASD/arrows, Shift and E/Space. Enable touch controls in Settings > Gameplay.

Menus and sliders read Input System pointer/touch input directly. Gameplay tracks separate fingers for movement, held sprint and interaction. Pausing or losing focus clears held controls.

## Build

Install Android Build Support for the exact editor version, with OpenJDK 17 and Android NDK r27c. Use the Unity-bundled SDK: command-line tools 16.0, platform-tools 36.0.0, build-tools 36.0.0 and Android API 35. Configure these under Unity Preferences > External Tools.

Choose **Gokil Kisah > Build Android Demo APK**. This helper excludes unused AI inference shader variants from the demo; other builds retain the installed package behavior. Alternatively:

```powershell
& 'PATH_TO_UNITY/Editor/Unity.exe' -batchmode -quit -projectPath . -buildTarget Android -executeMethod GokilKisah.Editor.MobileDemoBuild.BatchBuild -logFile Builds/android-build.log
```

For a machine with separate toolchains, optionally create the git-ignored `Tools/android-toolchain.local.json` with `sdk`, `ndk` and `jdk` directory paths. Never commit signing keys or machine-specific credentials.

## Art and verification

All 44 supplied files are preserved under `Assets/GokilKisah/ArtSources/November2026`, including editable AI/FLA files, alternate frame rates and previews. The full supplied smooth sequences (49 Sam frames, 49 Zack frames, 41 Toyol frames) and mobile-sized UI are loaded at runtime. **Gokil Kisah > Import November Mobile Artwork** regenerates the runtime sprites without changing source art.

Source hashes, Play Mode checks and screenshots are in `Verification/MobileDemo`. Tests cover menu touch, sliders, simultaneous controls, pause/release, saved layout, loaded art and animation. The customer regression covers ten simulated minutes. A physical Android device test is still required; no phone was connected during development.

Additional art/audio/font credits: `Assets/GokilKisah/ASSET-CREDITS.md` and the included third-party license files. Supplied TokTokMart artwork retains its original ownership.

## Movement and shift polish (0.3.0)

Gaits follow actual ground distance after collisions, with uninterrupted phase when switching between walking and sprinting. Idle uses the supplied standing artwork. Footstep contacts follow the same stride; blocked movement, pause and teleports produce no extra footsteps. The supplied walk cycles are retimed for sprinting; no separate run drawings were supplied.

Every shift opens with a goal briefing before its timer starts. Customer targets rise every shift, restocking targets every three shifts, and cleanup targets every four. The workload per available second rises every shift alongside the existing traffic, patience and skill-check difficulty curves. Required cleanup work is kept available in small batches. The partner covers checkout while player restock targets remain, then returns to normal restocking support.

| Shift | Customers | Restocks | Cleanups | Time |
| --- | ---: | ---: | ---: | ---: |
| 1 | 3 | 1 | 1 | 3:22 |
| 2 | 4 | 1 | 1 | 3:30 |
| 5 | 7 | 2 | 2 | 4:22 |
| 10 | 12 | 4 | 3 | 5:46 |
| 20 | 22 | 7 | 5 | 8:18 |

Boss encounters remain every fifth shift. Completing all targets and any required boss unlocks the next shift. The HUD shows goal progress and shop chaos. Movement/shift checks, the first 20 shifts' curve, and a recorded walk/run preview are in `Verification/Polish03`.
