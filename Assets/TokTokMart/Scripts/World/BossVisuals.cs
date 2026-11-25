using UnityEngine;

namespace TokTokMart
{
    public sealed partial class ShiftController
    {
        void CombatFeedback(CharacterAnimation actor, float hit, float stun, float strike, int facing, bool defeated = false)
        { var feedback = actor.GetComponent<CombatReaction>(); feedback.Apply(actor, hit, stun, strike, pulse, facing, defeated); }
        void PresentActiveEncounters()
        {
            for (int i = 0; i < shop.goblinActors.Length; i++)
            {
                var actor = shop.goblinActors[i]; bool visible = encounter == Encounter.Goblins && i < GoblinTotal && (goblinHealth[i] > 0 || goblinImpact[i] > 0);
                actor.gameObject.SetActive(visible); if (!visible) continue;
                int facing = goblinMotion[i].sqrMagnitude > .001f ? FaceColumn(goblinMotion[i]) : 24;
                actor.Apply(goblinPositions[i], facing, 0, goblinMotion[i], pulse); CombatFeedback(actor, goblinImpact[i], goblinStun[i], 0, facing, goblinHealth[i] <= 0);
            }
            for (int i = 0; i < shop.idolActors.Length; i++)
            {
                var actor = shop.idolActors[i]; actor.gameObject.SetActive(encounter == Encounter.KKung && !resolved && i >= circuitStep);
                var p = ShopLayout.ToGame(shop.idolSpawns[i].position); actor.Apply(p + new Vector2(0, Mathf.Sin(pulse * 2 + i) * 12), 24, 0, Vector2.zero, pulse);
            }
            shop.rayCharacter.gameObject.SetActive(encounter == Encounter.Goblins && rayCalled && rayRemaining <= 0 && !resolved);
            shop.rayCharacter.Apply(rayPosition, rayFacing, 12, rayMotion, pulse); CombatFeedback(shop.rayCharacter, 0, 0, rayPunchFlash, rayFacing);
            shop.salmahCharacter.gameObject.SetActive(encounter == Encounter.Salmah && !resolved);
            int salmahColumn = FaceColumn(salmahFacing); shop.salmahCharacter.Apply(salmah, salmahColumn, 3, salmahMotion, pulse);
            CombatFeedback(shop.salmahCharacter, salmahImpact, salmahStun, salmahDash > 0 ? .15f : 0, salmahColumn);
        }
        void DrawActiveEncounterWorld()
        {
            if (cleanFlash > Time.time) Text(new Rect(lastCleanPosition.x - 65, lastCleanPosition.y - 50, 130, 35), "CLEAN!", 20, arcadeTeal, FontStyle.Bold, TextAnchor.MiddleCenter);
            if (warningClock > 0 && ActiveFight)
            {
                var area = new Rect(warningPosition.x - 60, warningPosition.y - 60, 120, 120);
                Fill(area, new Color(1, .22f, .12f, .16f + .12f * Mathf.Sin(pulse * 20)));
                Fill(new Rect(area.x, area.y, area.width, 3), arcadeCoral); Fill(new Rect(area.x, area.yMax - 3, area.width, 3), arcadeCoral);
                Fill(new Rect(area.x, area.y, 3, area.height), arcadeCoral); Fill(new Rect(area.xMax - 3, area.y, 3, area.height), arcadeCoral);
                Text(new Rect(area.x, area.y - 24, area.width, 27), "MOVE!", 18, arcadeCoral, FontStyle.Bold, TextAnchor.MiddleCenter);
            }
            if (encounter == Encounter.Goblins) for (int i = 0; i < GoblinTotal; i++)
                {
                    var p = goblinPositions[i];
                    if (goblinHealth[i] > 0)
                    { Fill(new Rect(p.x - 28, p.y - 90, 56, 6), arcadeNavy); Fill(new Rect(p.x - 27, p.y - 89, 54 * goblinHealth[i] / 2, 4), arcadeGold); Text(new Rect(p.x - 65, p.y - 125, 130, 30), goblinStun[i] > 0 ? "STUNNED!" : "SHOO!", 17, goblinStun[i] > 0 ? arcadeTeal : arcadeCream, FontStyle.Bold, TextAnchor.MiddleCenter); }
                    if (goblinImpact[i] > 0) Text(new Rect(p.x - 60, p.y - 155, 120, 45), "POW!", 28, arcadeGold, FontStyle.Bold, TextAnchor.MiddleCenter);
                }
            if (encounter == Encounter.Salmah && !resolved && hasGun && salmahRecovery > 0)
                Text(new Rect(salmah.x - 70, salmah.y - 120, 140, 30), "FLANK!", 20, arcadeGold, FontStyle.Bold, TextAnchor.MiddleCenter);
            if (encounter == Encounter.Bo && boImpact > 0)
            { var p = ShopLayout.ToGame(shop.boCarTarget.position); Text(new Rect(p.x - 80, p.y - 120, 160, 45), "BOOM!", 30, arcadeGold, FontStyle.Bold, TextAnchor.MiddleCenter); }
        }
    }
}
