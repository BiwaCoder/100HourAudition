using UnityEngine;

namespace HundredHour.UI.Participants
{
    /// <summary>画像素材不要の角丸面。Mask/CanvasGroup/RectMask2Dに対応。</summary>
    [AddComponentMenu(""), RequireComponent(typeof(CanvasRenderer))]
    public sealed class RoundedCardGraphic : UnityEngine.UI.MaskableGraphic
    {
        [SerializeField, Min(0)] float radius = 24;
        [SerializeField] Color bottomColor = Color.white;
        [SerializeField] bool gradient;
        public void SetStyle(Color top, Color bottom, float cornerRadius)
        {
            color = top; bottomColor = bottom; radius = cornerRadius; gradient = top != bottom; SetVerticesDirty();
        }
        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper vh)
        {
            vh.Clear();
            var rect = GetPixelAdjustedRect();
            if (rect.width <= 0 || rect.height <= 0) return;
            float r = Mathf.Min(radius, Mathf.Min(rect.width, rect.height) * .5f);
            Color At(float y) => gradient ? Color.Lerp(bottomColor, color, Mathf.InverseLerp(rect.yMin,rect.yMax,y)) : color;
            vh.AddVert(rect.center, At(rect.center.y), Vector2.zero);
            const int steps = 12;
            for (int corner=0; corner<4; corner++)
            {
                float cx = corner==0 || corner==3 ? rect.xMax-r : rect.xMin+r;
                float cy = corner<2 ? rect.yMax-r : rect.yMin+r;
                for (int i=0; i<=steps; i++)
                {
                    float angle=(corner*90f+i*90f/steps)*Mathf.Deg2Rad;
                    var p=new Vector2(cx+Mathf.Cos(angle)*r,cy+Mathf.Sin(angle)*r);
                    vh.AddVert(p,At(p.y),Vector2.zero);
                }
            }
            int count=4*(steps+1);
            for(int i=0;i<count;i++) vh.AddTriangle(0,i+1,(i+1)%count+1);
        }
    }
}
