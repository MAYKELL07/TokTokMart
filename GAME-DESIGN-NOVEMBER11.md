# TokTokMart — November 11 gameplay update

The active scene is `Assets/GokilKisah/Scenes/TokMartPitch.unity`. The editable Canvas remains the default UI. The backup before these changes is under `UI-Backups/Before-Nov11-Gameplay-*`.

## Shift loop

Choose a shift from the main menu's level map, pick Sam or Zack, read the targets and star thresholds, then open the shop. Collect stock from the storage box, restock shelves with timing checks, clean spills, and serve customer receipts. The partner covers checkout while required player restocking remains. Partner sales count toward the shared customer target. Every fifth shift also requires defeating its boss. Completed shifts unlock the next shift. Returning bosses are limited to milestones earlier than the selected shift, so later unlocks cannot make an opening-shift replay harder. Earlier shifts can be replayed without losing later unlocks; best score and stars never decrease.

## Score and stars

Money remains a separate shop accounting value. Stars use fixed points, so random basket contents do not determine the rating.

| Element | Points |
|---|---:|
| Customer served, capped at the goal | 100 |
| Shelf restocked, capped at the goal | 150 |
| Spill cleaned, capped at the goal | 100 |
| Required milestone boss defeated | 200 |
| Accuracy bonus | 200 minus 25 per wrong total or timing miss, minus 100 per lost customer; minimum 0 |
| Speed bonus | 200 if finished within 70% of the budget, 100 within 85%, otherwise 0 |
| Shop control bonus | 100 at 0–15% final chaos, 50 above 15% through 40%, otherwise 0 |

All targets and any required boss must be cleared to earn stars. One star requires completion (objective points); two stars require objective points +300; three stars require objective points +450. Perfect is objective points +500. Speed and chaos boundary values are inclusive. Extra completed tasks do not farm rating points. Failed shifts earn no stars and no completion bonuses.

Shift one: 3 customers, 1 restock, 1 cleanup. Thresholds: 550 / 850 / 1000. Perfect: 1050. Its budget is 172 seconds; finish in 120.4 seconds or less for the full speed bonus. The briefing, map, HUD score help, and results all use the same score calculation.

## Feedback and balance

- Skill misses retain the current sequence and show direction plus their exact accuracy cost. Repeated presses are debounced.
- Restocking has a 20-second task deadline, cleaning 24 seconds, checkout 30 seconds. Expiry cancels the attempt, keeps world objects available, and costs 25 accuracy points.
- Customer patience continues during checkout. An expired customer leaves, closes a stale receipt, and costs 100 accuracy points.
- Queue customers show shrinking green/yellow/red patience bars. The HUD also shows the shortest patience remaining.
- The final 30 seconds and final 10 seconds have visible alerts. Closing/task/boss countdowns tick audibly in their final seconds.
- Completing stock or cleanup reduces chaos by 6; serving reduces it by 2; defeating a boss reduces it by 35.
- Timing difficulty caps at 1.35×, customer patience has a 45-second floor, and the shift budget is 120 seconds plus estimated required workload.
- The scoring guide pauses gameplay; closing it safely releases held controls.

## Supplied artwork

Bigbesty is the runtime and authored Canvas typeface. Updated task cards, joystick, interact/sprint controls, timer, counters, pause icon, logo, background, and star are imported from the supplied November 11 folder. New counter and storage-box sprites replace the placeholder art while retaining interaction points and collision geometry. Source exports are preserved in ArtSources; optimized UI images live in Resources/NovemberArt/UI. No synced ChatGPT-project source files are edited.

## Verification

`november11verify` through the existing editor command bridge exercises score boundaries, reachable perfect score, anti-farming, failure feedback, patience expiry, boss gating, best-score preservation, unlock preservation, updated artwork/font, and navigation routes. Captures and results are stored in `Verification/November11`. The verification restores any campaign save keys it touches.

Validated results: Gameplay and scoring: 57 passed; 0 failed. Canvas UI: 10 passed; 0 failed. Touch input and art: 60 passed; 0 failed. Complete playthroughs: 2 passed; 0 failed.

Live full-shift playthroughs completed shift 1 in 79.0/172 seconds with 1050/1050 points, and boss shift 5 in 161.5/232 seconds with 1900/1900 points. Both earned three stars. These verification sessions do not write campaign progress.
