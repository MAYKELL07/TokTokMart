# Editing the playable shop

Open Assets/GokilKisah/Scenes/TokMartPitch.unity. The **Shop** hierarchy now contains the actual playable world. Its objects remain active in Play mode. There is no separate preview or hidden replacement shop.

## Move and customize objects

Use Unity's normal move, rotate and scale tools on the objects under Shop. Floors, walls, furniture, street art and props are normal SpriteRenderers. Their saved transforms, sprites, colors and materials are used by the game.

Move a whole shelf using its **Shelf 1**, **Shelf 2**, etc. parent. The artwork, feet collision, shopping point and restock zone move together. You can duplicate a shelf under Shop; the game discovers it automatically. Disabling a shelf removes it from routes and interactions.

On **ShelfInventory**, edit Initial Stock, Capacity, Stocked Sprite and Empty Sprite. These control how the shelf starts and how inventory appears. Capacity should match the number of artwork slots you want to display. Inventory changes made by shoppers or restocking are runtime state.

Each shelf has two separate shapes:
- **Foot collision reference**: resize this BoxCollider2D to fit the solid furniture base.
- **Restock interaction zone**: a narrow trigger along the shelf front. Move or resize this child to control exactly where the player can restock. It no longer claims a large circular part of the aisle.

A nearby spill takes priority over stocking or checkout, including while the player carries a crate. Cleaning does not consume the crate.

## Stations and customers

Move **Stockroom**, **Checkout station** or **Wall phone station** as whole parents. Artwork and their interaction positions follow the transforms.

The checkout contains **Queue front**, **Partner checkout spot** and **Approach point** markers. Move these to fit your edited counter. Queue Spacing is exposed on ShopLayout.

Under **Customer routes**, move **Door** and **Inside entrance**. Shoppers use these positions to enter and leave. Shelf Shopping Point children control browsing destinations.

Under **Encounter locations**, edit the switch/can location markers and enemy start markers. Randomized encounter props use these authored locations rather than fixed coordinates in code.

## Characters and spills

Move **Player** or **Partner** before pressing Play to change their starting positions. Their artwork is a child, so you can adjust its scale, color and material independently. CharacterSpriteAnimator exposes the atlas, skin row, directional columns and animation speed. Turn Animate off to use your own SpriteRenderer sprite.

**Initial mop hazard** is a real SpillHazard scene object. Move or duplicate it to change starting hazards.

Under **Spawn templates**, customize Customer template, Spill template and the enemy templates. This group stays inactive because these are templates; live shoppers and new spills appear under **Runtime customers and hazards**.

For a new solid prop, add a SpriteRenderer and a **non-trigger Collider2D** under Shop. The game uses the collider for navigation and player collision. Decorative sprites without a solid collider remain walkable.

Edit **Walkable floor area** to change the allowed shop bounds. Ensure door, shelf approach points and queue markers are inside the allowed area and clear of solid furniture.

## Camera and saving

Shop Camera renders the real scene. On ShopLayout, turn **Frame Camera To Interface** off to use your own camera position and orthographic zoom. World labels follow the camera.

Save the scene normally. Opening it and building the prototype preserve your edits. Setup is a one-time migration and does not reconstruct the shop on every open.

Edit-mode changes persist when you enter Play. Unity's normal rule still applies: changes made only during Play mode are reverted when you stop.

The menu/HUD remains controlled by StoreInterface; this update replaces the world renderer and authoring workflow, not the existing settings menu with a new Canvas.

## Script names

- StoreGameController: shift state, player actions and encounter rules.
- CustomerSimulation: shopper baskets, shelf choices, movement and queues.
- SceneBindings: authored scene positions, collision shapes and runtime entities.
- ShelfInventory, ShopStation, SpillHazard, ShopObstacle: editable world components.
- CharacterSpriteAnimator and FootDepthSorter: walking poses and front/behind layering.
- ShopLayout: camera, route, station, template and encounter references.
- StoreInterface and EncounterPresentation: HUD, settings and world information.
- PitchEditorTools, ShopSceneBuilder and ArtistAssetImporter: editor tools.
- GameplayVerification, InterfaceCapture and CustomerReplayCapture: checks and pitching tools.

All script file renames preserve their Unity metadata IDs, so saved scene references remain connected.

