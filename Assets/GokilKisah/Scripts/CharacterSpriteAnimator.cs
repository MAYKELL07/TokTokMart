using System.Collections.Generic;
using UnityEngine;
namespace GokilKisah
{
    [DisallowMultipleComponent]
    public sealed class CharacterSpriteAnimator : MonoBehaviour
    {
        public SpriteRenderer artwork;
        public Transform heldStock, heldGun;
        public Texture2D atlas;
        [Tooltip("Turn off when using your own character sprite.")]
        public bool animate=true;
        public string smoothSkin="";
        public int skinRow;
        public int leftColumn=23, rightColumn=26, upColumn=25, downColumn=24;
        public float framesPerSecond=7;
        bool mirrorWalk;
        string displayedSkin;
        int displayedDirection=-1,displayedFrame=-1;

        public Vector2 LastGamePosition {get;private set;}
        static readonly Dictionary<string,Sprite> tiles=new Dictionary<string,Sprite>();
        public static Sprite AtlasSprite(Texture2D texture,int x,int y)
        {
            string key=texture.GetInstanceID()+":"+x+":"+y;
            if(tiles.TryGetValue(key,out var sprite)&&sprite)return sprite;
            sprite=Sprite.Create(texture,new Rect(x*17,texture.height-y*17-16,16,16),new Vector2(.5f,.5f),100);
            sprite.name=texture.name+" "+x+","+y;tiles[key]=sprite;return sprite;
        }
        public void Apply(Vector2 position,int facing,int row,Vector2 motion,float clock)
        {
            transform.position=ShopLayout.ToWorld(position);LastGamePosition=position;
            if(artwork&&!string.IsNullOrEmpty(smoothSkin))
            {
                int direction=facing==23?0:facing==26?1:facing==25?2:3;
                int frame=motion.sqrMagnitude>.001f?(int)(clock*framesPerSecond)%3:0;
                string skin=smoothSkin=="Keeper"?(row<6?"Sam":"Zack"):smoothSkin;
                if(skin=="Sam"||skin=="Zack"||skin=="Toyol")
                {
                    // Supplied walks face one side. Mirror horizontally, retain that pose for vertical travel.
                    if(direction==0)mirrorWalk=false;else if(direction==1)mirrorWalk=true;
                    bool moving=motion.sqrMagnitude>.001f;
                    var walk=NovemberArt.Walk(skin,moving?(int)(clock*12)%9:0);
                    artwork.sprite=walk?walk:SmoothArt.Load(skin=="Toyol"?"Goblin_3_0":skin+"_3_0");
                    artwork.flipX=mirrorWalk;return;
                }
                if(displayedSkin!=skin||displayedDirection!=direction||displayedFrame!=frame||!artwork.sprite)
                {artwork.sprite=SmoothArt.Load(skin+"_"+direction+"_"+frame);displayedSkin=skin;displayedDirection=direction;displayedFrame=frame;}
                return;
            }
            if(!animate||!atlas||!artwork)return;
            int column=facing==23?leftColumn:facing==26?rightColumn:facing==25?upColumn:downColumn;
            int pose=motion.sqrMagnitude>.001f?(int)(clock*framesPerSecond)%3:0;
            artwork.sprite=AtlasSprite(atlas,column,row+pose);
        }
        public bool WasMovedInScene()=>Vector2.Distance(ShopLayout.ToGame(transform.position),LastGamePosition)>.05f;
    }
}

