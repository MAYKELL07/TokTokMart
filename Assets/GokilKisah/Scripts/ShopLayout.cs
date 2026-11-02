using UnityEngine;

namespace GokilKisah
{
    [DisallowMultipleComponent]
    public sealed class ShopLayout : MonoBehaviour
    {
        [HideInInspector] public int schemaVersion;
        [Header("Playable scene")]
        public Camera worldCamera;
        [Tooltip("Preserved camera for comparison. Inactive while testing the new follow camera.")]
        public Camera originalWorldCamera;
        public ResponsiveFollowCamera testFollowCamera;
        public BoxCollider2D walkableArea, storageArea;
        public Transform storageDoor;
        public Bounds MapBounds{get{var bounds=walkableArea.bounds;if(storageArea)bounds.Encapsulate(storageArea.bounds);return bounds;}}
        [HideInInspector] public Bounds presentationBounds;
        public Bounds CameraBounds=>presentationBounds.size.x>0?presentationBounds:MapBounds;
        public bool IsFloor(Vector2 point)=>walkableArea&&walkableArea.OverlapPoint(point)||storageArea&&storageArea.OverlapPoint(point);
        public CharacterSpriteAnimator playerCharacter, partnerCharacter;
        public Transform customerDoor, customerEntryTarget, queueFront, partnerCheckoutSpot;
        public Vector2 queueSpacing = new Vector2(0, -.37f);
        public ShopStation stockroom, register, wallPhone, nailGun;
        public Transform[] switchLocations, canLocations, goblinSpawns, idolSpawns;
        public Transform salmahSpawn, rayDestination, boCarTarget;
        public CharacterSpriteAnimator customerTemplate, goblinTemplate, idolTemplate, rayCharacter, salmahCharacter;
        public SpriteRenderer lightSwitch, counterCan;
        public SpriteRenderer[] fluorescentLights;
        public SpillHazard spillTemplate;
        [Header("Scene camera")]
        [Tooltip("Turn off to use your own camera position and zoom. The viewport continues to fit the HUD.")]
        public bool frameCameraToInterface = true;
        [Header("Interaction")]
        [Range(.1f, 1.2f)] public float spillInteractionRadius = .65f;
        [Range(.1f, 1.5f)] public float stationInteractionRadius = .75f;

        public ShelfInventory[] Shelves => GetComponentsInChildren<ShelfInventory>();
        public ShopObstacle[] Obstacles => GetComponentsInChildren<ShopObstacle>();
        public SpillHazard[] StartingSpills
        {
            get
            {
                var all=GetComponentsInChildren<SpillHazard>();
                var result=new System.Collections.Generic.List<SpillHazard>();
                foreach(var spill in all)if(spill!=spillTemplate&&!spill.runtimeSpawned)result.Add(spill);
                return result.ToArray();
            }
        }
        public static Vector2 ToGame(Vector3 p)=>new Vector2(p.x*100,-p.y*100);
        public static Vector3 ToWorld(Vector2 p)=>new Vector3(p.x/100,-p.y/100,0);
        void OnDrawGizmosSelected()
        {
            Gizmos.color=new Color(.3f,1,.65f,.7f);
            foreach(var marker in new[]{customerDoor,customerEntryTarget,queueFront,partnerCheckoutSpot})
                if(marker)Gizmos.DrawWireSphere(marker.position,.12f);
        }
    }
}

