using System;
using System.Collections;
using UnityEngine;

namespace TokTokMart
{
    public sealed partial class ShiftController
    {
        [SerializeField] PunchCardScreens punchCards;

        void ConnectAuthoredScreens()
        {
            punchCards.Connect(SelectPunchTile, TurnPunchPage, PunchIn, ClosePunchMap, ResultButton);
            restockPanel.Connect(TryPlaceRestockPack, CancelCurrentTask);
            restockPanel.Close();
        }
        Coroutine shiftTransition;
        float shiftLoadProgress = -1;
        bool mapVisualDirty = true, resultsVisualDirty = true;

        void RefreshReleaseUI()
        {
            bool showMap = mainMenu && levelMapOpen && !settingsOpen;
            bool showResults = levelResults && !settingsOpen;
            if (!punchCards) return;

            if (showMap && mapVisualDirty)
            {
                string boss = mapSelection % 5 == 0 ? scenarioTitles[(int)MilestoneBoss(mapSelection)] : "";
                punchCards.BindMap(mapPage * LevelsPerPage + 1, mapSelection, HighestUnlocked,
                    level => PlayerPrefs.GetInt(BestKey(level, "Stars"), 0), new ShiftGoals(mapSelection).Hint, boss);
                mapVisualDirty = false;
            }
            if (showResults && resultsVisualDirty)
            {
                punchCards.BindResults(CurrentLevel, levelWon, Rating(levelWon), levelFailure,
                    stockActions, hazardsCleared, droppedStockPickedUp, served, shiftTime);
                resultsVisualDirty = false;
            }
            if (!showMap) mapVisualDirty = true;
            if (!showResults) resultsVisualDirty = true;
            punchCards.Show(showMap, showResults, shiftLoadProgress);
        }

        void SelectPunchTile(int localIndex)
        {
            int level = mapPage * LevelsPerPage + localIndex + 1;
            if (level > HighestUnlocked) return;
            mapSelection = level;
            mapVisualDirty = true;
            PlaySound("Click", .55f);
        }

        void TurnPunchPage(int direction)
        {
            int page = mapPage + direction;
            if (page < 0 || page * LevelsPerPage + 1 > HighestUnlocked) return;
            mapPage = page;
            SelectPunchTile(0);
        }

        void PunchIn()
        {
            if (mapSelection > HighestUnlocked) return;
            chosenLevel = mapSelection;
            levelMapOpen = false;
            mainMenu = false;
            selected = false;
            PlaySound("ClockIn");
        }

        void ClosePunchMap()
        {
            levelMapOpen = false;
            PlaySound("Click", .55f);
        }

        void ResultButton(int button)
        {
            PlaySound("Click", .55f);
            if (button == 2)
            {
                ReturnToMainMenu();
                OpenLevelMap();
                return;
            }
            int target = button == 0 && levelWon ? CurrentLevel + 1 : CurrentLevel;
            QueueShiftTransition(() => BeginCampaignLevel(target, campaignPersistence));
        }

        void QueueShiftTransition(Action opening)
        {
            if (shiftTransition == null) shiftTransition = StartCoroutine(ShowShiftTransition(opening));
        }

        IEnumerator ShowShiftTransition(Action opening)
        {
            shiftLoadProgress = 0;
            bufferedActionUntil = -1;
            ReleaseMobileControls();
            playerVelocity = Vector2.zero;
            PlaySound("ClockIn", .68f);
            float start = Time.unscaledTime;
            yield return null;

            // Queue imports in the background so the opening screen can render while Unity prepares them.
            var first = Resources.LoadAsync<Texture2D>("NovemberArt/KeeperIdle/Sam");
            var second = Resources.LoadAsync<Texture2D>("NovemberArt/KeeperIdle/Zack");
            while (!first.isDone || !second.isDone || Time.unscaledTime - start < .34f)
            {
                shiftLoadProgress = Mathf.Min(.7f, (Time.unscaledTime - start) * 2);
                yield return null;
            }
            opening();
            float reveal = Time.unscaledTime;
            while (Time.unscaledTime - reveal < .22f)
            {
                shiftLoadProgress = Mathf.Lerp(.8f, 1, (Time.unscaledTime - reveal) / .22f);
                yield return null;
            }
            shiftLoadProgress = -1;
            shiftTransition = null;
        }
    }
}
