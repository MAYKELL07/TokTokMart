using UnityEngine;
namespace GokilKisah
{
    [DisallowMultipleComponent]
    public sealed class SoftOccluder : MonoBehaviour
    {
        public enum OccluderKind { Shelf, SouthWall, NorthWall }
        public OccluderKind kind;
        public SpriteRenderer silhouette;
        public SpriteRenderer[] artwork;
        [Range(.05f,.8f)] public float minimumOpacity=.22f;
        [Min(.1f)] public float wallReach=1.05f;
        [Min(.01f)] public float fadeOutTime=.07f, restoreTime=.16f;
        Color[] originalColors;
        float visibility=1,hold;
        void Initialize()
        {
            if(originalColors!=null&&originalColors.Length==artwork.Length)return;
            originalColors=new Color[artwork.Length];
            for(int i=0;i<artwork.Length;i++)originalColors[i]=artwork[i]?artwork[i].color:Color.white;
        }
        public float DesiredOpacity(Vector2 feet,Bounds playerBounds)
        {
            if(!silhouette)return 1;
            var b=silhouette.bounds;
            float strength;
            if(kind!=OccluderKind.Shelf)
            {
                // The south face hides a player on the room side; keep it solid once they walk below it.
                float dx=Mathf.Max(0,Mathf.Max(b.min.x-feet.x,feet.x-b.max.x));
                float dy=Mathf.Max(0,Mathf.Max(b.min.y-feet.y,feet.y-b.max.y));
                float proximity=1-Mathf.SmoothStep(0,1,Mathf.Sqrt(dx*dx+dy*dy)/Mathf.Max(.01f,wallReach));
                // Both faces fade at the player's silhouette, including when walking below
                // the north frieze. Wall artwork can extend well above its collision base.
                float horizontal=playerBounds.max.x>=b.min.x-.12f&&playerBounds.min.x<=b.max.x+.12f?1:0;
                float overlap=playerBounds.max.y>=b.min.y-.12f&&playerBounds.min.y<=b.max.y+.12f?1:0;
                strength=Mathf.Max(proximity,horizontal*overlap);
            }
            else
            {
                float dx=Mathf.Max(0,Mathf.Max(b.min.x-playerBounds.max.x,playerBounds.min.x-b.max.x));
                float dy=Mathf.Max(0,Mathf.Max(b.min.y-playerBounds.max.y,playerBounds.min.y-b.max.y));
                float overlap=1-Mathf.SmoothStep(0,1,Mathf.Max(dx,dy)/.22f);
                float behind=Mathf.SmoothStep(0,1,Mathf.InverseLerp(transform.position.y-.1f,transform.position.y+.12f,feet.y));
                strength=overlap*behind;
            }
            return Mathf.Lerp(1,minimumOpacity,strength);
        }
        public void Tick(Vector2 feet,Bounds playerBounds,bool playing,float dt)
        {
            if(artwork==null)return;Initialize();
            float target=playing?DesiredOpacity(feet,playerBounds):1;
            if(target<.98f)hold=.08f;else if(playing&&hold>0){hold-=dt;target=visibility;}
            float time=target<visibility?fadeOutTime:restoreTime;
            visibility=Mathf.Lerp(visibility,target,1-Mathf.Exp(-Mathf.Min(dt,.05f)/Mathf.Max(.01f,time)));
            if(Mathf.Abs(visibility-target)<.001f)visibility=target;
            for(int i=0;i<artwork.Length;i++)if(artwork[i])
            {
                var color=originalColors[i];
                // Stock remains slightly more visible than the shelf surface.
                color.a*=artwork[i]==silhouette?visibility:Mathf.Lerp(.15f,1,visibility);
                artwork[i].color=color;
            }
        }
        public void ResetOpacity()
        {
            visibility=1;hold=0;
            if(originalColors==null||artwork==null)return;
            for(int i=0;i<artwork.Length&&i<originalColors.Length;i++)if(artwork[i])artwork[i].color=originalColors[i];
        }
        void OnDisable(){ResetOpacity();}
    }
}
