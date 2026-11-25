using System.Collections.Generic;
using UnityEngine;

namespace TokTokMart
{
    public sealed partial class ShiftController
    {
        enum ShopperState { Entering, Browsing, Picking, Queuing, Leaving }
        sealed class Shopper
        {
            public int id, shelf = -1, basket, wanted, facing = 25, visits, registerTurn;
            public readonly List<int> items = new List<int>();
            public Vector2 position, goal, motion;
            public ShopperState state;
            public float timer, scared, blocked, slipCooldown, speechUntil, yieldUntil;
            public bool clumsy, spilled, droppedItem, exitingRegister, angry;
            public string speech;
            public Vector2 detour;
            public float detourTime;
        }
        readonly List<Shopper> customers = new List<Shopper>();
        readonly List<Shopper> customerQueue = new List<Shopper>();
        Vector2 ShopDoor => ShopLayout.ToGame(shop.customerDoor.position);
        Shopper routingShopper;
        int nextCustomer;
        float customerArrival;
        int customerSpills, completedShoppingTrips;
        EntranceDoor entrancePresentation;
        public bool CustomerRequestsEntrance
        {
            get
            {
                if (!selected || mainMenu || !shop) return false;
                foreach (var c in customers) if ((c.state == ShopperState.Entering || c.state == ShopperState.Leaving) && Vector2.Distance(c.position, ShopDoor) < 165) return true;
                return false;
            }
        }
        float CustomerFrustration(Shopper c) => c.angry ? 1 : c.state == ShopperState.Queuing ? Mathf.Clamp01(c.timer / CustomerPatience) : 0;
        void ResetCustomers()
        {
            customers.Clear(); customerQueue.Clear(); nextCustomer = 0;
            customerArrival = 4; customerSpills = 0; completedShoppingTrips = 0;
            if (!entrancePresentation && shop) entrancePresentation = shop.GetComponentInChildren<EntranceDoor>();
            if (entrancePresentation) entrancePresentation.Tick(false, 2);
        }
        Shopper SpawnCustomer()
        {
            var c = new Shopper
            {
                id = ++nextCustomer,
                position = ShopDoor,
                goal = ShopLayout.ToGame(shop.customerEntryTarget.position),
                wanted = 1 + ((nextCustomer - 1) % MaxBasketItems),
                clumsy = campaignActive ? nextCustomer % Targets.SpillEvery == 1 : nextCustomer % 3 == 1,
                state = ShopperState.Entering
            };
            customers.Add(c); return c;
        }
        void ChooseShelf(Shopper c)
        {
            int best = -1; float score = float.MaxValue;
            for (int i = 0; i < shelves.Count; i++)
            {
                if (shelfStock[i] <= 0) continue;
                int crowd = 0; foreach (var other in customers) if (other != c && other.shelf == i && other.state != ShopperState.Leaving) crowd++;
                float value = Vector2.Distance(c.position, ShelfPoint(i)) + crowd * 220;
                // Rotating preferences keep everyone from choosing the same closest shelf.
                value += ((i + c.id) % 6) * 26;
                if (value < score) { score = value; best = i; }
            }
            if (best < 0 || c.basket >= c.wanted || c.visits >= MaxBasketItems)
            { JoinQueueOrLeave(c); return; }
            c.shelf = best;
            Vector2 point = ShelfPoint(best);
            var offset = Vector2.right * ((c.id % 2 == 0 ? 1 : -1) * 27);
            c.goal = StaticSegmentClear(point, point + offset) ? point + offset : point;
            c.state = ShopperState.Browsing; c.timer = 0;
        }
        void JoinQueueOrLeave(Shopper c)
        {
            c.shelf = -1; c.timer = 0;
            if (c.basket == 0) { c.state = ShopperState.Leaving; c.goal = ShopDoor; }
            else { c.state = ShopperState.Queuing; customerQueue.Add(c); }
        }
        void LeaveRegister(Shopper c)
        {
            customerQueue.Remove(c); c.state = ShopperState.Leaving; c.timer = 0; c.blocked = 0;
            c.exitingRegister = Vector2.Distance(c.position, QueuePoint(0)) < 38;
            c.goal = c.exitingRegister ? QueuePoint(0) + Vector2.left * 45 : ShopDoor;
            c.detourTime = 0;
        }
        Vector2 QueuePoint(int index) => ShopLayout.ToGame(shop.queueFront.position + (Vector3)(shop.queueSpacing * index));
        Shopper CustomerAtRegister()
        {
            if (customerQueue.Count == 0) return null;
            var c = customerQueue[0];
            return Vector2.Distance(c.position, QueuePoint(0)) < 12 ? c : null;
        }
        void CustomerSpill(Shopper c)
        {
            if (c.spilled) return;
            // A mishap is attached to the shopper's actual position, never a timer-spawned random puddle.
            if (!TrySpawnSpill(c.position, 6)) return;
            c.spilled = true; customerSpills++; c.scared = 1.4f; c.slipCooldown = 3;
            PlaySound("Splash");
            AddSplash(c.position);
            Speak(c, SpillLines[(c.id - 1) % SpillLines.Length]);
        }
        void ScareCustomer()
        {
            Shopper target = null; float nearest = 175;
            foreach (var c in customers)
            {
                if (c.state == ShopperState.Leaving || c.state == ShopperState.Queuing) continue;
                float d = Vector2.Distance(c.position, salmah);
                if (d < nearest) { nearest = d; target = c; }
            }
            if (target == null) return;
            customerHit = .5f; target.scared = 2; CustomerSpill(target);
            target.shelf = -1; ChooseShelf(target);
        }
        void UpdateCustomers(float dt)
        {
            if (!entrancePresentation) entrancePresentation = shop.GetComponentInChildren<EntranceDoor>();
            if (entrancePresentation) entrancePresentation.Tick(CustomerRequestsEntrance, dt);
            customerArrival -= dt;
            bool doorFree = true;
            foreach (var c in customers) if (Vector2.Distance(c.position, ShopDoor) < 65) doorFree = false;
            if (customerArrival <= 0 && customers.Count < CustomerLimit && doorFree)
            { SpawnCustomer(); customerArrival = CurrentArrivalInterval; }
            for (int n = customers.Count - 1; n >= 0; n--)
            {
                var c = customers[n]; c.motion = Vector2.zero; c.scared = Mathf.Max(0, c.scared - dt);
                c.slipCooldown -= dt;
                if (c.state == ShopperState.Queuing)
                {
                    c.timer += dt;
                    if (c.timer > CustomerPatience)
                    { c.angry = true; Speak(c, "Lambatnya! My queue number grew a beard."); LeaveRegister(c); missedCustomers++; chaos = Mathf.Min(100, chaos + 8); if (skillType == 1 && skillTarget == c.id) skillType = -1; Toast("Customer left · -100 pts"); PlaySound("Miss", .55f); }
                }
                if (c.state == ShopperState.Queuing) c.goal = QueuePoint(customerQueue.IndexOf(c));
                float remaining = Vector2.Distance(c.position, c.goal);
                if (c.state == ShopperState.Picking)
                {
                    c.timer += dt;
                    if (c.timer >= 1.1f + (c.id % 3 == 0 ? 2.3f : 0))
                    {
                        // Stock is removed when collected. Payment never takes stock a second time.
                        if (shelfStock[c.shelf] > 0)
                        {
                            int slot = --shelfStock[c.shelf];
                            c.items.Add(ShopProducts.ShelfProduct(c.shelf, slot));
                            c.basket++; c.visits++;
                        }
                        if (c.clumsy && c.basket > 0) CustomerSpill(c);
                        else if (c.visits == 1 && c.id % 4 == 2) TryDropShelfItem(c);
                        ChooseShelf(c);
                    }
                    continue;
                }
                if (remaining < 8)
                {
                    if (c.state == ShopperState.Entering) ChooseShelf(c);
                    else if (c.state == ShopperState.Browsing)
                    {
                        c.state = ShopperState.Picking; c.timer = 0;
                        c.facing = FaceColumn(ShopLayout.ToGame(sceneShelves[c.shelf].collisionBase.bounds.center) - c.position);
                    }
                    else if (c.state == ShopperState.Leaving)
                    { if (c.exitingRegister) { c.exitingRegister = false; c.goal = ShopDoor; } else { customers.RemoveAt(n); completedShoppingTrips++; } }
                    continue;
                }
                if (c.state == ShopperState.Browsing && shelfStock[c.shelf] == 0) { ChooseShelf(c); continue; }
                if (entrancePresentation && (c.state == ShopperState.Entering || c.state == ShopperState.Leaving) && Vector2.Distance(c.position, ShopDoor) < 100 && !entrancePresentation.IsPassable) continue;
                float speed = c.scared > 0 ? 130 : 100 + (c.id % 3) * 12;
                bool nearSpill = false;
                foreach (var spill in spills)
                {
                    if ((c.position - spill).sqrMagnitude > 46 * 46) continue;
                    nearSpill = true;
                    if (c.slipCooldown > 0 || (c.position - spill).sqrMagnitude > 33 * 33) continue;
                    c.slipCooldown = 12;
                    PlaySound("Splash", .65f);
                    AddSplash(c.position);
                    Speak(c, SlipLines[(c.id - 1) % SlipLines.Length]);
                }
                if (nearSpill) speed *= .55f;
                if (Time.time < c.yieldUntil && c.state != ShopperState.Queuing) speed *= .5f;
                routingShopper = c;
                c.detourTime -= dt;
                if (c.detourTime <= 0 && c.blocked > .18f && TryCustomerDetour(c, out var detour))
                { c.detour = detour; c.detourTime = .65f; }
                Vector2 routeGoal = c.detourTime > 0 && Vector2.Distance(c.position, c.detour) > 5 ? c.detour : c.goal;
                Vector2 candidate = Navigate(c.position, routeGoal, speed * dt);
                routingShopper = null;
                if (BlocksPlayerStep(c.position, candidate, player, 26)) candidate = c.position;
                // Let a paid customer clear the till before the next customer advances.
                if (c.state == ShopperState.Queuing) foreach (var departing in customers)
                        if (departing.exitingRegister && Vector2.Distance(departing.position, QueuePoint(0)) < 38) candidate = c.position;
                // Yield to a customer ahead. Try a walkable side step if an aisle is occupied.
                bool occupied = (candidate - c.position).sqrMagnitude < .0001f && remaining > 8;
                foreach (var other in customers)
                {
                    if (other == c) continue;
                    if (Vector2.Distance(candidate, other.position) < 29 &&
                       Vector2.Distance(candidate, other.position) < Vector2.Distance(c.position, other.position) &&
                       (other.id < c.id || other.state == ShopperState.Queuing || other.state == ShopperState.Picking))
                    { occupied = true; break; }
                }
                if (occupied)
                {
                    c.blocked += dt; candidate = c.position;
                    if (c.state != ShopperState.Queuing && c.blocked > .6f && c.detourTime <= 0)
                    {
                        Vector2 direction = (c.goal - c.position).normalized;
                        Vector2 side = new Vector2(-direction.y, direction.x) * (c.id % 2 == 0 ? 1 : -1);
                        var alternate = c.position + side * speed * dt;
                        bool clear = LineClear(c.position, alternate);
                        foreach (var other in customers) if (other != c && Vector2.Distance(alternate, other.position) < 29) clear = false;
                        if (clear && !BlocksPlayerStep(c.position, alternate, player, 26)) candidate = alternate;
                    }
                }
                else c.blocked = 0;
                c.motion = candidate - c.position; c.position = candidate;
                if (c.motion.sqrMagnitude > .001f) c.facing = Facing(c.motion);
            }
        }
    }
}
