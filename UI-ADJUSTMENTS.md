# Manually editing TokTokMart UI

The active scene now uses the TokMartEditableUI Canvas prefab in Assets/GokilKisah/Resources. The original IMGUI drawing code is retained as a fallback; the editable Canvas is the default screen renderer. Existing gameplay actions remain connected through a binding layer. World-space shop labels stay with the world renderer.

## Edit a screen
1. Stop Play mode.
2. Select TokMartEditableUI in the Hierarchy. Scroll to the Tok Mart Canvas component and use its screen preview buttons.
3. Expand Design (1600 x 900), then the screen group. Groups are named for their screen, button label, text, or artwork.
4. Move an entire UiButton group to keep its artwork, label and hit area together. Move the Joystick, Sprint or Interact groups for touch controls.
5. Edit Text components for wording, font, size, alignment and color. Edit Image/RawImage components for artwork and color. Use RectTransforms for placement and dimensions; use group Scale to resize a composite control.
6. Save the scene. Use the prefab Overrides menu to apply changes to the shared prefab, or open the prefab directly to edit it.

Other screens are inactive until gameplay needs them. The preview buttons only control edit-mode visibility; gameplay chooses the active screen automatically. Edit outside Play mode so changes persist.

## Data bindings
Tok Mart Canvas Element keeps the connection to game data. Leave Bind Game Data enabled for counters, timers, receipt values and feedback. Static text and artwork can be edited directly. Disable Bind Game Data only when you intentionally want a fixed authored value. Leave Animate Layout enabled on progress bars, joystick knobs and task timing markers; disable it to freeze a visual at its authored location.

The gameplay controller still controls visibility and sibling draw order. Move or resize existing elements without deleting their binding components, changing hidden keys, or reparenting bound elements. Add decorative objects freely within a screen group. New gameplay actions still need a code connection.

## Mobile sizing
Store Game Controller > Mobile UI sizing retains Mobile Hud Scale and Default Touch Scale. In-game Settings > Controls retains player control size/position sliders. Moving the authored mobile control groups moves their touch hit areas too.

## Recovery and validation
Store Game Controller > Editable Canvas UI > Use Original Imgui is an emergency visual fallback. Keep it off for the editable Canvas.
Original scripts and scene are backed up under UI-Backups. Verification/EditableCanvasFinal contains original-versus-Canvas captures and button, slider, character selection and mobile hit-area check results. This is a retained Canvas renderer with a compatibility binding pass, not a rewrite of gameplay or the world renderer.
