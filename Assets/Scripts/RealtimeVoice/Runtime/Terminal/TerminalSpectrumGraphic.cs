using UnityEngine;
using UnityEngine.UI;
namespace HundredHour.RealtimeVoice
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class TerminalSpectrumGraphic : MaskableGraphic
    {
        public readonly float[] Bands = new float[64];
        public Color SignalColor = new Color(.18f, .52f, 1);
        public float Formation;
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear(); var r = rectTransform.rect;
            float mid = r.center.y, step = r.width / Bands.Length;
            Quad(mesh, r.xMin, mid - .5f, r.xMax, mid + .5f, new Color(.65f, .7f, .8f, .18f));
            for (int i = 0; i < Bands.Length; i++)
            {
                float h = Bands[i] * r.height * .43f;
                if (h < .5f) continue;
                float x = r.xMin + i * step;
                var glow = SignalColor; glow.a = .075f;
                Quad(mesh, x - 2, mid - h - 6, x + step - 1, mid + h + 6, glow);
                var c = SignalColor; c.a = .85f;
                Quad(mesh, x + 2, mid - h, x + step - 4, mid + h, c);
                c.a = .35f; Quad(mesh, x + 2, mid + h + 3, x + step - 4, mid + h + 4, c);
            }
            // A separate quiet nucleus during formation, never presented as a voice waveform.
            if (Formation > 0)
            {
                var c = SignalColor; c.a = .55f * Formation;
                float x = r.center.x, radius = 3 + Formation * 3;
                Quad(mesh, x - radius, mid - radius, x + radius, mid + radius, c);
            }
        }
        static void Quad(VertexHelper mesh, float x0, float y0, float x1, float y1, Color color)
        {
            int i = mesh.currentVertCount;
            mesh.AddVert(new Vector3(x0, y0), color, Vector2.zero); mesh.AddVert(new Vector3(x0, y1), color, Vector2.zero);
            mesh.AddVert(new Vector3(x1, y1), color, Vector2.zero); mesh.AddVert(new Vector3(x1, y0), color, Vector2.zero);
            mesh.AddTriangle(i, i + 1, i + 2); mesh.AddTriangle(i, i + 2, i + 3);
        }
    }
}
