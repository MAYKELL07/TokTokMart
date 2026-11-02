using UnityEngine;
namespace GokilKisah
{
    [DisallowMultipleComponent]
    public sealed class SpillHazard : MonoBehaviour
    {
        [HideInInspector] public bool runtimeSpawned;
        public SpriteRenderer artwork;
        [Range(.1f,.8f)] public float cautionRadius=.46f;
        public Vector2 Position=>ShopLayout.ToGame(transform.position);
        void OnDrawGizmosSelected(){Gizmos.color=new Color(.25f,.7f,1,.7f);Gizmos.DrawWireSphere(transform.position,cautionRadius);}
    }
}

