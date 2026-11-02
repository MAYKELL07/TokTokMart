using UnityEngine;
namespace GokilKisah
{
    [DisallowMultipleComponent]
    public sealed class ShopStation : MonoBehaviour
    {
        public enum StationType { Stockroom, Checkout, Phone, NailGun }
        public StationType stationType;
        [Tooltip("Optional marker for approaching this station without entering its collision.")]
        public Transform approachPoint;
        public Vector2 Position=>ShopLayout.ToGame(transform.position);
    }
}

