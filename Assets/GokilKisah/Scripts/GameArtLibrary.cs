using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GokilKisah
{
    // Cropped, mobile-sized runtime assets; the original artist exports remain in ArtSources.
    public static class GameArtLibrary
    {
        static readonly Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        static readonly Dictionary<string, Sprite[]> walks = new Dictionary<string, Sprite[]>();
        static readonly Dictionary<string, Sprite> idles = new Dictionary<string, Sprite>();
        public static bool IsCustomer(string skin)=>skin=="Cat"||skin=="Girl"||skin=="Zomboy"||skin=="Zomgirl";
        public static Sprite CustomerPose(string skin,bool walking,int frame)
        {
            string key="Customer/"+skin+(walking?"Walk":"Idle");
            if(!walks.TryGetValue(key,out var frames))walks[key]=frames=Resources.LoadAll<Sprite>("NovemberArt/Customers/"+skin+(walking?"Walk":"Idle")).OrderBy(s=>s.name).ToArray();
            return frames.Length>0?frames[Mathf.Abs(frame)%frames.Length]:Idle(skin);
        }
        public static int CustomerWalkCount(string skin){CustomerPose(skin,true,0);return Mathf.Max(1,walks["Customer/"+skin+"Walk"].Length);}
        public static int WalkFrameCount(string skin)=>skin=="Toyol"?41:49;
        public static Sprite KeeperIdle(string skin,int frame)
        {
            const int columns=6, rows=4;
            string key=skin+"Idle";
            if(!walks.TryGetValue(key,out var frames))
            {
                var sheet=Resources.Load<Texture2D>("NovemberArt/KeeperIdle/"+skin);
                if(!sheet)return Idle(skin);
                int cellWidth=sheet.width/columns,cellHeight=sheet.height/rows;
                walks[key]=frames=new Sprite[columns*rows];
                for(int i=0;i<frames.Length;i++)
                {
                    var rect=new Rect(i%columns*cellWidth+1,(rows-1-i/columns)*cellHeight+1,cellWidth-2,cellHeight-2);
                    frames[i]=Sprite.Create(sheet,rect,new Vector2(.5f,0),rect.height/1.88f,0,SpriteMeshType.FullRect);
                    frames[i].name=skin+" idle "+i;
                }
            }
            return frames[frame%frames.Length];
        }
        public static Sprite Idle(string skin)
        {
            if(!idles.TryGetValue(skin,out var sprite))
                idles[skin]=sprite=Resources.Load<Sprite>("PresentationSprites/"+skin+"Static");
            return sprite?sprite:Walk(skin,0);
        }
        public static Texture2D UI(string name)
        {
            if (!textures.TryGetValue(name, out var value))
                textures[name] = value = Resources.Load<Texture2D>("NovemberArt/UI/" + name);
            return value;
        }
        public static Sprite Walk(string skin, int frame)
        {
            if (!walks.TryGetValue(skin, out var frames))
            {
                frames = new Sprite[WalkFrameCount(skin)];
                for (int i = 0; i < frames.Length; i++)
                    frames[i] = Resources.Load<Sprite>("NovemberArt/Walk/" + skin + i);
                walks[skin] = frames;
            }
            return frames[Mathf.Abs(frame) % frames.Length];
        }
    }
}
