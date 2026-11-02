using UnityEngine;
namespace GokilKisah
{
    [DisallowMultipleComponent]
    public sealed class ArcadeRoomCamera : MonoBehaviour
    {
        public ShopLayout layout;
        public Bounds shopFrame;
        public SpriteRenderer playerRing;
        [Range(1.5f,4)] public float closeZoom=2.8f;
        Vector3 velocity;
        public void Frame(Camera camera,Vector2 feet,float dt,bool snap)
        {
            if(playerRing)
            {
                float footOffset=layout.playerCharacter.artwork.bounds.min.y-layout.playerCharacter.transform.position.y;
                float horizontalOffset=layout.playerCharacter.artwork.bounds.center.x-layout.playerCharacter.transform.position.x;
                playerRing.transform.position=new Vector3(feet.x+horizontalOffset,feet.y+footOffset+.02f,0);
            }
            camera.orthographic=true;camera.orthographicSize=closeZoom;
            if(layout.testFollowCamera)
            {layout.testFollowCamera.Follow(camera,feet,Vector2.zero,layout.CameraBounds,dt,snap);return;}
            var center=new Vector3(feet.x,feet.y+.35f,-10);
            if(snap){camera.transform.position=center;velocity=Vector3.zero;}
            else camera.transform.position=Vector3.SmoothDamp(camera.transform.position,center,ref velocity,.085f,Mathf.Infinity,Mathf.Min(dt,.05f));
        }
    }
}
