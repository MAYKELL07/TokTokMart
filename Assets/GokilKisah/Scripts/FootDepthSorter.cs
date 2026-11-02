using UnityEngine;
namespace GokilKisah
{
    [ExecuteAlways,DisallowMultipleComponent,DefaultExecutionOrder(100)]
    public sealed class FootDepthSorter : MonoBehaviour
    {
        public Transform feet;
        public bool automaticSorting=true;
        public int orderOffset;
        public SpriteRenderer[] artwork;
        public int[] artworkOffsets;
        void LateUpdate()
        {
            if(!automaticSorting)return;
            int depth=Mathf.RoundToInt(-(feet?feet.position.y:transform.position.y)*100)+orderOffset;
            for(int i=0;i<artwork.Length;i++)if(artwork[i])artwork[i].sortingOrder=depth+(artworkOffsets!=null&&i<artworkOffsets.Length?artworkOffsets[i]:0);
        }
    }
}

