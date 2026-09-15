using UnityEngine;

namespace HundredHour.UI.Choices
{
    /// <summary>
    /// ShiroのChoiceSelectorを独立化。任意のRectTransformに追加できる枠のみのView。
    /// ホストや文字のTransformは変更せず、自分で生成した子だけを操作する。
    /// </summary>
    [DisallowMultipleComponent, RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("UI/Choices/Choice Selection Frame")]
    public sealed class ChoiceSelectionFrame : MonoBehaviour
    {
        [SerializeField] Color tint = Color.white;
        [SerializeField] Vector2 padding = new Vector2(4, 3);
        [SerializeField, Range(0, 0.1f)] float pulseAmount = 0.02f;
        [SerializeField, Min(0)] float pulseSpeed = 5.5f;
        [SerializeField, Min(0)] float fadeSpeed = 4f;
        [SerializeField] bool enableGlitch = true;
        [SerializeField] bool selected = true;
        RectTransform frame;
        readonly UnityEngine.UI.Image[] pieces = new UnityEngine.UI.Image[12];
        float glitchUntil;
        Vector2 glitchOffset;
        // ローカル乱数にしてゲーム本体のUnityEngine.Randomの系列に干渉しない。
        readonly System.Random random = new System.Random();

        public void SetSelected(bool value)
        {
            selected = value;
            if (frame != null) frame.gameObject.SetActive(value && isActiveAndEnabled);
        }

        void OnEnable()
        {
            if (frame == null) Build();
            glitchUntil = 0;
            SetSelected(selected);
        }

        void OnDisable() { if (frame != null) frame.gameObject.SetActive(false); }
        void OnDestroy()
        {
            if (frame == null) return;
            frame.gameObject.SetActive(false);
            if (Application.isPlaying) Destroy(frame.gameObject);
            else DestroyImmediate(frame.gameObject);
        }

        void Update()
        {
            if (!selected || frame == null) return;
            float now = Time.unscaledTime;
            float pulse = 1f + pulseAmount * Mathf.Sin(now * pulseSpeed);
            frame.localScale = new Vector3(pulse, pulse, 1);
            float alpha = 0.5f + 0.5f * (0.5f + 0.5f * Mathf.Sin(now * fadeSpeed));
            // 元の60fps時の2%/frameを時間ベースに換算。
            if (enableGlitch && random.NextDouble() < 1f - Mathf.Pow(0.98f, Time.unscaledDeltaTime * 60f))
            {
                glitchUntil = now + 0.05f;
                glitchOffset = new Vector2((float)random.NextDouble() * 6 - 3, (float)random.NextDouble() * 4 - 2);
            }
            bool glitch = enableGlitch && now < glitchUntil;
            frame.anchoredPosition = glitch ? glitchOffset : Vector2.zero;
            frame.sizeDelta = padding * 2;
            for (int i = 0; i < pieces.Length; i++)
            {
                var color = tint;
                color.a *= alpha * (glitch ? 0.35f : 1f) * (i < 4 ? 0.45f : 1f);
                pieces[i].color = color;
            }
        }

        void Build()
        {
            var go = new GameObject("SelectionFrame", typeof(RectTransform), typeof(UnityEngine.UI.LayoutElement));
            frame = (RectTransform)go.transform;
            frame.SetParent(transform, false);
            frame.SetAsFirstSibling();
            frame.anchorMin = Vector2.zero;
            frame.anchorMax = Vector2.one;
            frame.sizeDelta = padding * 2;
            go.GetComponent<UnityEngine.UI.LayoutElement>().ignoreLayout = true;
            Piece(0, "L_T", new Vector2(0, 1), Vector2.one, new Vector2(.5f, 1), new Vector2(0, 1.5f));
            Piece(1, "L_B", Vector2.zero, new Vector2(1, 0), new Vector2(.5f, 0), new Vector2(0, 1.5f));
            Piece(2, "L_L", Vector2.zero, new Vector2(0, 1), new Vector2(0, .5f), new Vector2(1.5f, 0));
            Piece(3, "L_R", new Vector2(1, 0), Vector2.one, new Vector2(1, .5f), new Vector2(1.5f, 0));
            for (int i = 0; i < 4; i++)
            {
                var corner = new Vector2(i % 2, i < 2 ? 1 : 0);
                Piece(4 + i * 2, "C_" + i + "_h", corner, corner, corner, new Vector2(18, 3));
                Piece(5 + i * 2, "C_" + i + "_v", corner, corner, corner, new Vector2(3, 14));
            }
        }

        void Piece(int index, string name, Vector2 min, Vector2 max, Vector2 pivot, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image));
            go.transform.SetParent(frame, false);
            var img = go.GetComponent<UnityEngine.UI.Image>();
            img.raycastTarget = false;
            var color = tint;
            color.a *= index < 4 ? 0.3375f : 0.75f;
            img.color = color;
            var rt = img.rectTransform;
            rt.anchorMin = min; rt.anchorMax = max; rt.pivot = pivot;
            rt.sizeDelta = size; rt.anchoredPosition = Vector2.zero;
            pieces[index] = img;
        }
    }
}
