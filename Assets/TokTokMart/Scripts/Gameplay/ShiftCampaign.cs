using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TokTokMart
{
    public sealed partial class ShiftController
    {
        const string LevelSave = "TokMart.Campaign.NextLevel", BossSave = "TokMart.Campaign.Bosses", UnlockSave = "TokMart.Campaign.HighestUnlocked";
        public int CurrentLevel { get; private set; } = 1;
        public int UnlockedBossMask { get; private set; }
        public bool LevelResultsVisible => levelResults;
        public bool CampaignActive => campaignActive;
        bool campaignActive, campaignPersistence = true, levelResults, levelWon, bossActive, mandatoryBossPending;
        int salesCents, missedCustomers, priceMistakes, randomFights, invoiceStars;
        float encounterRollClock;
        string levelFailure = "";
        Texture2D invoiceStar;
        ShiftGoals Targets => new ShiftGoals(CurrentLevel);
        public int ServeGoal => Targets.serve;
        public int StockGoal => Targets.stock;
        public int CleanGoal => Targets.clean;
        public float ShiftBudget => Targets.duration;
        public float ArrivalInterval => Mathf.Max(2.2f, 4.5f / (1 + Mathf.Min(25, CurrentLevel - 1) * .035f)) * Targets.ArrivalScale;
        public float ShiftRush => Mathf.SmoothStep(0, 1, Mathf.Clamp01(shiftTime / (campaignActive ? ShiftBudget : 240f)));
        public float CurrentArrivalInterval => Mathf.Max(1.4f, (campaignActive ? ArrivalInterval : 4.5f) * Mathf.Lerp(2.6f, .65f, ShiftRush));
        public int CustomerLimit => Mathf.RoundToInt(Mathf.Lerp(campaignActive ? Mathf.Min(5, 2 + (CurrentLevel - 1) / 5) : 2, campaignActive ? Mathf.Min(10, 6 + (CurrentLevel - 1) / 3) : 6, ShiftRush));
        public float CustomerPatience => Mathf.Max(16, 42 / (1 + (CurrentLevel - 1) * .03f));
        public float TaskDifficulty => campaignActive ? Mathf.Min(1.35f, 1 + (CurrentLevel - 1) * .018f) : 1;
        public static Encounter MilestoneBoss(int level) => (Encounter)(1 + ((Mathf.Max(5, level) / 5 - 1) % 4));
        int SavedLevel => chosenLevel > 0 ? chosenLevel : Mathf.Max(1, PlayerPrefs.GetInt(LevelSave, 1));

        public void BeginCampaignLevel(int level, bool persist = true)
        {
            ResetRun(Encounter.Store);
            campaignActive = true; campaignPersistence = persist; levelResults = false; levelWon = false;
            selected = true; mainMenu = false; CurrentLevel = Mathf.Clamp(level, 1, 10000);
            customerArrival = Mathf.Max(1.5f, 4f / (1 + (CurrentLevel - 1) * .065f));
            UnlockedBossMask = PlayerPrefs.GetInt(BossSave, 0) & 15;
            mandatoryBossPending = CurrentLevel % 5 == 0; bossActive = false;
            salesCents = 0; missedCustomers = 0; priceMistakes = 0; timingMistakes = 0; randomFights = 0; levelFailure = ""; milestoneCleared = false; ResetShiftFeedback();
            encounterRollClock = mandatoryBossPending ? 5 : UnityEngine.Random.Range(24f, 42f);
            tuning.boDeadline = Mathf.Max(11, 21 - CurrentLevel * .2f);
            tuning.rayWait = Mathf.Min(15, 5 + CurrentLevel * .3f);
            tuning.nailHits = 3 + Mathf.Min(5, CurrentLevel / 10);
            EnsureShiftCleanupWork();
            if (Targets.focus == ShiftFocus.Restock) for (int i = 0; i < shelfStock.Length; i++) shelfStock[i] = Mathf.Min(shelfStock[i], 1);
            if (Targets.focus == ShiftFocus.Chaos) chaos = 18;
            notifiedShiftGoals = 0;
            shiftBriefOpen = true;
            if (persist) { PlayerPrefs.SetInt(UnlockSave, Mathf.Max(HighestUnlocked, CurrentLevel)); PlayerPrefs.SetInt(LevelSave, CurrentLevel); PlayerPrefs.Save(); }
            SetStep("LEVEL " + CurrentLevel, "Serve customers, restock shelves, and clear spills before closing.");
            if (!shiftBriefOpen) Toast(mandatoryBossPending ? "Boss shift: " + scenarioTitles[(int)MilestoneBoss(CurrentLevel)] : "Shop open");
            else noticeUntil = 0;
        }
        void SaveCampaignProgress(int nextLevel)
        {
            if (!campaignPersistence) return;
            PlayerPrefs.SetInt(UnlockSave, Mathf.Max(HighestUnlocked, nextLevel));
            PlayerPrefs.SetInt(LevelSave, nextLevel); PlayerPrefs.SetInt(BossSave, PlayerPrefs.GetInt(BossSave, 0) | UnlockedBossMask); PlayerPrefs.Save();
        }
        void StartCampaignEncounter(Encounter next)
        {
            if (next == Encounter.Store) return;
            encounter = next; resolved = false; bossActive = true; rayCalled = false; rayRemaining = 0;
            lightsOn = false; nailsHit = 0; nailCooldown = 0; hasGun = false; canFlying = false; canKicked = false;
            boRemaining = tuning.boDeadline; nextMess = 2; nextAttack = 1;
            goblins = next == Encounter.Goblins ? shop.goblinSpawns.Length : 0;
            rayX = rayStart.x; salmah = ShopLayout.ToGame(shop.salmahSpawn.position);
            switchIndex = (switchIndex + 1) % switches.Length; lightSwitch = switches[switchIndex];
            canIndex = (canIndex + 1) % cans.Length; can = cans[canIndex];
            ResetActiveEncounter(next);
            Toast(scenarioTitles[(int)next] + " arrived");
            PlaySound("Impact", .5f);
        }
        Encounter PickUnlockedBoss()
        {
            var pool = new List<Encounter>(); for (int i = 1; i <= 4; i++) if (i * 5 < CurrentLevel && (UnlockedBossMask & (1 << (i - 1))) != 0) pool.Add((Encounter)i);
            return pool.Count == 0 ? Encounter.Store : pool[UnityEngine.Random.Range(0, pool.Count)];
        }
        void TickCampaign(float dt)
        {
            if (!campaignActive || levelResults) return;
            NotifyShiftGoals();
            EnsureShiftCleanupWork();
            chaos = Mathf.Min(100, chaos + dt * (Mathf.Max(0, spills.Count - 1) * .025f + Mathf.Max(0, customerQueue.Count - 1) * .06f));
            if (Targets.focus == ShiftFocus.Chaos && !bossActive)
                chaos = Mathf.Min(100, chaos + dt * (.08f + spills.Count * .08f + Mathf.Max(0, customerQueue.Count - 1) * .06f));
            if (gameOver) { FinishCampaignLevel(false, "Bo beat the countdown."); return; }
            if (bossActive && resolved)
            {
                UnlockedBossMask |= 1 << ((int)encounter - 1); bossActive = false; if (CurrentLevel % 5 == 0) milestoneCleared = true; chaos = Mathf.Max(0, chaos - 35);
                SaveCampaignProgress(CurrentLevel);
                Toast(scenarioTitles[(int)encounter] + " defeated");
                encounter = Encounter.Store; encounterRollClock = UnityEngine.Random.Range(24f, 42f);
            }
            if (shiftTime >= ShiftBudget) { FinishCampaignLevel(false, "The shop closed before all targets were met."); return; }
            if (chaos >= 100) { FinishCampaignLevel(false, "The shop became too chaotic. Keep spills and queues under control."); return; }
            if (served >= ServeGoal && stockActions >= StockGoal && hazardsCleared >= CleanGoal && !bossActive && !mandatoryBossPending && skillType < 0)
            { FinishCampaignLevel(true, ""); return; }
            if (bossActive) return;
            encounterRollClock -= dt;
            if (encounterRollClock > 0 || skillType >= 0) return;
            if (mandatoryBossPending)
            { mandatoryBossPending = false; StartCampaignEncounter(MilestoneBoss(CurrentLevel)); return; }
            encounterRollClock = UnityEngine.Random.Range(18f, 32f);
            // Only defeated bosses return; chaos shifts allow one surprise encounter.
            if (Targets.focus == ShiftFocus.Chaos && UnlockedBossMask != 0 && randomFights < 1 && UnityEngine.Random.value < .6f)
            { var next = PickUnlockedBoss(); if (next != Encounter.Store) { randomFights++; StartCampaignEncounter(next); } }
        }
        void FinishCampaignLevel(bool won, string reason)
        {
            if (levelResults) return;
            levelResults = true; levelWon = won; gameOver = !won; levelFailure = reason; skillType = -1;
            resultsVisualDirty = true;
            playerVelocity = Vector2.zero; ReleaseMobileControls(); heldKeys.Clear(); shiftBriefOpen = false;
            invoiceStars = Rating(won).stars;
            DeviceVibration.Pulse(won ? 65 : 90);
            if (won && campaignPersistence) SaveLevelBest();
            if (won) { SaveCampaignProgress(CurrentLevel + 1); PlaySound("Success"); } else PlaySound("Miss");
        }
        string CampaignObjective()
        {
            if (bossActive) return scenarioDescriptions[(int)encounter];
            return "Level " + CurrentLevel + "   Serve " + served + "/" + ServeGoal + "   ·   Stock " + stockActions + "/" + StockGoal + "   ·   Clean " + hazardsCleared + "/" + CleanGoal;
        }
        int InvoicePenalty => missedCustomers * 100 + priceMistakes * 25;
        int InvoiceTotal => salesCents + stockActions * 75 + hazardsCleared * 50 - InvoicePenalty;
        Texture2D InvoiceStar()
        {
            var supplied = ShopArtwork.UI("Star"); if (supplied) return supplied;
            if (invoiceStar) return invoiceStar;
            invoiceStar = new Texture2D(96, 96, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
            var vertices = new Vector2[10]; for (int i = 0; i < 10; i++) { float angle = (90 + i * 36) * Mathf.Deg2Rad; vertices[i] = new Vector2(48, 48) + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (i % 2 == 0 ? 45 : 20); }
            var pixels = new Color[96 * 96];
            for (int y = 0; y < 96; y++) for (int x = 0; x < 96; x++)
                {
                    bool inside = false; for (int i = 0, j = 9; i < 10; j = i++) if ((vertices[i].y > y) != (vertices[j].y > y) && x < (vertices[j].x - vertices[i].x) * (y - vertices[i].y) / (vertices[j].y - vertices[i].y) + vertices[i].x) inside = !inside;
                    pixels[y * 96 + x] = inside ? Color.white : Color.clear;
                }
            invoiceStar.SetPixels(pixels); invoiceStar.Apply(); return invoiceStar;
        }
    }
}
