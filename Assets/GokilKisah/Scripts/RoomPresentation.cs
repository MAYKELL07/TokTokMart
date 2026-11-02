using System.Collections.Generic;
using UnityEngine;

namespace GokilKisah
{
    // Presentation only: room masks and contact shadows never change collisions or AI.
    public sealed class RoomPresentation : MonoBehaviour
    {
        public bool InStorage { get; private set; }
        public float MainOpacity => mainMask ? mainMask.color.a : 0;
        public float StorageOpacity => storageMask ? storageMask.color.a : 0;
        ShopLayout layout;
        SpriteRenderer mainMask, storageMask;
        Sprite flat, soft;
        Texture2D flatTexture, softTexture;
        readonly Dictionary<CharacterSpriteAnimator,SpriteRenderer> actorShadows=new Dictionary<CharacterSpriteAnimator,SpriteRenderer>();
        readonly List<CharacterSpriteAnimator> expired=new List<CharacterSpriteAnimator>();
        float nextScan;
        public void Initialize(ShopLayout shop)
        {
            if(layout)return;layout=shop;
            flatTexture=new Texture2D(1,1);flatTexture.SetPixel(0,0,Color.white);flatTexture.Apply();
            flat=Sprite.Create(flatTexture,new Rect(0,0,1,1),Vector2.one*.5f,1);
            softTexture=new Texture2D(64,32,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
            for(int y=0;y<32;y++)for(int x=0;x<64;x++)
            {
                float radius=new Vector2((x+.5f-32)/32,(y+.5f-16)/16).magnitude;
                softTexture.SetPixel(x,y,new Color(0,0,0,Mathf.Pow(Mathf.Clamp01(1-radius),1.6f)*.32f));
            }
            softTexture.Apply();soft=Sprite.Create(softTexture,new Rect(0,0,64,32),Vector2.one*.5f,64);
            var root=shop.transform.Find("Reference shop layout");
            foreach(var r in root.GetComponentsInChildren<SpriteRenderer>())
            {
                string n=r.name;
                bool north=n.StartsWith("North wall"),south=n.StartsWith("Facade ")||n=="Storage south wall fade"||n=="Storage door beside opening";
                if(north||south)
                {
                    var fade=r.GetComponent<SoftOccluder>();if(!fade)fade=r.gameObject.AddComponent<SoftOccluder>();
                    fade.kind=north?SoftOccluder.OccluderKind.NorthWall:SoftOccluder.OccluderKind.SouthWall;
                    fade.silhouette=r;fade.artwork=new[]{r};fade.minimumOpacity=.18f;fade.wallReach=.85f;
                }
                if(n=="Artist shelf fixture"||n.StartsWith("Fridge ")||n=="Counter long top"||n=="Counter return")
                {
                    var b=r.bounds;var shadow=Make(n+" contact shadow",soft,-850);
                    shadow.transform.position=new Vector3(b.center.x+.08f,b.min.y-.04f,0);
                    shadow.transform.localScale=new Vector3(b.size.x*1.06f,Mathf.Min(.65f,b.size.y*.38f),1);
                }
            }
            var main=shop.walkableArea.bounds;var storage=shop.storageArea.bounds;
            float boundary=main.max.y;
            mainMask=Mask("Main room shade",new Vector2(main.min.x-.35f,main.min.y-.65f),new Vector2(main.max.x+.35f,boundary));
            // Keep the doorway reveal clear so the active character's head is never shaded.
            storageMask=Mask("Storage room shade",new Vector2(storage.min.x-.15f,boundary+1.1f),new Vector2(storage.max.x+.15f,storage.max.y+.15f));
        }
        SpriteRenderer Make(string title,Sprite sprite,int order)
        {
            var go=new GameObject(title);go.transform.SetParent(transform,false);
            var r=go.AddComponent<SpriteRenderer>();r.sprite=sprite;r.sortingOrder=order;return r;
        }
        SpriteRenderer Mask(string title,Vector2 min,Vector2 max)
        {
            var r=Make(title,flat,30000);r.transform.position=(min+max)*.5f;
            r.transform.localScale=new Vector3(max.x-min.x,max.y-min.y,1);r.color=new Color(.025f,.035f,.05f,0);return r;
        }
        public void Tick(Vector2 feet,bool playing,float dt)
        {
            if(!layout||!layout.storageArea)return;
            float threshold=layout.walkableArea.bounds.max.y;
            if(feet.y>threshold+.1f)InStorage=true;else if(feet.y<threshold-.1f)InStorage=false;
            float blend=1-Mathf.Exp(-Mathf.Min(dt,.05f)/.16f);
            Shade(mainMask,playing&&InStorage?.94f:0,blend);
            Shade(storageMask,playing&&!InStorage?.94f:0,blend);
            if(Time.unscaledTime>=nextScan)
            {
                nextScan=Time.unscaledTime+.35f;
                foreach(var actor in layout.GetComponentsInChildren<CharacterSpriteAnimator>(true))if(!actorShadows.ContainsKey(actor))
                    actorShadows.Add(actor,Make(actor.name+" contact shadow",soft,-820));
            }
            expired.Clear();
            foreach(var pair in actorShadows)
            {
                var actor=pair.Key;var shadow=pair.Value;
                if(!actor){Destroy(shadow.gameObject);expired.Add(actor);continue;}
                shadow.enabled=actor.gameObject.activeInHierarchy&&actor.artwork&&actor.artwork.enabled;
                var shadowFeet=actor.transform.position;
                if(actor.artwork)shadowFeet.y=actor.artwork.bounds.min.y+.015f;
                shadow.transform.position=shadowFeet+Vector3.right*.04f;
                float width=actor.artwork?Mathf.Clamp(actor.artwork.bounds.size.x*.9f,.3f,.65f):.5f;
                shadow.transform.localScale=new Vector3(width,.24f,1);
            }
            foreach(var actor in expired)actorShadows.Remove(actor);
        }
        static void Shade(SpriteRenderer r,float target,float blend){var color=r.color;color.a=Mathf.Lerp(color.a,target,blend);r.color=color;}
        void OnDestroy()
        {
            if(flat)Destroy(flat);if(soft)Destroy(soft);if(flatTexture)Destroy(flatTexture);if(softTexture)Destroy(softTexture);
        }
    }
}
