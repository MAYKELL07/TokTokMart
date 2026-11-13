using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        public void BeginNovemberPlaytest(string folder)=>StartCoroutine(NovemberPlaytest(folder));
        IEnumerator NovemberPlaytest(string folder)
        {
            Directory.CreateDirectory(folder);var results=new List<string>();var rng=UnityEngine.Random.state;bool touch=showTouchControls;
            bool oldFocusPause=pauseOnFocusLoss;pauseOnFocusLoss=false;testing=false;verificationRunning=false;settingsOpen=false;showTouchControls=true;Time.captureFramerate=30;
            foreach(int level in new[]{1,5})
            {
                BeginCampaignLevel(level,false);UnlockedBossMask=0;randomFights=2;shiftBriefOpen=false;
                if(level==5){encounterRollClock=0;TickCampaign(0,true);yield return Go(phone);Interact();while(bossActive&&!levelResults)yield return null;}
                int guard=0;
                while(stockActions<StockGoal&&!levelResults&&guard++<12)
                {
                    yield return Go(supply);Interact();int index=0;for(int i=0;i<shelfStock.Length;i++)if(shelfStock[i]<shelfStock[index])index=i;
                    yield return Go(ShelfPoint(index));Interact();if(skillType>=0)yield return CompleteSkill();
                }
                guard=0;while(hazardsCleared<CleanGoal&&!levelResults&&spills.Count>0&&guard++<12){yield return Go(spills[0]);Interact();if(skillType>=0)yield return CompleteSkill();}
                while(!levelResults&&shiftTime<ShiftBudget+1)yield return null;
                bool ok=levelResults&&levelWon;results.Add((ok?"PASS ":"FAIL ")+"Complete shift "+level+" through actual navigation, task timing, shoppers and partner AI: "+served+"/"+ServeGoal+" served, "+stockActions+"/"+StockGoal+" stocked, "+hazardsCleared+"/"+CleanGoal+" cleaned; "+shiftTime.ToString("F1")+"s / "+ShiftBudget+"s; stars "+invoiceStars+"; score "+Rating(levelWon).total+"/"+Rating(levelWon).maximum+(ok?"":"; "+levelFailure));
                yield return NovemberShot(folder,"playthrough-shift-"+level);
            }
            Time.captureFramerate=0;UnityEngine.Random.state=rng;verificationRunning=false;testing=false;pauseOnFocusLoss=oldFocusPause;showTouchControls=touch;ReturnToMainMenu();File.WriteAllLines(Path.Combine(folder,"playthrough.txt"),results);Debug.Log(string.Join("\n",results));
        }
        IEnumerator NovemberShot(string folder,string name)
        {yield return null;yield return new WaitForEndOfFrame();yield return null;yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(folder,name+".png"));yield return null;yield return new WaitForEndOfFrame();}
        public void BeginNovemberVerification(string folder)=>StartCoroutine(VerifyNovember(folder));
        IEnumerator VerifyNovember(string folder)
        {
            Directory.CreateDirectory(folder);var results=new List<string>();int failures=0;
            Action<bool,string> check=(ok,label)=>{results.Add((ok?"PASS ":"FAIL ")+label);if(!ok)failures++;};
            var keys=new[]{LevelSave,BossSave,UnlockSave,BestKey(1,"Score"),BestKey(1,"Stars")};
            var saved=keys.ToDictionary(k=>k,k=>(PlayerPrefs.HasKey(k),PlayerPrefs.GetInt(k)));
            var rng=UnityEngine.Random.state;bool touch=showTouchControls;testing=true;verificationRunning=true;settingsOpen=false;autoPaused=false;
            try
            {
                var t=new ShiftTargets(1);
                ShiftScore Score(float time=0,float mess=0,int errors=0,int lost=0,bool win=true,int extra=0)=>new ShiftScore(t,t.serve+extra,t.stock+extra,t.clean+extra,false,false,errors,0,lost,time,mess,win);
                var perfect=Score();check(perfect.total==perfect.maximum&&perfect.stars==3&&perfect.maximum==1050,"Perfect score is reachable and totals exactly 1050 on shift one");
                check(Score(t.duration*.7f,15).total==1050,"Full bonus includes the exact 70% time and 15% chaos boundaries ("+Score(t.duration*.7f,15).total+" pts)");
                check(Score(t.duration*.7f+.01f,15).speed==100&&Score(t.duration*.85f+.01f).speed==0,"Speed bonus switches only at the advertised boundaries");
                check(Score(mess:15.01f).control==50&&Score(mess:40.01f).control==0,"Chaos bands match the scoring guide");
                check(Score(errors:2).total==perfect.threeStars&&Score(errors:2).stars==3&&Score(errors:3).stars==2,"Three stars begins exactly at the displayed threshold");
                check(Score(errors:8).stars==2&&Score(errors:9,lost:1,mess:100,time:t.duration).stars==1,"Two stars begins at objectives +300; completion still earns one star");
                check(Score(win:false).stars==0&&Score(win:false).total==550,"An incomplete shift earns zero stars and no completion bonuses");
                check(Score(extra:50).total==perfect.total,"Repeating tasks and extra customers cannot farm rating points");
                BeginCampaignLevel(1,false);shiftBriefOpen=false;skillType=0;skillTarget=0;PrepareTask(0);skillClock=0;taskLastAttempt=-1;
                check(!AttemptSkill()&&timingMistakes==1&&taskFeedbackFailed&&taskFeedback.Contains("-25"),"A timing miss records a visible 25-point accuracy deduction");
                int misses=timingMistakes;AttemptSkill();check(timingMistakes==misses,"Input debounce prevents repeated misses in the same frame");
                carryingCrate=true;taskElapsed=TaskBudget-.01f;TickShiftFeedback(.02f);check(skillType==-1&&carryingCrate&&timingMistakes==misses+1,"Task timeout exits safely, keeps the crate and applies one penalty");
                BeginCampaignLevel(1,false);var customer=SpawnCustomer();customer.basket=1;customer.items.Add(0);customer.state=ShopperState.Queuing;customer.position=QueuePoint(0);customerQueue.Add(customer);player=checkout;Interact();
                priceEntry="1";taskLastAttempt=-1;SubmitPrice();check(priceMistakes==1&&served==0&&taskFeedback.Contains("-25"),"Wrong checkout total is explained and does not complete a sale");
                priceEntry=RinggitNumber(receiptTotal);SubmitPrice();check(served==1&&salesCents==350,"Correct checkout records actual revenue separately from fixed rating points");
                var waiting=SpawnCustomer();waiting.basket=1;waiting.items.Add(1);waiting.state=ShopperState.Queuing;waiting.position=QueuePoint(0);waiting.timer=CustomerPatience+.1f;customerQueue.Add(waiting);skillType=1;skillTarget=waiting.id;UpdateCustomers(.1f);
                check(missedCustomers==1&&skillType==-1&&!customerQueue.Contains(waiting),"Customer patience expires even during checkout; stale receipt closes");
                BeginCampaignLevel(5,false);served=ServeGoal;stockActions=StockGoal;hazardsCleared=CleanGoal;TickCampaign(0,true);check(!levelResults,"A boss shift requires its boss as well as every task target");
                BeginCampaignLevel(1,false);UnlockedBossMask=15;check(PickUnlockedBoss()==Encounter.Store,"Replaying an opening shift cannot import bosses unlocked in later shifts");
                BeginCampaignLevel(6,false);UnlockedBossMask=15;bool onlyEarlyBoss=true;for(int i=0;i<50;i++)onlyEarlyBoss&=PickUnlockedBoss()==Encounter.Goblins;check(onlyEarlyBoss,"Returning bosses respect the selected shift's milestone difficulty");
                for(int level=1;level<=20;level++){BeginCampaignLevel(level,false);check(spills.Count>=Mathf.Min(2,CleanGoal)&&TaskDifficulty<=1.35f&&CustomerPatience>=45,"Playable work, capped timing difficulty and patience on shift "+level);}
                BeginCampaignLevel(1,false);stockActions=StockGoal;hazardsCleared=CleanGoal;served=ServeGoal;TickCampaign(0,true);check(levelWon&&invoiceStars==3,"Completing every objective awards the three-star result");
                PlayerPrefs.SetInt(UnlockSave,8);PlayerPrefs.SetInt(LevelSave,8);BeginCampaignLevel(1,true);served=ServeGoal;stockActions=StockGoal;hazardsCleared=CleanGoal;FinishCampaignLevel(true,"");
                check(HighestUnlocked==8&&PlayerPrefs.GetInt(BestKey(1,"Stars"))==3,"Replaying old shifts preserves later unlocks and saves best stars");
                int best=PlayerPrefs.GetInt(BestKey(1,"Score"));BeginCampaignLevel(1,true);served=ServeGoal;stockActions=StockGoal;hazardsCleared=CleanGoal;priceMistakes=20;shiftTime=ShiftBudget*.95f;chaos=80;FinishCampaignLevel(true,"");
                check(PlayerPrefs.GetInt(BestKey(1,"Score"))==best&&PlayerPrefs.GetInt(BestKey(1,"Stars"))==3,"A worse replay never overwrites the best score or stars");
                OpenLevelMap();check(mapSelection<=HighestUnlocked&&mapPage==0,"Level map starts on an unlocked shift");
                BeginCampaignLevel(1,false);shiftBriefOpen=true;mainMenu=false;yield return NovemberShot(folder,"01-briefing");
                shiftBriefOpen=false;showTouchControls=true;var queued=SpawnCustomer();queued.basket=1;queued.items.Add(0);queued.state=ShopperState.Queuing;queued.position=QueuePoint(0);queued.timer=CustomerPatience*.82f;customerQueue.Add(queued);yield return NovemberShot(folder,"02-mobile-hud");
                check(NovemberArt.UI("Star")&&NovemberArt.UI("CheckoutCard01")&&NovemberArt.UI("ProgressBar01"),"New star, task cards and separate progress-bar art load");
                check(font&&font.name=="Bigbesty"&&editableUI.GetComponentsInChildren<Text>().All(x=>x.font==font),"Bigbesty is applied to every visible Canvas text");
                scoreGuideOpen=true;yield return NovemberShot(folder,"03-score-guide");scoreGuideOpen=false;
                skillType=2;PrepareTask(2);taskFeedbackFailed=true;taskFeedback="Right of the green zone. -25 accuracy points; try again.";taskElapsed=21;yield return NovemberShot(folder,"04-task-failure-countdown");skillType=-1;
                served=ServeGoal;stockActions=StockGoal;hazardsCleared=CleanGoal;FinishCampaignLevel(true,"");yield return NovemberShot(folder,"05-three-star-results");
                ReturnToMainMenu();OpenLevelMap();yield return NovemberShot(folder,"06-level-map");
                check(editableUI.GetComponentsInChildren<Button>().Count(x=>x.interactable)>5,"Native Canvas level-map buttons are interactable");
                var root=shop.transform.Find("Reference shop layout");check(root.Find("Artist counter table November11")&&shop.stockroom.transform.Find("Artist storage box November11"),"New counter and storage-box artwork exist in the playable scene");
                var goals=new List<Vector2>{supply,QueuePoint(0),phone,nailGun};foreach(var shelf in sceneShelves)goals.Add(ShopLayout.ToGame(shelf.shoppingPoint.position));
                foreach(var goal in goals){var point=playerStart;for(int i=0;i<2400&&Vector2.Distance(point,goal)>8;i++)point=Navigate(point,goal,16);check(Vector2.Distance(point,goal)<8,"Gameplay route retained to "+goal);yield return null;}
            }
            finally
            {
                foreach(var kv in saved){if(kv.Value.Item1)PlayerPrefs.SetInt(kv.Key,kv.Value.Item2);else PlayerPrefs.DeleteKey(kv.Key);}PlayerPrefs.Save();UnityEngine.Random.state=rng;
                showTouchControls=touch;scoreGuideOpen=false;ReturnToMainMenu();testing=false;verificationRunning=false;
                File.WriteAllText(Path.Combine(folder,"verification.txt"),string.Join("\n",results)+"\n\n"+results.Count+" checks; "+failures+" failures.\n");
                Debug.Log("November 11: "+results.Count+" checks; "+failures+" failures.");
            }
        }
    }
}
