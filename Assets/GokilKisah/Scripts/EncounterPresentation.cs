using UnityEngine;
namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        float ActorDepth(Vector2 p)=>p.y;
        float ShelfDepth(int i)=>shelves[i].yMax;
        // Only labels and encounter information belong to the HUD. All shop art is in the scene.
        void DrawStore()
        {
            if(roomPresentation&&roomPresentation.InStorage)
            {
                var room=shop.storageArea.bounds;
                Text(new Rect(room.center.x*100-240,-room.max.y*100+20,480,32),"STORAGE / BREAKER ROOM",16,ink,FontStyle.Bold,TextAnchor.MiddleCenter);
                return;
            }
            Text(new Rect(ShopDoor.x-90,ShopDoor.y,180,25),"WELCOME",16,new Color(.32f,.43f,.39f),FontStyle.Bold,TextAnchor.MiddleCenter);
            for(int i=0;i<shelves.Count;i++)if(Vector2.Distance(player,ShelfPoint(i))<70)
            {
                var r=shelves[i];Panel(new Rect(r.center.x-46,r.y-45,92,25),surface);
                Text(new Rect(r.center.x-46,r.y-44,92,24),shelfStock[i]+" / "+ShelfCapacity(i)+" stock",14,paper,FontStyle.Bold,TextAnchor.MiddleCenter);
            }
            if(shop.storageArea&&roomPresentation&&roomPresentation.InStorage){var room=shop.storageArea.bounds;Text(new Rect(room.center.x*100-240,-room.max.y*100+20,480,32),"STORAGE / BREAKER ROOM",16,ink,FontStyle.Bold,TextAnchor.MiddleCenter);}
            Text(new Rect(checkout.x-50,checkout.y+19,105,24),"Checkout",15,new Color(.25f,.34f,.31f),FontStyle.Bold,TextAnchor.MiddleCenter);
            if(encounter==Encounter.Goblins&&!rayCalled)Text(new Rect(phone.x-40,phone.y+23,95,25),"Wall phone",15,new Color(.25f,.34f,.31f),FontStyle.Bold,TextAnchor.MiddleCenter);
            if(encounter==Encounter.KKung&&!lightsOn){var b=shop.walkableArea.bounds;Fill(new Rect(b.min.x*100,-b.max.y*100,b.size.x*100,b.size.y*100),new Color(.13f,.08f,.22f,.10f));}
            if(encounter==Encounter.Salmah&&!resolved)
            {
                Panel(new Rect(salmah.x-40,salmah.y-83,83,26),surface);
                Text(new Rect(salmah.x-40,salmah.y-82,83,24),nailsHit+" / "+tuning.nailHits,15,paper,FontStyle.Bold,TextAnchor.MiddleCenter);
                if(hasGun&&CanHitSalmah()&&nailCooldown>.5f)Fill(new Rect(salmah.x+25,salmah.y-26,Mathf.Max(0,player.x-salmah.x-25),3),gold);
            }
        }
    }
}

