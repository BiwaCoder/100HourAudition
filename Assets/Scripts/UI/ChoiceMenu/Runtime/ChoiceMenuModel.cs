using System;
using System.Collections.Generic;

namespace HundredHour.UI.Choices
{
    /// <summary>Unity非依存の選択状態。決定は一回のみ。空リストは選択不可。</summary>
    public sealed class ChoiceMenuModel
    {
        public IReadOnlyList<string> Labels { get; private set; } = Array.AsReadOnly(Array.Empty<string>());
        public int SelectedIndex { get; private set; } = -1;
        public bool IsConfirmed { get; private set; }
        public event Action Changed;

        public void SetChoices(IReadOnlyList<string> labels, int initialIndex = 0)
        {
            if (labels == null) throw new ArgumentNullException(nameof(labels));
            var copy = new string[labels.Count];
            for (int i = 0; i < copy.Length; i++) copy[i] = labels[i] ?? string.Empty;
            Labels = Array.AsReadOnly(copy);
            SelectedIndex = copy.Length == 0 ? -1 : Math.Max(0, Math.Min(initialIndex, copy.Length - 1));
            IsConfirmed = false;
            Changed?.Invoke();
        }

        public bool Select(int index)
        {
            if (IsConfirmed || index < 0 || index >= Labels.Count || index == SelectedIndex) return false;
            SelectedIndex = index;
            Changed?.Invoke();
            return true;
        }

        public bool Move(int delta)
        {
            if (Labels.Count == 0 || IsConfirmed) return false;
            return Select((int)(((long)SelectedIndex + delta % Labels.Count + Labels.Count) % Labels.Count));
        }

        public bool TryConfirm(out int index)
        {
            index = SelectedIndex;
            if (index < 0 || IsConfirmed) return false;
            IsConfirmed = true;
            Changed?.Invoke();
            return true;
        }
    }
}
