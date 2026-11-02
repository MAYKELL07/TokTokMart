> November 2026: see the repository root README.md for the current Android demo, supplied character animations, artwork and adjustable mobile controls. The original pitch notes below describe the earlier prototype.

# Tok Mart / Gokil Kisah pitch prototype

This is a playable Unity mechanics prototype using free artist-made Kenney sprites. It follows the supplied GDD's store loop, Sam/Zack selection and four named encounters. It is intended to show mechanics for a pitch; it is not the finished mobile game.

## Play in your Unity project

The implementation is in `A:/Projects/Personal/TokTokMart/Assets/GokilKisah`. The asset archive is intended for this Unity 6 project with its existing URP 2D renderer. The Windows executable includes the renderer and runs independently.

1. Let Unity import and compile the new files.
2. Choose **Gokil Kisah > Open Pitch Prototype** from Unity's menu.
3. Press Play and select Sam or Zack.
4. Move with WASD, arrows, or the touch pad at the lower left. Use E, Space, or the yellow ACTION button.
5. Customers arrive automatically. Keep shelves stocked and wait for a shopper at checkout before scanning.
6. Use **Settings > Gameplay** to select an encounter. **Settings > Pitch tools** starts guided replays or enables the normally hidden replay button.

**Settings > Pitch tools > Customer journey** shows the new shopping loop. See CUSTOMER-UPDATE.md for customer behaviour and the editable shop scene.

You can also launch `TokMartPrototype/TokMart.exe`. Keep the executable alongside its data folder and DLLs.

## Editing the shop

The Shop hierarchy contains the playable world. Shelf shopping points and narrow restock zones follow their parent objects. Nearby spills take priority over restocking, including while carrying a crate. See EDITING-GUIDE.md.

## Mechanics

| Demonstration | Player action | Outcome |
|---|---|---|
| Customer journey | Watch entering and browsing; serve the first waiting shopper; clean customer-caused spills | Real stock is collected into baskets. Customers queue, pay and return through the door. |
| Store tasks | Collect a crate at the stockroom; time the restock check; complete three checkout scans; pass the mop check | Stock replenished, customer served and spill removed. The unchosen character navigates the shop, stocks and serves customers. |
| Goblins / Uncle Ray | Reach the phone, call Ray, wait for his arrival | Goblins remain during the wait; Ray arrives and collects them. Their spills remain to be cleaned. |
| K-Kung | Search for the relocated switch and activate fluorescent lights | The possessed idols disappear. The switch changes position on the next encounter. |
| Miss Salmah | Collect the nail gun and move behind her, within range | Automatic nail-gun shots only damage her from behind. Front attacks do not work. |
| Bo and gang | Find the relocated poo-filled Ikan Iblis and kick it before the countdown ends | The can hits Bo's car and stops the encounter. Expiry causes instant game over. |

## GDD fidelity and prototype decisions

The supplied GDD gives the core actions and encounter counters but leaves the game title, detailed controls, win conditions, general loss conditions, HUD, numerical balance, game pillars and selling points incomplete. The prototype therefore introduces only demonstration settings:

- Top-down shop layout with six central shelves and a checkout on the right, based on the supplied sketch.
- Landscape layout with keyboard controls and optional touch controls in Settings. This is a desktop pitch build, not a tested Android/iOS release.
- A five-second Uncle Ray wait, an eighteen-second Bo countdown, three automatic nail-gun hits and a limited firing range.
- A reusable timing bar: one success for stocking, three scans for checkout, one success for mopping. These rules illustrate the specified skill-check minigames; their exact designs are not specified by the GDD.
- A disruption meter for visual feedback only. It does not invent an additional loss condition.
- Replays and tabs are presentation tools. They do not claim to be the final progression or encounter schedule.
- The counter-can automatically aims at Bo's car after the player reaches and kicks it.
- Human-composed shop music, artist-made action sounds, seamless floors and foot-position depth ordering are included. See POLISH-UPDATE.md for controls and settings.
- World characters, shelves, counters, floor, walls, street, car, props and weapon graphics now use artist-made Kenney packs under CC0. Walking uses the supplied directional character poses. These are temporary stand-ins for the IP designs, and generic props serve the labeled GDD roles. See ASSET-CREDITS.md and the original licenses. No generative image tools were used.

The specific counter rules take priority over the broad automatic-attack description: goblins require Ray, K-Kung requires fluorescent lights, Miss Salmah requires rear nail-gun hits, and Bo requires the counter-can.

The sample scene and existing project assets are preserved. No full campaign, upgrades, economy, monetization, story scenes or final IP character art have been invented.

## Pitch media

The MP4 recordings and animated GIFs show actual Unity gameplay with scripted player movement and captions. The replays use the same interaction, collision, skill-check and enemy-counter rules as manual play. Storyboard sheets present the mechanics step by step.

See `Verification.txt` for gameplay checks and the build result.

