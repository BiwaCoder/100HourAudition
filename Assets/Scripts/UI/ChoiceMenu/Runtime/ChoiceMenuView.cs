using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HundredHour.UI.Choices
{
    /// <summary>表示とポインター通知のみ。ゲーム進行・音・入力デバイスには依存しない。</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(RectTransform))]
    public sealed class ChoiceMenuView : MonoBehaviour
    {
        [SerializeField] TMP_FontAsset font;
        [SerializeField, Min(1)] float fontSize = 30;
        [SerializeField, Min(1)] float rowHeight = 46;
        [SerializeField, Min(0)] float spacing = 4;
        readonly List<ChoiceRowView> rows = new List<ChoiceRowView>();
        RectTransform content;
        public event Action<int> Hovered;
        public event Action<int> Clicked;
        public TMP_FontAsset Font { get => font; set => font = value; }

        public void Build(IReadOnlyList<string> labels)
        {
            if (labels == null) throw new ArgumentNullException(nameof(labels));
            if (font == null)
                throw new InvalidOperationException("Assign a TMP_FontAsset to ChoiceMenuView.Font before Show().");
            Clear();
            var go = new GameObject("ChoiceContent", typeof(RectTransform));
            content = (RectTransform)go.transform;
            content.SetParent(transform, false);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = Vector2.one;
            content.pivot = new Vector2(.5f, 1);
            content.sizeDelta = new Vector2(0, labels.Count * rowHeight + Mathf.Max(0, labels.Count - 1) * spacing);
            content.anchoredPosition = Vector2.zero;
            for (int i = 0; i < labels.Count; i++)
            {
                var rowGo = new GameObject("ChoiceRow" + i, typeof(RectTransform), typeof(UnityEngine.UI.Image));
                var rt = (RectTransform)rowGo.transform;
                rt.SetParent(content, false);
                rt.anchorMin = new Vector2(0, 1); rt.anchorMax = Vector2.one;
                rt.pivot = new Vector2(.5f, 1);
                rt.sizeDelta = new Vector2(0, rowHeight);
                rt.anchoredPosition = new Vector2(0, -i * (rowHeight + spacing));
                var bg = rowGo.GetComponent<UnityEngine.UI.Image>();
                bg.color = Color.clear; bg.raycastTarget = true;
                var textGo = new GameObject("Text", typeof(RectTransform));
                textGo.transform.SetParent(rt, false);
                var text = textGo.AddComponent<TextMeshProUGUI>();
                if (font != null) text.font = font;
                text.fontSize = fontSize;
                text.alignment = TextAlignmentOptions.MidlineLeft;
                text.textWrappingMode = TextWrappingModes.NoWrap;
                text.raycastTarget = false;
                text.richText = false;
                text.rectTransform.anchorMin = Vector2.zero;
                text.rectTransform.anchorMax = Vector2.one;
                text.rectTransform.offsetMin = new Vector2(24, 0);
                text.rectTransform.offsetMax = new Vector2(-24, 0);
                var row = rowGo.AddComponent<ChoiceRowView>();
                row.Initialize(i, text, index => Hovered?.Invoke(index), index => Clicked?.Invoke(index));
                rows.Add(row);
            }
        }

        public void Render(IReadOnlyList<string> labels, int selectedIndex)
        {
            for (int i = 0; i < rows.Count && i < labels.Count; i++) rows[i].Render(labels[i], i == selectedIndex);
        }

        public void SetSelectedFrameVisible(int index, bool visible)
        {
            if (index >= 0 && index < rows.Count) rows[index].SetFrameVisible(visible);
        }

        public void Clear()
        {
            rows.Clear();
            if (content == null) return;
            content.gameObject.SetActive(false);
            if (Application.isPlaying) Destroy(content.gameObject);
            else DestroyImmediate(content.gameObject);
            content = null;
        }
        void OnDestroy() => Clear();
    }
}
