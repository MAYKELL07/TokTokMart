using UnityEngine;
namespace GokilKisah
{
    // A wordless expression cue; no shopping/status labels above customers.
    [DefaultExecutionOrder(110)]
    public sealed class CustomerMoodPresentation : MonoBehaviour
    {
        CharacterSpriteAnimator actor;
        SpriteRenderer mark;
        Color normal;
        float frustration;
        static Sprite angerSprite;
        void Awake()
        {
            actor=GetComponent<CharacterSpriteAnimator>();normal=actor.artwork.color;
            if(!angerSprite)
            {
                var t=new Texture2D(32,32,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
                var points=new[]{new Vector2(5,12),new Vector2(12,12),new Vector2(12,5),new Vector2(20,5),new Vector2(20,12),new Vector2(27,12),new Vector2(27,20),new Vector2(20,20),new Vector2(20,27),new Vector2(12,27),new Vector2(12,20),new Vector2(5,20)};
                for(int y=0;y<32;y++)for(int x=0;x<32;x++)
                {
                    float distance=99;var p=new Vector2(x,y);
                    for(int corner=0;corner<4;corner++)for(int edge=0;edge<2;edge++)
                    {var a=points[corner*3+edge];var b=points[corner*3+edge+1];var d=b-a;distance=Mathf.Min(distance,(p-a-d*Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude)).magnitude);}
                    t.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(2.3f-distance)));
                }
                t.Apply();angerSprite=Sprite.Create(t,new Rect(0,0,32,32),Vector2.one*.5f,100);
            }
            var go=new GameObject("Anger expression");go.transform.SetParent(transform,false);
            mark=go.AddComponent<SpriteRenderer>();mark.sprite=angerSprite;mark.color=new Color(.94f,.18f,.12f);mark.enabled=false;
        }
        public void SetFrustration(float amount){frustration=amount;}
        void LateUpdate()
        {
            if(!actor||!actor.artwork)return;
            float annoyed=Mathf.InverseLerp(.45f,.85f,frustration);
            actor.artwork.color=Color.Lerp(normal,new Color(1,.68f,.57f,normal.a),annoyed*.45f);
            mark.enabled=frustration>=.75f&&actor.artwork.enabled;
            if(!mark.enabled)return;
            var b=actor.artwork.bounds;mark.transform.position=new Vector3(b.max.x+.035f,b.max.y-.06f,0);
            float beat=1+Mathf.Sin(Time.time*8)*.08f;mark.transform.localScale=Vector3.one*beat*.7f;
            mark.sortingOrder=actor.artwork.sortingOrder+3;
        }
    }
}
