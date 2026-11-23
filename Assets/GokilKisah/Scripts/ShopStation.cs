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
        [Range(0, 1.5f), Tooltip("Action reach in world units. Zero uses the shop default.")]
        public float interactionRadius;
        public Vector2 Position=>ShopLayout.ToGame(transform.position);
        public Vector2 InteractionPosition=>approachPoint?ShopLayout.ToGame(approachPoint.position):Position;
    }
}

