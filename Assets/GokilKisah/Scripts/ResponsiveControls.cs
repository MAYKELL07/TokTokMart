using UnityEngine;
namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        [Header("Movement response")]
        [Min(100)] public float moveAcceleration=5200, turnAcceleration=8500;
        float bufferedActionUntil=-1;
        void ApplyResponsiveMovement(Vector2 direction,float speed,float dt)
        {
            if(skillType>=0){playerVelocity=Vector2.zero;return;}
            if(direction.sqrMagnitude<.0001f){playerVelocity=Vector2.zero;return;}
            var desired=Vector2.ClampMagnitude(direction,1)*speed;
            bool reversing=Vector2.Dot(playerVelocity,desired)<0;
            if(reversing)playerVelocity=Vector2.zero;
            float acceleration=reversing?turnAcceleration:moveAcceleration;
            playerVelocity=Vector2.MoveTowards(playerVelocity,desired,acceleration*dt);
            var before=player;var requested=playerVelocity*dt;MovePlayer(requested);
            var actual=player-before;
            if(Mathf.Abs(actual.x-requested.x)>.01f)playerVelocity.x=0;
            if(Mathf.Abs(actual.y-requested.y)>.01f)playerVelocity.y=0;
        }
        bool InteractionInReach()
        {
            if(encounter==Encounter.Goblins&&!resolved&&Vector2.Distance(player,phone)<76)return true;
            if(encounter==Encounter.KKung&&!resolved&&Vector2.Distance(player,lightSwitch)<70)return true;
            if(encounter==Encounter.Salmah&&!hasGun&&Vector2.Distance(player,nailGun)<70)return true;
            if(encounter==Encounter.Bo&&!canKicked&&Vector2.Distance(player,can)<70)return true;
            return NearestSpill()>=0||Vector2.Distance(player,supply)<75||NearestShelf()>=0||Vector2.Distance(player,checkout)<135;
        }
        void ProcessBufferedAction()
        {
            if(bufferedActionUntil<0)return;
            if(skillType>=0||settingsOpen||autoPaused||mainMenu||!selected||gameOver||levelResults){bufferedActionUntil=-1;return;}
            if(InteractionInReach()||Time.unscaledTime>=bufferedActionUntil){bufferedActionUntil=-1;Interact();}
        }
    }
}
