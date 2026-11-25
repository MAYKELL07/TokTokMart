using UnityEngine;
namespace TokTokMart
{
    [DisallowMultipleComponent]
    public sealed class WaterSpill : MonoBehaviour
    {
        public SpriteRenderer artwork;
        [Range(.1f, .8f)] public float cautionRadius = .46f;
        public Vector2 Position => ShopLayout.ToGame(transform.position);
        public Vector2 SyncedPosition { get; private set; }
        public bool HasManualEdit => synced && (Position - SyncedPosition).sqrMagnitude > .0025f;
        bool synced;
        public void ApplyPosition(Vector2 position)
        { transform.position = ShopLayout.ToWorld(position); SyncedPosition = position; synced = true; }
        void OnDrawGizmosSelected() { Gizmos.color = new Color(.25f, .7f, 1, .7f); Gizmos.DrawWireSphere(transform.position, cautionRadius); }
    }
}
