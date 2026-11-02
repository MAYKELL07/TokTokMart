using UnityEngine;
namespace GokilKisah
{
    [DisallowMultipleComponent]
    public sealed class ShelfInventory : MonoBehaviour
    {
        [Min(1)] public int capacity=5;
        [Min(0)] public int initialStock=0;
        public BoxCollider2D collisionBase;
        [Tooltip("Move or resize this trigger to set where restocking is available.")]
        public BoxCollider2D interactionZone;
        public Transform shoppingPoint;
        public SpriteRenderer[] stockSlots;
        public Sprite stockedSprite, emptySprite;
        public int CurrentStock {get;private set;}
        int displayedStock=-1;
        public bool CanInteract(Vector2 worldPoint)=>interactionZone&&interactionZone.enabled&&interactionZone.OverlapPoint(worldPoint);
        public void ShowStock(int amount)
        {
            CurrentStock=Mathf.Clamp(amount,0,capacity);
            if(stockSlots==null||stockSlots.Length==0){displayedStock=-1;return;}
            if(displayedStock==CurrentStock)return;
            displayedStock=CurrentStock;
            for(int i=0;i<stockSlots.Length;i++)if(stockSlots[i])
                stockSlots[i].sprite=i<CurrentStock?stockedSprite:emptySprite;
        }
        void OnValidate()
        {capacity=Mathf.Max(1,capacity);initialStock=Mathf.Clamp(initialStock,0,capacity);displayedStock=-1;ShowStock(initialStock);}
        void OnDrawGizmosSelected()
        {
            if(!interactionZone)return;
            Gizmos.color=new Color(.3f,1,.65f,.65f);
            var previous=Gizmos.matrix;Gizmos.matrix=interactionZone.transform.localToWorldMatrix;
            Gizmos.DrawWireCube(interactionZone.offset,interactionZone.size);
            Gizmos.matrix=previous;
        }
    }
}


