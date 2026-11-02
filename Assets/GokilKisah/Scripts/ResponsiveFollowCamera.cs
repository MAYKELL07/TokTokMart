using UnityEngine;
namespace GokilKisah
{
    [DisallowMultipleComponent, RequireComponent(typeof(Camera))]
    public sealed class ResponsiveFollowCamera : MonoBehaviour
    {
        [Range(.03f,.3f)] public float followTime=.085f;
        [Range(0,1)] public float lookAheadDistance=.42f;
        [Range(.03f,.5f)] public float lookAheadTime=.12f;
        public float originalGameplayZoom=2.35f, testGameplayZoom=2.8f;
        Vector2 center, velocity, lookAhead;
        bool initialized;
        void OnEnable(){initialized=false;velocity=Vector2.zero;lookAhead=Vector2.zero;}
        public void Follow(Camera camera, Vector2 feet, Vector2 worldVelocity, Bounds bounds, float dt, bool snap)
        {
            dt=Mathf.Clamp(dt,0,.05f);
            var body=feet+Vector2.up*.35f;
            var desiredLook=Vector2.ClampMagnitude(worldVelocity/2.6f,1.5f)*lookAheadDistance;
            if(snap||!initialized){center=body;lookAhead=Vector2.zero;velocity=Vector2.zero;initialized=true;}
            else lookAhead=Vector2.Lerp(lookAhead,desiredLook,1-Mathf.Exp(-dt/Mathf.Max(.01f,lookAheadTime)));
            var target=Clamp(body+lookAhead,bounds,camera);
            if(snap)center=target;
            else center=Vector2.SmoothDamp(center,target,ref velocity,followTime,Mathf.Infinity,dt);
            var clamped=Clamp(center,bounds,camera);
            if(!Mathf.Approximately(clamped.x,center.x))velocity.x=0;
            if(!Mathf.Approximately(clamped.y,center.y))velocity.y=0;
            center=clamped;camera.transform.position=new Vector3(center.x,center.y,-10);
        }
        static Vector2 Clamp(Vector2 p,Bounds bounds,Camera camera)
        {
            float hy=camera.orthographicSize,hx=hy*camera.aspect;
            p.x=bounds.size.x<=hx*2?bounds.center.x:Mathf.Clamp(p.x,bounds.min.x+hx,bounds.max.x-hx);
            p.y=bounds.size.y<=hy*2?bounds.center.y:Mathf.Clamp(p.y,bounds.min.y+hy,bounds.max.y-hy);
            return p;
        }
    }
}
