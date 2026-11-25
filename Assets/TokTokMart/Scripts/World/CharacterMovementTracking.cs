using UnityEngine;

namespace TokTokMart
{
    public sealed partial class ShiftController
    {
        Vector2 lastPlayer, lastHelper, playerMotion, helperMotion;
        int playerFacing = 24, helperFacing = 24;
        void TrackArtistMotion()
        {
            playerMotion = player - lastPlayer; helperMotion = helper - lastHelper; lastPlayer = player; lastHelper = helper;
            if (playerMotion.sqrMagnitude > .01f) playerFacing = Facing(playerMotion);
            if (helperMotion.sqrMagnitude > .01f) helperFacing = Facing(helperMotion);
        }
        int Facing(Vector2 d) { return Mathf.Abs(d.x) > Mathf.Abs(d.y) ? (d.x < 0 ? 23 : 26) : (d.y < 0 ? 25 : 24); }
    }
}
