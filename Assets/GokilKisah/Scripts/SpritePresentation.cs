using UnityEngine;

namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        Texture2D urbanArt, indoorArt, characterArt, nailArt;
        Texture2D shopFloor, streetFloor;
        Vector2 playerMotion, helperMotion, lastPlayer, lastHelper;
        int playerFacing=24, helperFacing=24;

        void LoadArtistArt()
        {
            urbanArt=Resources.Load<Texture2D>("ArtistArt/Urban");
            indoorArt=Resources.Load<Texture2D>("ArtistArt/Indoors");
            characterArt=Resources.Load<Texture2D>("ArtistArt/Characters");
            nailArt=Resources.Load<Texture2D>("ArtistArt/NailGun");
            foreach(var atlas in new[]{urbanArt,indoorArt,characterArt,nailArt})
                if(atlas){atlas.filterMode=FilterMode.Point;atlas.wrapMode=TextureWrapMode.Clamp;}
            if(!urbanArt||!indoorArt||!characterArt||!nailArt)
                Debug.LogError("TOK MART: a credited Kenney art asset is missing.");
            if(urbanArt&&urbanArt.isReadable)
            {
                if(shopFloor)Destroy(shopFloor);if(streetFloor)Destroy(streetFloor);
                shopFloor=FloorTile(1,4);streetFloor=FloorTile(9,4);
            }
        }
        Texture2D FloorTile(int x,int y)
        {
            // Crop the original 16px tile at runtime. Repeating one texture eliminates scaled atlas seams.
            var tile=new Texture2D(16,16,TextureFormat.RGBA32,false){filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Repeat};
            tile.SetPixels(urbanArt.GetPixels(x*17,urbanArt.height-y*17-16,16,16));tile.Apply();return tile;
        }
        void Tile(Texture2D atlas,int x,int y,Rect area,int width=1,int height=1)
        {
            if(!atlas)return;
            // The original sheets are untouched; Unity samples the selected tile region.
            // Sample pixel centres so scaled atlas tiles cannot bleed into transparent gutters.
            float px=x*17+.5f,py=y*17+.5f,w=width*17-2,h=height*17-2;
            EditableTextureUV(area,atlas,new Rect(px/atlas.width,1-(py+h)/atlas.height,w/atlas.width,h/atlas.height),true);
        }
        void TrackArtistMotion()
        {
            playerMotion=player-lastPlayer;helperMotion=helper-lastHelper;lastPlayer=player;lastHelper=helper;
            if(playerMotion.sqrMagnitude>.01f)playerFacing=Facing(playerMotion);
            if(helperMotion.sqrMagnitude>.01f)helperFacing=Facing(helperMotion);
        }
        int Facing(Vector2 d){return Mathf.Abs(d.x)>Mathf.Abs(d.y)?(d.x<0?23:26):(d.y<0?25:24);}
        void DrawArtistActor(Vector2 p,string title,bool ai)
        {
            Rect area=new Rect(p.x-32,p.y-58,64,64);
            Fill(new Rect(p.x-17,p.y+1,34,5),new Color(.14f,.13f,.16f,.2f), uiKey:"SpritePresentation_2698");
            if(title=="GOBLIN")Tile(characterArt,0,3,area);
            else if(title=="K-KUNG IDOL")Tile(characterArt,0,11,area);
            else
            {
                int row=title=="ZACK"?6:title=="UNCLE RAY"?12:title=="MISS SALMAH"?3:title=="SAM"?0:9;
                int col=title=="MISS SALMAH"?23:24;
                Vector2 movement=ai?helperMotion:playerMotion;
                if(title=="SAM"||title=="ZACK") {col=ai?helperFacing:playerFacing;if(selected&&movement.sqrMagnitude>.01f)row+=(int)(pulse*7)%3;}
                Tile(urbanArt,col,row,area);
            }
            if(showNames&&!string.IsNullOrEmpty(title))
            {
                float labelY=p.y-81;
                string tag=title+(ai?" / AI":"");float labelW=Mathf.Max(64,tag.Length*8);
                Fill(new Rect(p.x-labelW/2,labelY,labelW,21),ink, uiKey:"SpritePresentation_3561");
                Text(new Rect(p.x-labelW/2,labelY,labelW,21),tag,12,Color.white,FontStyle.Bold,TextAnchor.MiddleCenter, uiKey:"SpritePresentation_3628");
            }
        }
        void DrawArtistStation(Vector2 p,string symbol)
        {
            Rect icon=new Rect(p.x-28,p.y-30,56,56);
            switch(symbol)
            {
                case "+":Tile(urbanArt,3,10,icon);break; // stock crate
                case "=":Tile(indoorArt,4,14,icon);Tile(urbanArt,10,12,new Rect(p.x-16,p.y-39,32,32));break;
                case "R":Tile(urbanArt,11,16,icon);break; // wall call terminal stand-in
                case "L":Tile(urbanArt,13,12,icon);break;
                case "C":Tile(urbanArt,10,9,icon);break;
                case "N":if(nailArt)EditableTexture(new Rect(p.x-30,p.y-11,60,23),nailArt,ScaleMode.ScaleToFit);break;
            }
        }
        void DrawArtistFloor()
        {
            if(shopFloor)EditableTextureUV(new Rect(320,187,1035,548),shopFloor,new Rect(0,0,1035/48f,548/48f));
            for(int x=320;x<1355;x+=48)Tile(urbanArt,1,3,new Rect(x,175,48,36));
            for(int y=211;y<735;y+=48){Tile(urbanArt,0,4,new Rect(307,y,26,48));Tile(urbanArt,2,4,new Rect(1343,y,26,48));}
            Tile(urbanArt,8,13,new Rect(736,704,144,40),3,2);
            if(streetFloor)EditableTextureUV(new Rect(1390,224,165,519),streetFloor,new Rect(0,0,165/48f,519/48f));
        }
        void DrawArtistShelf(int index)
        {
            Rect r=shelves[index];
            for(int j=0;j<5;j++)Tile(indoorArt,j<shelfStock[index]?(index%2==0?5:6):3,15,new Rect(r.x+j*41,r.y-16,41,71));
            if(Vector2.Distance(player,ShelfPoint(index))<90)
            {
                Panel(new Rect(r.center.x-46,r.y-45,92,25),surface, uiKey:"SpritePresentation_5314");
                Text(new Rect(r.center.x-46,r.y-44,92,24),shelfStock[index]+" / 5 stock",14,paper,FontStyle.Bold,TextAnchor.MiddleCenter, uiKey:"SpritePresentation_5383");
            }
        }
    }
}
