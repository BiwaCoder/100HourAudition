using System;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
namespace HundredHour.RealtimeVoice
{
    [RequireComponent(typeof(TMP_Text))]
    public sealed class TerminalTextReveal : MonoBehaviour
    {
        TMP_Text label;
        string target = "";
        int[] elements = Array.Empty<int>();
        readonly StringBuilder frame = new StringBuilder();
        float cursor, nextFrame;
        int style;
        bool running;
        public string FullText => target;
        public bool IsRevealing => running;
        public void Show(string value, int stage)
        {
            value = value ?? "";
            if (!label) label = GetComponent<TMP_Text>();
            if (value == target) return;
            bool append = target.Length > 0 && value.StartsWith(target, StringComparison.Ordinal);
            target = value; style = stage;
            elements = StringInfo.ParseCombiningCharacters(target);
            if (!append) cursor = 0;
            running = Application.isPlaying && isActiveAndEnabled && elements.Length > 0;
            // Source is never parsed as TMP markup (including transcripts and JSON).
            label.richText = false;
            if (!running) { label.text = target; return; }
            Draw();
        }
        void Update()
        {
            if (!running) return;
            cursor = Mathf.Min(elements.Length, cursor + Time.unscaledDeltaTime * Mathf.Max(style == 0 ? 38 : 60, elements.Length / 1.6f));
            if (cursor >= elements.Length) { label.text = target; running = false; return; }
            if (Time.unscaledTime < nextFrame) return;
            nextFrame = Time.unscaledTime + 1f / 30; Draw();
        }
        void Draw()
        {
            int count = Mathf.Min((int)cursor, elements.Length);
            frame.Clear();
            if (count > 0) frame.Append(target, 0, count == elements.Length ? target.Length : elements[count]);
            if (style == 0) frame.Append('_');
            else
            {
                // A small decoding frontier; completed text stays completely still and readable.
                const string glyphs = "01:/+*=._";
                int frontier = style == 1 ? 3 : 8;
                for (int i = count; i < Mathf.Min(elements.Length, count + frontier); i++)
                {
                    char c = target[elements[i]];
                    if (char.IsWhiteSpace(c)) frame.Append(c);
                    else frame.Append(glyphs[(i * 7 + (int)(Time.unscaledTime * 18)) % glyphs.Length]);
                }
            }
            label.text = frame.ToString();
        }
        void OnDisable() { running = false; if (label) label.text = target; }
    }
}
