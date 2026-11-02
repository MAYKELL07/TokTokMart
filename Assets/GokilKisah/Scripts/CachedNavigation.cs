using System;
using System.Collections.Generic;
using UnityEngine;

namespace GokilKisah
{
    public sealed partial class StoreGameController
    {
        struct CollisionShape {public Collider2D collider;public Rect bounds;public bool axisAlignedBox;}
        readonly List<CollisionShape> collisionCache=new List<CollisionShape>();
        Rect mainFloorBounds,storageFloorBounds;
        bool hasStorageFloor;
        float geometryPollAt;
        readonly Dictionary<int,int[]> navigationFlows=new Dictionary<int,int[]>();
        Vector2 navigationOrigin;
        int navigationWidth,navigationHeight;
        const float NavigationCell=20;
        bool[] navigationOpen;
        byte[] navigationEdges;
        int[] navigationQueue;
        static Rect BoundsRect(Bounds b)=>Rect.MinMaxRect(b.min.x,b.min.y,b.max.x,b.max.y);
        static bool Contains(Rect r,Vector2 p)=>p.x>=r.xMin&&p.x<=r.xMax&&p.y>=r.yMin&&p.y<=r.yMax;
        bool CachedFloor(Vector2 p)=>Contains(mainFloorBounds,p)||hasStorageFloor&&Contains(storageFloorBounds,p);
        static float RectDistanceSquared(Rect r,Vector2 p)
        {float x=Mathf.Max(r.xMin-p.x,Mathf.Max(0,p.x-r.xMax)),y=Mathf.Max(r.yMin-p.y,Mathf.Max(0,p.y-r.yMax));return x*x+y*y;}
        void CacheCollisionGeometry()
        {
            mainFloorBounds=BoundsRect(shop.walkableArea.bounds);hasStorageFloor=shop.storageArea;
            if(hasStorageFloor)storageFloorBounds=BoundsRect(shop.storageArea.bounds);
            var unique=new HashSet<Collider2D>();collisionCache.Clear();
            foreach(var c in blockingShapes)if(c&&c.enabled&&c.gameObject.activeInHierarchy&&!c.isTrigger)unique.Add(c);
            foreach(var obstacle in sceneObstacles)if(obstacle&&obstacle.shapes!=null)
                foreach(var c in obstacle.shapes)if(c&&c.enabled&&c.gameObject.activeInHierarchy)unique.Add(c);
            foreach(var c in unique)
            {
                float z=c.transform.eulerAngles.z;
                collisionCache.Add(new CollisionShape{collider=c,bounds=BoundsRect(c.bounds),axisAlignedBox=c is BoxCollider2D&&Mathf.Abs(z-Mathf.Round(z/90)*90)<.01f});
            }
            foreach(var c in blockingShapes)if(c)c.transform.hasChanged=false;
            shop.walkableArea.transform.hasChanged=false;if(shop.storageArea)shop.storageArea.transform.hasChanged=false;
            navigationOpen=null;navigationEdges=null;navigationFlows.Clear();geometryPollAt=Time.unscaledTime+.5f;
        }
        bool CachedSceneWalkable(Vector2 p)
        {
            var wp=new Vector2(p.x*.01f,-p.y*.01f);const float radius=FootRadius*.01f;
            if(!CachedFloor(wp+Vector2.left*radius)||!CachedFloor(wp+Vector2.right*radius)||!CachedFloor(wp+Vector2.up*radius)||!CachedFloor(wp+Vector2.down*radius))return false;
            const float rr=(radius-.00001f)*(radius-.00001f);
            for(int i=0;i<collisionCache.Count;i++)
            {
                var shape=collisionCache[i];
                if(shape.axisAlignedBox){if(RectDistanceSquared(shape.bounds,wp)<rr)return false;}
                else if(((Vector2)shape.collider.ClosestPoint(wp)-wp).sqrMagnitude<rr)return false;
            }
            return true;
        }
        bool GeometryWasEdited()
        {
            if(!Application.isEditor||Time.unscaledTime<geometryPollAt)return false;
            geometryPollAt=Time.unscaledTime+.5f;
            foreach(var c in blockingShapes)if(c&&c.transform.hasChanged)return true;
            foreach(var shape in collisionCache)
                if(!shape.collider||!shape.collider.enabled||!shape.collider.gameObject.activeInHierarchy||BoundsRect(shape.collider.bounds)!=shape.bounds)return true;
            return false;
        }
        static float PointSegmentDistanceSquared(Vector2 p,Vector2 a,Vector2 b)
        {var d=b-a;float t=d.sqrMagnitude>0?Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude):0;return (p-a-d*t).sqrMagnitude;}
        static bool SegmentBox(Vector2 a,Vector2 b,Rect r)
        {
            float lo=0,hi=1;var d=b-a;
            for(int axis=0;axis<2;axis++)
            {
                float v=axis==0?d.x:d.y,s=axis==0?a.x:a.y,min=axis==0?r.xMin:r.yMin,max=axis==0?r.xMax:r.yMax;
                if(Mathf.Abs(v)<.0000001f){if(s<min||s>max)return false;continue;}
                float t1=(min-s)/v,t2=(max-s)/v;if(t1>t2){float swap=t1;t1=t2;t2=swap;}
                lo=Mathf.Max(lo,t1);hi=Mathf.Min(hi,t2);if(lo>hi)return false;
            }
            return true;
        }
        bool StaticSegmentClear(Vector2 from,Vector2 target)
        {
            if(!CachedSceneWalkable(target))return false;
            var a=new Vector2(from.x*.01f,-from.y*.01f);var b=new Vector2(target.x*.01f,-target.y*.01f);
            const float radius=FootRadius*.01f-.00001f,rr=radius*radius;
            for(int i=0;i<collisionCache.Count;i++)
            {
                var shape=collisionCache[i];if(!shape.axisAlignedBox)continue;
                var r=shape.bounds;var expanded=Rect.MinMaxRect(r.xMin-radius,r.yMin-radius,r.xMax+radius,r.yMax+radius);
                if(!SegmentBox(a,b,expanded))continue;
                if(SegmentBox(a,b,r)||RectDistanceSquared(r,a)<rr||RectDistanceSquared(r,b)<rr||
                   PointSegmentDistanceSquared(new Vector2(r.xMin,r.yMin),a,b)<rr||PointSegmentDistanceSquared(new Vector2(r.xMin,r.yMax),a,b)<rr||
                   PointSegmentDistanceSquared(new Vector2(r.xMax,r.yMin),a,b)<rr||PointSegmentDistanceSquared(new Vector2(r.xMax,r.yMax),a,b)<rr)return false;
            }
            int samples=Mathf.CeilToInt(Vector2.Distance(from,target)/10);
            for(int i=1;i<samples;i++)if(!CachedSceneWalkable(Vector2.Lerp(from,target,(float)i/samples)))return false;
            return true;
        }
        Vector2 NavigationPoint(int index)=>navigationOrigin+new Vector2(index%navigationWidth,index/navigationWidth)*NavigationCell;
        int NavigationNeighbor(int index,int direction)
        {
            int x=index%navigationWidth,y=index/navigationWidth;
            if(direction==0)return x>0?index-1:-1;if(direction==1)return x+1<navigationWidth?index+1:-1;
            if(direction==2)return y>0?index-navigationWidth:-1;return y+1<navigationHeight?index+navigationWidth:-1;
        }
        void EnsureNavigationGrid()
        {
            if(navigationOpen!=null)return;
            var bounds=shop.MapBounds;navigationOrigin=new Vector2(bounds.min.x*100+FootRadius,-bounds.max.y*100+FootRadius);
            navigationWidth=Mathf.CeilToInt((bounds.size.x*100-FootRadius*2)/NavigationCell)+1;
            navigationHeight=Mathf.CeilToInt((bounds.size.y*100-FootRadius*2)/NavigationCell)+1;
            int count=navigationWidth*navigationHeight;navigationOpen=new bool[count];navigationEdges=new byte[count];navigationQueue=new int[count];
            for(int i=0;i<count;i++)navigationOpen[i]=CachedSceneWalkable(NavigationPoint(i));
            for(int i=0;i<count;i++)if(navigationOpen[i])for(int direction=0;direction<4;direction++)
            {int n=NavigationNeighbor(i,direction);if(n>=0&&navigationOpen[n]&&StaticSegmentClear(NavigationPoint(i),NavigationPoint(n)))navigationEdges[i]|=(byte)(1<<direction);}
        }
        int ConnectedNavigationNode(Vector2 p)
        {
            int nx=Mathf.Clamp(Mathf.RoundToInt((p.x-navigationOrigin.x)/NavigationCell),0,navigationWidth-1),ny=Mathf.Clamp(Mathf.RoundToInt((p.y-navigationOrigin.y)/NavigationCell),0,navigationHeight-1),best=-1;
            float bestDistance=float.PositiveInfinity;
            for(int y=Mathf.Max(0,ny-2);y<=Mathf.Min(navigationHeight-1,ny+2);y++)for(int x=Mathf.Max(0,nx-2);x<=Mathf.Min(navigationWidth-1,nx+2);x++)
            {
                int n=y*navigationWidth+x;if(!navigationOpen[n])continue;var point=NavigationPoint(n);float distance=(point-p).sqrMagnitude;
                if(distance<bestDistance&&StaticSegmentClear(p,point)){best=n;bestDistance=distance;}
            }
            return best;
        }
        int[] FlowTo(int goal)
        {
            if(navigationFlows.TryGetValue(goal,out var flow))return flow;
            flow=new int[navigationOpen.Length];for(int i=0;i<flow.Length;i++)flow[i]=-1;
            int head=0,tail=0;navigationQueue[tail++]=goal;flow[goal]=goal;
            while(head<tail)
            {
                int current=navigationQueue[head++];
                for(int direction=0;direction<4;direction++)if((navigationEdges[current]&(1<<direction))!=0)
                {int n=NavigationNeighbor(current,direction);if(flow[n]>=0)continue;flow[n]=current;navigationQueue[tail++]=n;}
            }
            // Bound the cache when customers choose slightly different approach points.
            if(navigationFlows.Count>64)navigationFlows.Clear();navigationFlows[goal]=flow;return flow;
        }
        Vector2 CachedNavigate(Vector2 from,Vector2 target,float distance)
        {
            if(distance<=0||!CachedSceneWalkable(target))return from;
            Vector2 waypoint=target;
            if(!StaticSegmentClear(from,target))
            {
                EnsureNavigationGrid();int start=ConnectedNavigationNode(from),goal=ConnectedNavigationNode(target);if(start<0||goal<0)return from;
                var flow=FlowTo(goal);if(flow[start]<0)return from;
                waypoint=NavigationPoint(flow[start]);if(!StaticSegmentClear(from,waypoint))waypoint=NavigationPoint(start);
            }
            var candidate=Vector2.MoveTowards(from,waypoint,distance);
            if(LineClear(from,candidate))return candidate;
            if(routingShopper!=null)
            {
                var forward=(waypoint-from).normalized;
                // Local avoidance handles shoppers and spills without rebuilding static routes.
                foreach(float angle in avoidanceAngles)
                {var d=(Vector2)(Quaternion.Euler(0,0,angle)*forward);candidate=from+d*Mathf.Min(distance,(waypoint-from).magnitude);if(LineClear(from,candidate))return candidate;}
            }
            return from;
        }
        static readonly float[] avoidanceAngles={45,-45,80,-80};
        bool TryCustomerDetour(Shopper shopper,out Vector2 waypoint)
        {
            waypoint=shopper.position;EnsureNavigationGrid();
            int start=ConnectedNavigationNode(shopper.position),goal=ConnectedNavigationNode(shopper.goal);
            if(start<0||goal<0)return false;
            var parent=new int[navigationOpen.Length];for(int i=0;i<parent.Length;i++)parent[i]=-1;
            var occupied=new bool[parent.Length];
            for(int i=0;i<occupied.Length;i++)if(navigationOpen[i])
                foreach(var other in customers)if(other!=shopper&&(NavigationPoint(i)-other.position).sqrMagnitude<32*32){occupied[i]=true;break;}
            int head=0,tail=0;navigationQueue[tail++]=start;parent[start]=start;
            while(head<tail&&parent[goal]<0)
            {
                int current=navigationQueue[head++];
                for(int direction=0;direction<4;direction++)if((navigationEdges[current]&(1<<direction))!=0)
                {
                    int next=NavigationNeighbor(current,direction);
                    if(parent[next]>=0||occupied[next])continue;
                    if(current==start&&!LineClear(shopper.position,NavigationPoint(next)))continue;
                    parent[next]=current;navigationQueue[tail++]=next;
                }
            }
            if(parent[goal]<0)return false;
            int node=goal;
            while(parent[node]!=start&&parent[node]!=node)node=parent[node];
            waypoint=NavigationPoint(node);return LineClear(shopper.position,waypoint);
        }
    }
}
