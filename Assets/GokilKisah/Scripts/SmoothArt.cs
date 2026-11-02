using UnityEngine;
namespace GokilKisah
{
    public static class SmoothArt
    {
        static readonly System.Collections.Generic.Dictionary<string,Sprite> cache=new System.Collections.Generic.Dictionary<string,Sprite>();
        public static Sprite Load(string name)
        {
            // Supplied keeper art currently has one pose per character.
            if(name.StartsWith("Sam_",System.StringComparison.Ordinal))name="SamStatic";
            if(name.StartsWith("Zack_",System.StringComparison.Ordinal))name="ZackStatic";
            if(cache.TryGetValue(name,out var sprite)&&sprite)return sprite;
            sprite=Resources.Load<Sprite>("PresentationSprites/"+name);if(sprite){cache[name]=sprite;return sprite;}
            var texture=Resources.Load<Texture2D>("OnlineArt/"+name)??Resources.Load<Texture2D>("SmoothArt/"+name);if(!texture)return null;
            texture.filterMode=FilterMode.Bilinear;
            sprite=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f),texture.width);cache[name]=sprite;return sprite;
        }
    }
}
