using UnityEngine;
namespace GokilKisah
{
    [DisallowMultipleComponent]
    public sealed class ShopObstacle : MonoBehaviour
    {
        [Tooltip("Feet collide with these shapes. Resize them in the Scene view.")]
        public Collider2D[] shapes;
        public bool Blocks(Vector2 point,float radius)
        {
            foreach(var shape in shapes)
                if(shape&&shape.enabled&&shape.gameObject.activeInHierarchy&&
                   ((Vector2)shape.ClosestPoint(point)-point).sqrMagnitude<(radius-.00001f)*(radius-.00001f))return true;
            return false;
        }
    }
}

