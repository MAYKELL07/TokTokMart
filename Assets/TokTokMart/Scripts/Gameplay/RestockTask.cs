using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TokTokMart
{
    public sealed partial class ShiftController
    {
        [SerializeField] RestockScreen restockPanel;
        bool restockViewNeedsReset;
        readonly int[] restockPackBySlot = new int[RestockScreen.MaxPacks];
        int packsToPlace, restockFirstSlot, restockRows, usedRestockPacks;

        static int RestockRowsForShift(int shift) => shift >= 10 ? 3 : shift >= 5 ? 2 : 1;

        void PrepareRestock()
        {
            restockFirstSlot = shelfStock[skillTarget];
            restockRows = RestockRowsForShift(campaignActive ? CurrentLevel : 1);
            packsToPlace = restockRows * RestockScreen.PacksPerRow;
            usedRestockPacks = 0;
            Array.Fill(restockPackBySlot, -1);
            restockViewNeedsReset = true;
        }

        void RefreshRestockPanel()
        {
            bool visible = skillType == 0 && selected && !mainMenu && !settingsOpen && !autoPaused &&
                !levelResults && !shiftBriefOpen && !scoreGuideOpen && !gameOver;
            if (!visible)
            {
                if (restockPanel && restockPanel.gameObject.activeSelf) restockPanel.Close();
                return;
            }
            if (!restockPanel) return;
            if (restockViewNeedsReset)
            {
                restockPanel.Open(skillTarget, restockFirstSlot, restockRows, true);
                for (int slot = 0; slot < packsToPlace; slot++)
                    if (restockPackBySlot[slot] >= 0) restockPanel.ShowPlacement(restockPackBySlot[slot], slot);
                restockPanel.SetProgress(taskCombo, packsToPlace);
                restockViewNeedsReset = false;
            }
            else if (!restockPanel.gameObject.activeSelf) restockPanel.Resume();
        }

        // Scripted replays use the same validation as drag-and-drop.
        bool PlaceRestockPack()
        {
            int pack = 0, slot = 0;
            while (pack < packsToPlace && (usedRestockPacks & (1 << pack)) != 0) pack++;
            while (slot < packsToPlace && restockPackBySlot[slot] >= 0) slot++;
            return TryPlaceRestockPack(pack, slot);
        }

        bool TryPlaceRestockPack(int pack, int slot)
        {
            if (skillType != 0 || !carryingCrate || pack < 0 || pack >= packsToPlace ||
                slot < 0 || slot >= packsToPlace || restockPackBySlot[slot] >= 0 ||
                (usedRestockPacks & (1 << pack)) != 0) return false;

            usedRestockPacks |= 1 << pack;
            restockPackBySlot[slot] = pack;
            taskCombo++;
            skillScans = taskCombo;
            PlaySound("Stock");
            if (restockPanel && !restockViewNeedsReset)
            {
                restockPanel.ShowPlacement(pack, slot);
                restockPanel.SetProgress(taskCombo, packsToPlace);
            }
            if (taskCombo < packsToPlace) return true;

            shelfStock[skillTarget] = ShelfCapacity(skillTarget);
            carryingCrate = false;
            stockActions++;
            SetStep("03 / CHECKOUT", "Add up the customer's receipt at the till.");
            FinishStoreTask("Shelf restocked");
            if (restockPanel) restockPanel.Close();
            return true;
        }

        void CancelCurrentTask()
        {
            skillType = -1;
            taskCombo = 0;
            playerVelocity = Vector2.zero;
            Toast("Task cancelled");
        }

        void FinishStoreTask(string feedback)
        {
            skillType = -1;
            chaos = Mathf.Max(0, chaos - (campaignActive && Targets.focus == ShiftFocus.Chaos ? 12 : 6));
            Toast(feedback);
            DeviceVibration.Pulse(35);
            EnsureShiftCleanupWork();
        }
    }
}
