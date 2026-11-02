# Customer and scene update

Open **Gokil Kisah > Open Pitch Prototype**. The Scene view now contains a saved shop layout: floor, walls, entrance, six shelves, register, stockroom, phone, starting characters and initial hazard.

The saved Shop hierarchy is now the playable world. Move shelves, furniture, character artwork, route markers and hazards directly in Unity. Their scene positions drive collision, customer movement and interaction. See EDITING-GUIDE.md for the components and editable settings.

## Actual shopper behaviour

- Customers enter through the door, choose stocked shelves and prefer less crowded shelves.
- Each shopper collects one to three items into a basket. Shelf stock decreases at pickup, not again at payment.
- An empty shelf makes them choose another. A sold-out store makes customers with empty baskets leave.
- Customers navigate furniture, other shoppers and spills. They slow down if close to a hazard and yield when routes are occupied.
- Customers queue at checkout in arrival order. The player completes three timing scans; the AI partner can serve a waiting shopper too. Checkout cannot create a sale without an actual customer at the register.
- Paid shoppers walk to the door and leave. The next customer advances.
- Every third shopper is clumsy and can drop items during shopping, creating a spill at their actual position. Each shopper creates at most one spill and the shop caps active hazards at six.
- Miss Salmah can frighten a real nearby shopper into dropping an item. The decorative victim has been removed. Goblin disruptions remain separate.
- Walking uses the existing human-made directional character poses and foot-position draw order.

These behaviours extend the prototype at your request; customer basket sizes, arrival timings, crowd limits and mishap frequency are tuning decisions rather than claimed GDD specifications.

## Pitching

**Settings > Pitch tools > Customer journey** demonstrates entering, browsing, spills, checkout and leaving. Pitch controls remain hidden by default. The new customer recording and four-step storyboard show actual Unity gameplay. The other five mechanic recordings have also been refreshed.

The Windows executable and Unity asset archive contain the update. Human-made art, sound and music remain unchanged, with their credits and licenses included.
