using System.Collections.Generic;
using UnityEngine;

namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        const float SpillSpacing=80, CleanPatchRadius=60;
        struct CleanPatch {public Vector2 position;public float until;}
        readonly List<CleanPatch> cleanPatches=new List<CleanPatch>();
        SpillHazard mopTargetView;
        Vector2 mopTargetPosition,lastCleanPosition;
        float cleanFlash;
        bool TrySpawnSpill(Vector2 position,int limit=7)
        {
            if(spills.Count>=limit||!Walkable(position))return false;
            foreach(var spill in spills)if(Vector2.Distance(spill,position)<SpillSpacing)return false;
            for(int i=cleanPatches.Count-1;i>=0;i--)
            {
                if(Time.time>=cleanPatches[i].until){cleanPatches.RemoveAt(i);continue;}
                if(Vector2.Distance(cleanPatches[i].position,position)<110)return false;
            }
            spills.Add(position);return true;
        }
        void CaptureMopTarget(int index)
        {
            SyncSpillViews();mopTargetPosition=spills[index];mopTargetView=index<spillViews.Count?spillViews[index]:null;
        }
        bool FinishMopPatch()
        {
            var position=mopTargetView?mopTargetView.Position:mopTargetPosition;
            mopTargetView=null;
            return ClearMopPatchAt(position);
        }
        bool ClearMopPatchAt(Vector2 position)
        {
            bool removed=false;
            // Older saves/scenes and previously spawned goblin mess may contain stacked
            // puddles. A completed minigame clears the visible patch, not just one layer.
            for(int i=spills.Count-1;i>=0;i--)if(Vector2.Distance(spills[i],position)<=CleanPatchRadius)
                removed=ClearSpill(i)||removed;
            if(!removed)return false;
            cleanPatches.Add(new CleanPatch{position=position,until=Time.time+14});
            lastCleanPosition=position;cleanFlash=Time.time+1;return true;
        }
        void ResetSpillLifecycle(){cleanPatches.Clear();mopTargetView=null;cleanFlash=0;}
        void CollapseStackedSpills()
        {
            for(int i=0;i<spills.Count;i++)for(int j=spills.Count-1;j>i;j--)
                if(Vector2.Distance(spills[i],spills[j])<=CleanPatchRadius)ClearSpill(j);
        }
    }
}
