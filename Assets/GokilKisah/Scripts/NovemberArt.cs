using System.Collections.Generic;
using UnityEngine;

namespace GokilKisah
{
    // Cropped, mobile-sized runtime assets; the original artist exports remain in ArtSources.
    public static class NovemberArt
    {
        static readonly Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        static readonly Dictionary<string, Sprite[]> walks = new Dictionary<string, Sprite[]>();
        static readonly Dictionary<string, Sprite> idles = new Dictionary<string, Sprite>();
        public static int WalkFrameCount(string skin)=>skin=="Toyol"?41:49;
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
