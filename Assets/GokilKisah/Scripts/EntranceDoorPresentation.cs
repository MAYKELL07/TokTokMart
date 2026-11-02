using UnityEngine;
namespace GokilKisah
{
    // Visual sliding leaves; the existing entrance navigation remains unobstructed.
    public sealed class EntranceDoorPresentation : MonoBehaviour
    {
        public ShopLayout layout;
        public Transform leftLeaf,rightLeaf;
        public float travel=.65f;
        [Min(.1f)] public float tweenDuration=.4f;
        [Min(0)] public float closeDelay=.45f;
        Vector3 leftClosed,rightClosed;
        bool initialized;
        float open,hold;
        public float OpenAmount=>open;
        public bool IsPassable=>open>=.85f;
        void Awake(){Initialize();}
        void Initialize(){if(initialized||!leftLeaf||!rightLeaf)return;leftClosed=leftLeaf.localPosition;rightClosed=rightLeaf.localPosition;initialized=true;}
        public void Tick(bool customerNear,float dt)
        {
            if(!leftLeaf||!rightLeaf)return;
            Initialize();
            if(customerNear)hold=closeDelay;else hold=Mathf.Max(0,hold-dt);
            open=Mathf.MoveTowards(open,customerNear||hold>0?1:0,dt/Mathf.Max(.1f,tweenDuration));
            float eased=open*open*(3-2*open);
            leftLeaf.localPosition=leftClosed+Vector3.left*travel*eased;rightLeaf.localPosition=rightClosed+Vector3.right*travel*eased;
        }
    }
}
