using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        bool gameplayShiftPlaytest;
        IEnumerator ReplayRestock()
        {
            yield return Go(supply);
            for(int attempt=0;attempt<8&&!carryingCrate&&!levelResults;attempt++)
            {Interact();if(skillType>=0)yield return CompleteSkill();}
            if(!carryingCrate||levelResults)yield break;
            int index=0;for(int i=0;i<shelfStock.Length;i++)if(shelfStock[i]<shelfStock[index])index=i;
            yield return Go(ShelfPoint(index));
            for(int attempt=0;attempt<8&&carryingCrate&&!levelResults;attempt++)
            {Interact();if(skillType>=0)yield return CompleteSkill();else yield return new WaitForSeconds(.5f);}
        }
        void PerfectRefreshStroke()
        {if(skillType==0){PlaceRestockPack();return;}taskLastAttempt=-1;skillClock=SkillZoneCenter()/((.75f+.08f*taskCombo)*TaskDifficulty);AttemptSkill();}
        public void BeginGameplayRefreshVerification(string folder)=>StartCoroutine(VerifyGameplayRefresh(folder));
        IEnumerator VerifyGameplayRefresh(string folder)
        {
            Directory.CreateDirectory(folder);var checks=new List<string>();
            Action<bool,string> check=(ok,label)=>checks.Add((ok?"PASS ":"FAIL ")+label);
            bool oldFocus=pauseOnFocusLoss,oldTouch=showTouchControls;var rng=UnityEngine.Random.state;
            pauseOnFocusLoss=false;testing=verificationRunning=true;settingsOpen=false;autoPaused=false;
            try
            {
                for(int level=2;level<=100;level++)
                {var before=new ShiftTargets(level-1);var after=new ShiftTargets(level);check(after.taskCount==before.taskCount+1&&after.duration>before.duration,"Shift "+level+" adds work and lasts longer");}
                BeginCampaignLevel(1,false);check(CustomerPatience==42,"Opening patience is 42 seconds instead of 95");
                BeginCampaignLevel(20,false);check(CustomerPatience<28&&CustomerPatience>=16,"Later customers lose patience faster");
                var queued=SpawnCustomer();queued.state=ShopperState.Queuing;queued.basket=1;queued.position=QueuePoint(0);queued.timer=CustomerPatience-.05f;customerQueue.Add(queued);
                int missed=missedCustomers;UpdateCustomers(.1f);check(queued.angry&&missedCustomers==missed+1,"The shorter patience deadline actually makes a customer leave");

                ResetRun(Encounter.Goblins);selected=true;mainMenu=false;
                for(int i=0;i<100;i++)UpdateEncounter(.4f);
                bool separate=true;for(int i=0;i<spills.Count;i++)for(int j=i+1;j<spills.Count;j++)if(Vector2.Distance(spills[i],spills[j])<SpillSpacing)separate=false;
                check(separate&&spills.Count<=7,"Live goblin mess cannot stack puddles at repeated spawn points");
                check(!resolved,"Goblins cannot disappear without Ray and player actions");

                BeginCampaignLevel(4,false);shiftBriefOpen=false;SyncSpillViews();
                var target=spills[0];var other=spills[1];spills.Add(target+Vector2.one*.2f);SyncSpillViews();
                player=target;Interact();for(int stroke=0;stroke<4;stroke++)PerfectRefreshStroke();
                yield return null;yield return null;yield return null;
                check(hazardsCleared==1&&!spills.Any(p=>Vector2.Distance(p,target)<=CleanPatchRadius),"One four-stroke minigame removes every layer of a stacked patch and awards one cleanup");
                check(spills.Contains(other),"Cleaning a stacked patch preserves the other puddle");
                check(!spillViews.Any(v=>v&&v.gameObject.activeSelf&&Vector2.Distance(v.Position,target)<=CleanPatchRadius),"No visible puddle survives on the cleaned patch");
                check(!TrySpawnSpill(target),"A fresh cleanup cannot immediately respawn under the mop");
                check(!shop.spillTemplate.gameObject.activeSelf,"The reusable spill template is never a visible uncleanable hazard");

                BeginCampaignLevel(4,false);shiftBriefOpen=false;SyncSpillViews();target=spills[1];player=target;Interact();ClearSpill(0);
                for(int stroke=0;stroke<4;stroke++)PerfectRefreshStroke();yield return null;
                check(hazardsCleared==1&&!spills.Any(p=>Vector2.Distance(p,target)<=CleanPatchRadius),"Mopping still clears its captured patch when another spill shifts list indexes");
                ResetRun(Encounter.Store);yield return null;
                check(spills.Count==initialSpillPositions.Count&&authoredSpills.All(s=>s.gameObject.activeSelf),"Retry restores authored hazards without ghost runtime views");

                ResetRun(Encounter.Goblins);selected=true;mainMenu=false;rayCalled=true;rayRemaining=0;rayPosition=goblinPositions[0];
                UpdateEncounter(.2f);check(rayPunches==0,"Ray cannot defeat unstunned goblins by waiting alone");
                for(int i=0;i<GoblinTotal;i++)
                {
                    player=goblinPositions[i];shooCooldown=0;Interact();rayPosition=goblinPositions[i];
                    UpdateEncounter(.7f);yield return null;UpdateEncounter(.7f);
                }
                check(rayPunches==GoblinTotal*2&&goblinShoos>=GoblinTotal&&goblins==0,"Player stuns and six real Ray punches knock out the whole gang");
                yield return null;check(shop.rayCharacter.GetComponent<CharacterCombatFeedback>(),"Ray has visible punch/lunge feedback");
                UpdateEncounter(.8f);check(resolved,"Goblin victory follows the fight, not the arrival timer");

                ResetRun(Encounter.KKung);selected=true;mainMenu=false;player=lightSwitch;Interact();
                check(circuitStep==1&&!resolved&&!lightsOn,"One switch no longer instantly wins the K-Kung fight");
                Vector2 previousSwitch=lightSwitch;player=lightSwitch;Interact();check(lightSwitch!=previousSwitch,"Restoring a circuit moves the next objective");
                player=lightSwitch;Interact();check(circuitStep==3&&lightsOn&&resolved,"Three circuits banish K-Kung");
                ResetRun(Encounter.KKung);selected=true;mainMenu=false;warningPosition=player;warningClock=.1f;player+=Vector2.right*130;float danger=chaos;TickGroundAttack(.2f);
                check(chaos==danger,"Leaving a telegraphed attack avoids its chaos penalty");
                warningPosition=player;warningClock=.1f;TickGroundAttack(.2f);check(chaos==danger+10,"Ignoring a telegraph has a real consequence");

                ResetRun(Encounter.Salmah);selected=true;mainMenu=false;hasGun=true;salmahStun=2;player=salmah-salmahFacing*90;UpdateEncounter(.2f);
                check(nailsHit==0,"Standing behind Salmah does not fire automatically");
                Interact();check(nailsHit==1,"An explicit rear shot damages Salmah");
                player=salmah+salmahFacing*90;nailCooldown=0;Interact();check(nailsHit==1,"Front shots cannot bypass flanking");

                ResetRun(Encounter.Bo);selected=true;mainMenu=false;
                for(int i=0;i<BoReturnGoal;i++)
                {player=can;boSpawnDelay=0;Interact();check(canFlying,"Bo round "+(i+1)+" needs an actual kick");FinishBoReturn();check(resolved==(i==BoReturnGoal-1),"Bo resolves only after return "+(i+1));}
                check(boReturns==3,"All three returned cans are counted");

                ResetRun(Encounter.Goblins);selected=true;mainMenu=false;rayCalled=true;rayRemaining=0;player=goblinPositions[0];Interact();rayPosition=goblinPositions[0]-Vector2.right*45;rayPunchCooldown=0;UpdateEncounter(.04f);
                showTouchControls=true;yield return NovemberShot(folder,"ray-fight");
                ResetRun(Encounter.KKung);selected=true;mainMenu=false;player=lightSwitch;Interact();warningPosition=player+Vector2.down*60;warningClock=.8f;
                yield return NovemberShot(folder,"circuit-fight");
                check(editableUI.MissingBindingCount==0,"New encounter states retain authored Canvas bindings");
            }
            finally
            {
                pauseOnFocusLoss=oldFocus;showTouchControls=oldTouch;UnityEngine.Random.state=rng;testing=verificationRunning=false;ReturnToMainMenu();
                File.WriteAllText(Path.Combine(folder,"checks.txt"),string.Join("\n",checks)+"\n\n"+checks.Count+" checks; "+checks.Count(s=>s.StartsWith("FAIL"))+" failures.\n");
            }
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"--quit-after-gameplay-refresh")>=0)Application.Quit(checks.Any(s=>s.StartsWith("FAIL"))?1:0);
        }
        public void BeginGameplayRefreshPlaytest(string folder)=>StartCoroutine(PlaytestGameplayRefresh(folder));
        public void BeginGameplayRefreshShiftPlaytest(string folder)=>StartCoroutine(PlaytestLongerShifts(folder));
        IEnumerator PlaytestLongerShifts(string folder)
        {
            Directory.CreateDirectory(folder);var results=new List<string>();bool oldFocus=pauseOnFocusLoss,oldTouch=showTouchControls;
            var rng=UnityEngine.Random.state;pauseOnFocusLoss=false;showTouchControls=true;settingsOpen=false;testing=false;verificationRunning=true;gameplayShiftPlaytest=true;Time.captureFramerate=30;
            foreach(int level in new[]{1,5,10,20})
            {
                BeginCampaignLevel(level,false);shiftBriefOpen=false;UnlockedBossMask=0;randomFights=2;
                if(level%5==0){encounterRollClock=0;TickCampaign(0,true);yield return PlayActiveBoss(MilestoneBoss(level));TickCampaign(0,true);}
                int guard=0;
                while(stockActions<StockGoal&&!levelResults&&guard++<40)
                {
                    yield return ReplayRestock();
                }
                guard=0;
                while(hazardsCleared<CleanGoal&&!levelResults&&guard++<40)
                {
                    EnsureShiftCleanupWork();if(spills.Count==0){yield return null;continue;}
                    yield return Go(spills[0]);Interact();if(skillType>=0)yield return CompleteSkill();TickCampaign(0,true);
                }
                while(!levelResults&&shiftTime<ShiftBudget+1)yield return null;
                TickCampaign(0,true);
                results.Add((levelWon?"PASS ":"FAIL ")+"Shift "+level+": "+Targets.taskCount+" tasks; "+served+"/"+ServeGoal+" customers, "+stockActions+"/"+StockGoal+" restocks, "+hazardsCleared+"/"+CleanGoal+" cleanups; "+shiftTime.ToString("F1")+"/"+ShiftBudget+" seconds; patience "+CustomerPatience.ToString("F1")+"s; stars "+invoiceStars+"; "+levelFailure);
                yield return NovemberShot(folder,"shift-"+level);File.WriteAllLines(Path.Combine(folder,"shift-playthroughs.txt"),results);
            }
            Time.captureFramerate=0;pauseOnFocusLoss=oldFocus;showTouchControls=oldTouch;UnityEngine.Random.state=rng;testing=verificationRunning=gameplayShiftPlaytest=false;ReturnToMainMenu();Debug.Log(string.Join("\n",results));
        }
        IEnumerator PlaytestGameplayRefresh(string folder)
        {
            Directory.CreateDirectory(folder);var results=new List<string>();bool oldFocus=pauseOnFocusLoss,oldTouch=showTouchControls;
            pauseOnFocusLoss=false;showTouchControls=true;settingsOpen=false;autoPaused=false;testing=verificationRunning=true;Time.captureFramerate=30;
            foreach(Encounter boss in new[]{Encounter.Goblins,Encounter.KKung,Encounter.Salmah,Encounter.Bo})
            {
                ResetRun(boss);selected=true;mainMenu=false;yield return PlayActiveBoss(boss);
                results.Add((resolved&&!gameOver?"PASS ":"FAIL ")+boss+" completed through navigation and real inputs; Ray punches "+rayPunches+", circuits "+circuitStep+", nails "+nailsHit+", can returns "+boReturns);
                yield return NovemberShot(folder,"boss-"+boss);File.WriteAllLines(Path.Combine(folder,"boss-playthroughs.txt"),results);
            }
            Time.captureFramerate=0;pauseOnFocusLoss=oldFocus;showTouchControls=oldTouch;testing=verificationRunning=false;ReturnToMainMenu();Debug.Log(string.Join("\n",results));
        }
    }
}
