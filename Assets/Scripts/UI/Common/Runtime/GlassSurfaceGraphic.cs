using UnityEngine;
namespace HundredHour.UI.Audition
{
    [RequireComponent(typeof(CanvasRenderer)), AddComponentMenu("")]
    public sealed class GlassSurfaceGraphic : UnityEngine.UI.MaskableGraphic
    {
        [SerializeField] Color bottom=new Color(.8f,.95f,1,.4f);
        [SerializeField] Color border=new Color(1,1,1,.8f);
        [SerializeField,Min(0)] float radius=28, borderWidth=1.5f;
        public void SetStyle(Color top,Color lower,Color edge,float round,float width=1.5f)
        {color=top;bottom=lower;border=edge;radius=round;borderWidth=width;SetVerticesDirty();}
        protected override void OnPopulateMesh(UnityEngine.UI.VertexHelper vh)
        {
            vh.Clear(); var rect=GetPixelAdjustedRect(); if(rect.width<=0 || rect.height<=0)return;
            float bw=Mathf.Min(borderWidth,Mathf.Min(rect.width,rect.height)*.5f);
            float r=Mathf.Min(radius,Mathf.Min(rect.width,rect.height)*.5f);
            var inside=new Rect(rect.x+bw,rect.y+bw,rect.width-bw*2,rect.height-bw*2);
            Color At(float y)=>Color.Lerp(bottom,color,Mathf.InverseLerp(rect.yMin,rect.yMax,y));
            vh.AddVert(inside.center,At(inside.center.y),Vector2.zero);
            const int steps=16,count=4*(steps+1);
            Vector2 Point(Rect rc,float rad,int corner,int step)
            {
                float a=(corner*90f+step*90f/steps)*Mathf.Deg2Rad;
                return new Vector2((corner==0||corner==3 ? rc.xMax-rad:rc.xMin+rad)+Mathf.Cos(a)*rad,
                    (corner<2 ? rc.yMax-rad:rc.yMin+rad)+Mathf.Sin(a)*rad);
            }
            for(int c=0;c<4;c++)for(int s=0;s<=steps;s++)
            {var p=Point(inside,Mathf.Max(0,r-bw),c,s);vh.AddVert(p,At(p.y),Vector2.zero);}
            for(int i=0;i<count;i++)vh.AddTriangle(0,i+1,(i+1)%count+1);
            if(bw<=0)return;
            int start=vh.currentVertCount;
            for(int c=0;c<4;c++)for(int s=0;s<=steps;s++)
            {vh.AddVert(Point(rect,r,c,s),border,Vector2.zero);vh.AddVert(Point(inside,Mathf.Max(0,r-bw),c,s),border,Vector2.zero);}
            for(int i=0;i<count;i++)
            {int a=start+i*2,b=start+((i+1)%count)*2;vh.AddTriangle(a,b,a+1);vh.AddTriangle(b,b+1,a+1);}
        }
    }
}
