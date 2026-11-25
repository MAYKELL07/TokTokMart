using System;
using UnityEngine;

namespace TokTokMart
{
    public sealed partial class ShiftController
    {
        enum PartnerTask { Idle, StepAside, Checkout, Mop, CollectCrate, Restock, PickUp }

        const float PartnerSpeed = 110f;
        const float PartnerCheckoutSeconds = 5.5f;
        const float PartnerTaskDistance = 8f;
        PartnerTask partnerTask;
        Vector2 partnerGoal, unreachablePartnerGoal;
        ShelfInventory partnerShelf;
        Shopper partnerCustomer;
        DroppedStock partnerDroppedItem;
        int registerArrivals;
        float partnerWorkTime, partnerStalled, partnerSearchIn, partnerBreak, partnerAvoidFor;

        void ResetPartner()
        {
            ClearPartnerTask();
            registerArrivals = 0;
            partnerBreak = partnerSearchIn = partnerAvoidFor = 0;
        }

        void ClearPartnerTask()
        {
            partnerTask = PartnerTask.Idle;
            partnerShelf = null;
            partnerCustomer = null;
            partnerDroppedItem = null;
            partnerWorkTime = partnerStalled = 0;
            aiStatus = "Looking for side jobs";
        }

        bool PlayerAtRegister => StationInReach(shop.register) || skillType == 1;

        Vector2 PartnerTill => shop.partnerCheckoutSpot ? ShopLayout.ToGame(shop.partnerCheckoutSpot.position) : checkout;

        Vector2 PartnerStandbySpot => checkout + new Vector2(-145, 65);

        bool PartnerCanRestock()
        {
            return campaignActive && Targets.focus != ShiftFocus.Restock &&
                shop.stockroom && shop.stockroom.isActiveAndEnabled;
        }

        bool PartnerCanMop()
        {
            return campaignActive && Targets.focus != ShiftFocus.Cleanup;
        }

        bool CanPartnerServe(Shopper shopper)
        {
            if (shopper == null || shopper.registerTurn == 0 || PlayerAtRegister) return false;
            int interval = campaignActive && Targets.focus == ShiftFocus.Rush ? 8 : 5;
            return shopper.registerTurn % interval == 0;
        }

        void UpdateAI(float dt)
        {
            if (!sceneInitialized || dt <= 0) return;
            var shopper = CustomerAtRegister();
            if (shopper != null && shopper.registerTurn == 0)
                shopper.registerTurn = ++registerArrivals;

            partnerSearchIn -= dt;
            partnerBreak -= dt;
            partnerAvoidFor -= dt;

            if (!PartnerTaskAvailable(shopper)) ClearPartnerTask();

            if ((partnerTask == PartnerTask.Idle || partnerTask == PartnerTask.StepAside) && partnerSearchIn <= 0)
            {
                partnerSearchIn = .4f;
                if (partnerBreak <= 0) FindPartnerTask(shopper);
                if (partnerTask == PartnerTask.Idle && (helper - PartnerTill).sqrMagnitude < 145 * 145)
                    TryStartPartnerTask(PartnerTask.StepAside, PartnerStandbySpot);
            }
            if (partnerTask == PartnerTask.Idle) return;

            // Navigate to current task goal
            if ((helper - partnerGoal).sqrMagnitude > PartnerTaskDistance * PartnerTaskDistance)
            {
                var next = Navigate(helper, partnerGoal, PartnerSpeed * dt);
                partnerStalled = (next - helper).sqrMagnitude > .0001f ? 0 : partnerStalled + dt;
                helper = next;
                if (partnerStalled >= 1.5f)
                {
                    unreachablePartnerGoal = partnerGoal;
                    partnerAvoidFor = 8f;
                    ClearPartnerTask();
                }
                return;
            }

            partnerWorkTime += dt;
            switch (partnerTask)
            {
                case PartnerTask.StepAside:
                    ClearPartnerTask();
                    return;
                case PartnerTask.CollectCrate:
                    if (partnerWorkTime < .85f) return;
                    int shelf = Array.IndexOf(sceneShelves, partnerShelf);
                    if (TryStartPartnerTask(PartnerTask.Restock, ShelfPoint(shelf))) PlaySound("Pickup", .45f);
                    else ClearPartnerTask();
                    return;
                case PartnerTask.Checkout:
                    if (partnerWorkTime < PartnerCheckoutSeconds) return;
                    Speak(shopper, "Okay boss, I got this one.");
                    ServeCustomer();
                    partnerBreak = 3f;
                    break;
                case PartnerTask.Mop:
                    if (partnerWorkTime < 3.4f) return;
                    if (ClearMopPatchAt(partnerGoal))
                    {
                        hazardsCleared++;
                        PlaySound("Mop", .55f);
                        Toast("Partner cleaned a spill");
                    }
                    partnerBreak = 2f;
                    break;
                case PartnerTask.Restock:
                    if (partnerWorkTime < 4f) return;
                    int index = Array.IndexOf(sceneShelves, partnerShelf);
                    shelfStock[index] = ShelfCapacity(index);
                    stockActions++;
                    PlaySound("Stock", .55f);
                    Toast("Partner restocked a shelf");
                    partnerBreak = 3f;
                    break;
                case PartnerTask.PickUp:
                    if (partnerWorkTime < .85f) return;
                    PickUpDroppedItem(droppedStock.IndexOf(partnerDroppedItem), byPartner: true);
                    partnerBreak = 1f;
                    break;
            }
            ClearPartnerTask();
        }

        bool PartnerTaskAvailable(Shopper shopper)
        {
            switch (partnerTask)
            {
                case PartnerTask.Checkout:
                    return shopper != null && shopper == partnerCustomer && CanPartnerServe(shopper) &&
                        CustomerPatience - shopper.timer >= PartnerCheckoutSeconds - partnerWorkTime;
                case PartnerTask.CollectCrate:
                case PartnerTask.Restock:
                    return PartnerCanRestock() && PartnerShelfNeedsStock(Array.IndexOf(sceneShelves, partnerShelf));
                case PartnerTask.Mop:
                    return PartnerCanMop() && !PlayerMopping(partnerGoal) && spills.Contains(partnerGoal);
                case PartnerTask.PickUp:
                    return droppedStock.Contains(partnerDroppedItem);
                default:
                    return true;
            }
        }

        void FindPartnerTask(Shopper shopper)
        {
            var focus = campaignActive ? Targets.focus : ShiftFocus.Warmup;

            // 1. Pick up any dropped/fallen items
            foreach (var item in droppedStock)
            {
                if (!TryStartPartnerTask(PartnerTask.PickUp, item.position)) continue;
                partnerDroppedItem = item;
                return;
            }

            // 2. If the level is about restocking, partner prioritizes other tasks (checkout & spills)
            // and DOES NOT restock!
            if (focus == ShiftFocus.Restock)
            {
                if (shopper != null && CanPartnerServe(shopper) &&
                    (CustomerPatience - shopper.timer >= PartnerCheckoutSeconds + Vector2.Distance(helper, PartnerTill) / PartnerSpeed) &&
                    (TryStartPartnerTask(PartnerTask.Checkout, PartnerTill) || TryStartPartnerTask(PartnerTask.Checkout, checkout)))
                {
                    partnerCustomer = shopper;
                    return;
                }
                if (PartnerCanMop() && FindPartnerSpill()) return;
            }
            else
            {
                // In other levels (especially Rush / customer levels):
                // Prioritize cleaning and restocking side jobs so partner does cashier less!
                if (PartnerCanMop() && hazardsCleared < CleanGoal && FindPartnerSpill()) return;
                if (PartnerCanRestock() && FindPartnerShelf()) return;

                if (shopper != null && CanPartnerServe(shopper) &&
                    (CustomerPatience - shopper.timer >= PartnerCheckoutSeconds + Vector2.Distance(helper, PartnerTill) / PartnerSpeed) &&
                    (TryStartPartnerTask(PartnerTask.Checkout, PartnerTill) || TryStartPartnerTask(PartnerTask.Checkout, checkout)))
                {
                    partnerCustomer = shopper;
                    return;
                }

                if (PartnerCanMop() && FindPartnerSpill()) return;
                if (PartnerCanRestock() && FindPartnerShelf()) return;
            }
        }

        bool PlayerMopping(Vector2 position)
        {
            return skillType == 2 && (position - mopTargetPosition).sqrMagnitude <= CleanPatchRadius * CleanPatchRadius;
        }

        bool FindPartnerSpill()
        {
            Vector2 bestSpill = Vector2.zero;
            float nearest = float.PositiveInfinity;
            bool found = false;
            foreach (var spill in spills)
            {
                float distance = (helper - spill).sqrMagnitude;
                if (distance >= nearest || PlayerMopping(spill) || !PartnerCanReach(spill)) continue;
                nearest = distance;
                bestSpill = spill;
                found = true;
            }
            return found && TryStartPartnerTask(PartnerTask.Mop, bestSpill);
        }

        bool PartnerShelfNeedsStock(int shelf)
        {
            return shelf >= 0 && (skillType != 0 || shelf != skillTarget) &&
                shelfStock[shelf] < ShelfCapacity(shelf) && shelfStock[shelf] <= Mathf.Max(1, ShelfCapacity(shelf) / 4);
        }

        bool FindPartnerShelf()
        {
            int bestShelf = -1;
            float nearest = float.PositiveInfinity;
            for (int i = 0; i < sceneShelves.Length; i++)
            {
                if (!PartnerShelfNeedsStock(i)) continue;
                var point = ShelfPoint(i);
                float distance = (supply - point).sqrMagnitude;
                if (distance >= nearest || !PartnerCanReach(point)) continue;
                bestShelf = i;
                nearest = distance;
            }
            if (bestShelf < 0 || !TryStartPartnerTask(PartnerTask.CollectCrate, supply)) return false;
            partnerShelf = sceneShelves[bestShelf];
            return true;
        }

        bool TryStartPartnerTask(PartnerTask task, Vector2 goal)
        {
            if (!PartnerCanReach(goal)) return false;
            partnerTask = task;
            partnerGoal = goal;
            partnerWorkTime = partnerStalled = 0;
            aiStatus = task switch
            {
                PartnerTask.Checkout => "Helping at checkout",
                PartnerTask.Mop => "Mopping a spill",
                PartnerTask.CollectCrate => "Collecting a stock crate",
                PartnerTask.Restock => "Restocking a shelf",
                PartnerTask.PickUp => "Picking up fallen stock",
                _ => "Making room at checkout"
            };
            return true;
        }

        bool PartnerCanReach(Vector2 goal)
        {
            if (partnerAvoidFor > 0 && (goal - unreachablePartnerGoal).sqrMagnitude <= 10 * 10) return false;
            if (StaticSegmentClear(helper, goal)) return true;
            if (!SceneWalkable(goal)) return false;
            EnsureNavigationGrid();
            int start = ConnectedNavigationNode(helper), end = ConnectedNavigationNode(goal);
            return start >= 0 && end >= 0 && FlowTo(end)[start] >= 0;
        }

        void ShowPartnerCrate()
        {
            var partner = shop.partnerCharacter;
            bool carrying = partnerTask == PartnerTask.Restock;
            if (partner.heldStock && partner.heldStock.gameObject.activeSelf != carrying)
                partner.heldStock.gameObject.SetActive(carrying);
        }
    }
}
