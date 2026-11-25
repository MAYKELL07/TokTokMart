using UnityEngine;

namespace TokTokMart
{
    [DefaultExecutionOrder(120)]
    public sealed class CharacterShadow : MonoBehaviour
    {
        [SerializeField] SpriteRenderer artwork;
        [SerializeField] SpriteRenderer shadow;

        Vector3 offset;
        Vector3 size;

        void Awake()
        {
            offset = shadow.transform.localPosition;
            size = shadow.transform.localScale;
        }

        void LateUpdate()
        {
            shadow.enabled = artwork.enabled;
            var bounds = artwork.bounds;
            float width = Mathf.Clamp(bounds.size.x * .9f, .3f, .65f);
            shadow.transform.position = new Vector3(artwork.transform.position.x, bounds.min.y, artwork.transform.position.z) + offset;
            shadow.transform.localScale = new Vector3(size.x * width, size.y, size.z);
        }
    }
}
