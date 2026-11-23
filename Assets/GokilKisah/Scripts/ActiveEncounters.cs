using UnityEngine;

namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        Vector2[] goblinPositions=new Vector2[0],goblinGoals=new Vector2[0],goblinMotion=new Vector2[0];
        int[] goblinHealth=new int[0];
        float[] goblinStun=new float[0],goblinImpact=new float[0];
        Vector2 rayPosition,rayMotion,salmahMotion,chargeTarget,warningPosition;
        int rayFacing=23,circuitStep,boReturns,rayPunches,goblinShoos,messCursor;
        float shooCooldown,rayPunchCooldown,rayPunchFlash,bossVictoryClock,roamClock;
        float warningClock,warningCooldown,salmahWindup,salmahDash,salmahRecovery,salmahStun,salmahImpact,boSpawnDelay,boImpact;
        const int CircuitGoal=3,BoReturnGoal=3;
        int GoblinTotal=>goblinHealth.Length;
        bool ActiveFight=>encounter!=Encounter.Store&&!resolved;
        Vector2 SafeBossPoint(Vector2 point)
        {
            if(Walkable(point))return point;
            for(int r=25;r<=125;r+=25)for(int d=0;d<8;d++)
            {var candidate=point+new Vector2(Mathf.Cos(d*Mathf.PI/4),Mathf.Sin(d*Mathf.PI/4))*r;if(Walkable(candidate))return candidate;}
            return playerStart;
        }
        void ResetActiveEncounter(Encounter next)
        {
            int count=shop?shop.goblinSpawns.Length:0;
            goblinPositions=new Vector2[count];goblinGoals=new Vector2[count];goblinMotion=new Vector2[count];
            goblinHealth=new int[count];goblinStun=new float[count];goblinImpact=new float[count];
            for(int i=0;i<count;i++){goblinPositions[i]=SafeBossPoint(ShopLayout.ToGame(shop.goblinSpawns[i].position)+Vector2.down*35);goblinGoals[i]=goblinPositions[i];goblinHealth[i]=2;}
            goblins=next==Encounter.Goblins?count:0;
            rayPosition=shop?SafeBossPoint(ShopLayout.ToGame(shop.customerEntryTarget.position)):rayStart;rayMotion=Vector2.zero;
            rayPunches=goblinShoos=circuitStep=boReturns=messCursor=0;
            shooCooldown=rayPunchCooldown=rayPunchFlash=bossVictoryClock=roamClock=0;
            warningClock=salmahWindup=salmahDash=salmahRecovery=salmahStun=salmahImpact=boImpact=0;
            warningCooldown=3;boSpawnDelay=.5f;salmahMotion=Vector2.zero;salmahFacing=Vector2.left;
            if(next==Encounter.Salmah)salmah=SafeBossPoint(salmah);
        }
        int FaceColumn(Vector2 direction)=>Mathf.Abs(direction.x)>Mathf.Abs(direction.y)?direction.x<0?23:26:direction.y<0?25:24;
        int NearestGoblin(float radius=85)
        {
            int nearest=-1;float best=radius;
            for(int i=0;i<GoblinTotal;i++)if(goblinHealth[i]>0){float d=Vector2.Distance(player,goblinPositions[i]);if(d<best){best=d;nearest=i;}}
            return nearest;
        }
        bool EncounterInteractionInReach()
        {
            if(!ActiveFight)return false;
            if(encounter==Encounter.Goblins)return !rayCalled&&StationInReach(shop.wallPhone)||NearestGoblin()>=0;
            if(encounter==Encounter.KKung)return Vector2.Distance(player,lightSwitch)<70;
            if(encounter==Encounter.Salmah)return !hasGun?Vector2.Distance(player,nailGun)<70:Vector2.Distance(player,salmah)<=tuning.nailRange;
            return encounter==Encounter.Bo&&!canKicked&&Vector2.Distance(player,can)<70;
        }
        bool TryEncounterInteraction()
        {
            if(!EncounterInteractionInReach())return false;
            if(encounter==Encounter.Goblins)
            {
                if(!rayCalled&&StationInReach(shop.wallPhone))
                {rayCalled=true;rayRemaining=Mathf.Min(6,tuning.rayWait);PlaySound("Call");Toast("Ray is coming");SetStep("RAY IS COMING","Shoo goblins while Ray gets into position.");return true;}
                int target=NearestGoblin();if(target<0||shooCooldown>0)return true;
                goblinStun[target]=5;goblinImpact[target]=.18f;shooCooldown=.4f;goblinShoos++;PlaySound("Kick");
                Toast(rayCalled?"Goblin stunned":"Call Ray at the till");return true;
            }
            if(encounter==Encounter.KKung)
            {
                circuitStep++;PlaySound("Switch");chaos=Mathf.Max(0,chaos-5);
                if(circuitStep>=CircuitGoal){lightsOn=true;resolved=true;Toast("All circuits restored. K-Kung banished!");}
                else {switchIndex=(switchIndex+1)%switches.Length;lightSwitch=switches[switchIndex];Toast("Circuits "+circuitStep+"/"+CircuitGoal);}
                return true;
            }
            if(encounter==Encounter.Salmah)
            {
                if(!hasGun){hasGun=true;PlaySound("Pickup");Toast("Nail gun collected");return true;}
                if(nailCooldown>0)return true;
                if(!CanHitSalmah()){Toast("Fire from behind");return true;}
                nailsHit++;nailCooldown=.42f;salmahImpact=.25f;salmahStun=1;PlaySound("Shot");
                if(nailsHit>=tuning.nailHits){resolved=true;Toast("Miss Salmah stopped!");}return true;
            }
            if(boSpawnDelay>0){Toast("Can incoming!");return true;}
            canKicked=canFlying=true;flyingCan=can;PlaySound("Kick");return true;
        }
        string ActiveEncounterObjective()
        {
            if(encounter==Encounter.Goblins)return !rayCalled?"CALL RAY + SHOO THE GOBLINS":rayRemaining>0?"RAY ARRIVES IN "+rayRemaining.ToString("0.0")+"s / SHOO GOBLINS!":"STUN THEM FOR RAY!  "+(GoblinTotal-goblins)+" / "+GoblinTotal+" DEFEATED";
            if(encounter==Encounter.KKung)return "RESTORE CIRCUITS  "+circuitStep+" / "+CircuitGoal+"  /  DODGE THE MARKS!";
            if(encounter==Encounter.Salmah)return hasGun?"DODGE, FLANK, TAP INTERACT TO FIRE  "+nailsHit+" / "+tuning.nailHits:"PICK UP THE NAIL GUN";
            return "RETURN THE CANS  "+boReturns+" / "+BoReturnGoal+"  /  "+boRemaining.ToString("0.0")+"s";
        }
        string ActiveInteractionLabel()
        {
            if(!EncounterInteractionInReach())return null;
            if(encounter==Encounter.Goblins)return !rayCalled&&StationInReach(shop.wallPhone)?"Call Uncle Ray":"Shoo goblin for Ray";
            if(encounter==Encounter.KKung)return "Restore circuit "+(circuitStep+1)+" / "+CircuitGoal;
            if(encounter==Encounter.Salmah)return hasGun?(CanHitSalmah()?"Fire nail gun!":"Flank Miss Salmah"):"Pick up nail gun";
            return boSpawnDelay>0?"Can incoming!":"Return can "+(boReturns+1)+" / "+BoReturnGoal;
        }
        void TickActiveEncounter(float dt)
        {
            shooCooldown=Mathf.Max(0,shooCooldown-dt);rayPunchCooldown=Mathf.Max(0,rayPunchCooldown-dt);rayPunchFlash=Mathf.Max(0,rayPunchFlash-dt);
            salmahImpact=Mathf.Max(0,salmahImpact-dt);salmahStun=Mathf.Max(0,salmahStun-dt);boImpact=Mathf.Max(0,boImpact-dt);
            rayMotion=salmahMotion=Vector2.zero;
            for(int i=0;i<GoblinTotal;i++){goblinMotion[i]=Vector2.zero;goblinStun[i]=Mathf.Max(0,goblinStun[i]-dt);goblinImpact[i]=Mathf.Max(0,goblinImpact[i]-dt);}
            if(resolved)return;
            if(encounter==Encounter.Goblins)TickGoblinFight(dt);
            else if(encounter==Encounter.KKung){chaos=Mathf.Min(100,chaos+dt*.45f);if(skillType<0)TickGroundAttack(dt);}
            else if(encounter==Encounter.Salmah)TickSalmahFight(dt);
            else if(encounter==Encounter.Bo)
            {
                boSpawnDelay=Mathf.Max(0,boSpawnDelay-dt);if(skillType<0)TickGroundAttack(dt);
                if(!canKicked){boRemaining=Mathf.Max(0,boRemaining-dt);if(boRemaining<=0){gameOver=true;PlaySound("Miss");Toast("Bo's throw got through!");}}
            }
        }
        void TickGoblinFight(float dt)
        {
            chaos=Mathf.Min(100,chaos+dt*(.3f+goblins*.12f));roamClock-=dt;nextMess-=dt;
            if(roamClock<=0)
            {
                roamClock=3;
                for(int i=0;i<GoblinTotal;i++)if(goblinHealth[i]>0&&sceneShelves.Length>0)
                    goblinGoals[i]=SafeBossPoint(ShelfPoint((i+messCursor)%sceneShelves.Length)+new Vector2(i%2==0?65:-65,65));
            }
            for(int i=0;i<GoblinTotal;i++)if(goblinHealth[i]>0&&goblinStun[i]<=0)
            {var before=goblinPositions[i];goblinPositions[i]=Navigate(before,goblinGoals[i],80*dt);goblinMotion[i]=goblinPositions[i]-before;}
            if(nextMess<=0&&goblins>0)
            {
                nextMess=3.2f;
                int origin=messCursor++%Mathf.Max(1,GoblinTotal);
                if(GoblinTotal>0&&goblinHealth[origin]>0)for(int d=0;d<8;d++)
                {var point=goblinPositions[origin]+new Vector2(Mathf.Cos(d*Mathf.PI/4),Mathf.Sin(d*Mathf.PI/4))*60;if(TrySpawnSpill(point))break;}
            }
            if(!rayCalled)return;
            if(rayRemaining>0){rayRemaining=Mathf.Max(0,rayRemaining-dt);return;}
            int target=-1;float best=float.MaxValue;
            for(int i=0;i<GoblinTotal;i++)if(goblinHealth[i]>0)
            {float score=Vector2.Distance(rayPosition,goblinPositions[i])+(goblinStun[i]>0?0:1500);if(score<best){best=score;target=i;}}
            if(target<0){bossVictoryClock+=dt;if(bossVictoryClock>.65f){resolved=true;PlaySound("Success");Toast("Ray knocked out the gang!");}return;}
            var beforeRay=rayPosition;var delta=goblinPositions[target]-rayPosition;rayFacing=FaceColumn(delta);
            if(delta.magnitude>53)rayPosition=Navigate(rayPosition,goblinPositions[target],280*dt);
            rayMotion=rayPosition-beforeRay;rayX=rayPosition.x;
            if(Vector2.Distance(rayPosition,goblinPositions[target])<=64&&goblinStun[target]>0&&rayPunchCooldown<=0)
            {
                rayPunchCooldown=.65f;rayPunchFlash=.28f;goblinImpact[target]=.4f;goblinHealth[target]--;rayPunches++;PlaySound("Impact",.7f);
                if(goblinHealth[target]==0){goblins--;chaos=Mathf.Max(0,chaos-5);}
            }
        }
        void TickGroundAttack(float dt)
        {
            if(warningClock>0)
            {
                warningClock-=dt;if(warningClock>0)return;
                if(Vector2.Distance(player,warningPosition)<60){chaos=Mathf.Min(100,chaos+10);PlaySound("Impact",.5f);Toast("Dodge the marked area!");}
                warningCooldown=Mathf.Max(2.5f,4.2f-CurrentLevel*.025f);return;
            }
            warningCooldown-=dt;if(warningCooldown<=0){warningPosition=player;warningClock=.95f;PlaySound("Click",.35f);}
        }
        void TickSalmahFight(float dt)
        {
            nailCooldown=Mathf.Max(0,nailCooldown-dt);if(skillType>=0||salmahStun>0)return;
            if(salmahRecovery>0){salmahRecovery=Mathf.Max(0,salmahRecovery-dt);return;}
            if(salmahWindup>0)
            {
                salmahWindup-=dt;warningPosition=chargeTarget;warningClock=Mathf.Max(.01f,salmahWindup);
                if(salmahWindup<=0){salmahDash=.4f;warningClock=0;ScareCustomer();}return;
            }
            var before=salmah;
            if(salmahDash>0)
            {
                salmahDash-=dt;salmah=Navigate(salmah,chargeTarget,430*dt);
                if(Vector2.Distance(player,salmah)<48){chaos=Mathf.Min(100,chaos+12);salmahDash=0;PlaySound("Impact",.5f);}
                if(salmahDash<=0){salmahRecovery=1.5f;nextAttack=2.3f;}
            }
            else
            {
                nextAttack-=dt;var direction=player-salmah;if(direction.sqrMagnitude>1)salmahFacing=direction.normalized;
                salmah=Navigate(salmah,player,55*dt);
                if(nextAttack<=0){chargeTarget=player;salmahWindup=.85f;warningPosition=player;warningClock=.85f;PlaySound("Miss",.25f);}
            }
            salmahMotion=salmah-before;
        }
        void FinishBoReturn()
        {
            canFlying=false;boReturns++;boImpact=.7f;PlaySound("Impact");chaos=Mathf.Max(0,chaos-4);
            if(boReturns>=BoReturnGoal){resolved=true;Toast("All three cans returned! Bo retreats.");return;}
            canIndex=(canIndex+1)%cans.Length;can=cans[canIndex];canKicked=false;boSpawnDelay=.7f;
            boRemaining=Mathf.Max(10,tuning.boDeadline-boReturns);Toast("Cans returned "+boReturns+"/"+BoReturnGoal);
        }
    }
}
