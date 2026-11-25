using UnityEngine;
using UnityEngine.UI;

namespace TokTokMart
{
    [AddComponentMenu("UI/Effects/TokMart Horizontal Fade")]
    public sealed class HorizontalMenuFade : BaseMeshEffect
    {
        [Range(0, 1)] public float leftOpacity = 1;
        [Range(0, 1)] public float rightOpacity = 0;

        public override void ModifyMesh(VertexHelper vertices)
        {
            if (!IsActive() || vertices.currentVertCount == 0) return;
            var rect = ((RectTransform)transform).rect;
            if (rect.width <= 0) return;
            var vertex = new UIVertex();
            for (int i = 0; i < vertices.currentVertCount; i++)
            {
                vertices.PopulateUIVertex(ref vertex, i);
                float t = Mathf.InverseLerp(rect.xMin, rect.xMax, vertex.position.x);
                var color = vertex.color;
                color.a = (byte)Mathf.RoundToInt(color.a * Mathf.Lerp(leftOpacity, rightOpacity, t));
                vertex.color = color;
                vertices.SetUIVertex(vertex, i);
            }
        }
    }
}
