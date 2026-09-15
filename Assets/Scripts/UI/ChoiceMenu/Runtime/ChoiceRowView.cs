using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HundredHour.UI.Choices
{
    public sealed class ChoiceRowView : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        int index;
        TextMeshProUGUI label;
        ChoiceSelectionFrame frame;
        Action<int> hover;
        Action<int> click;
        string selectedMarker;

        internal void Initialize(int rowIndex, TextMeshProUGUI text, Action<int> onHover, Action<int> onClick)
        {
            index = rowIndex; label = text; hover = onHover; click = onClick;
            selectedMarker = text.font != null && text.font.HasCharacter('▶') ? "▶" : ">";
            frame = gameObject.AddComponent<ChoiceSelectionFrame>();
        }

        internal void Render(string text, bool selected)
        {
            label.text = selected ? $" {selectedMarker} {index + 1}. {text}" : $"   {index + 1}. {text}";
            label.color = selected ? Color.white : new Color(127f / 255f, 127f / 255f, 127f / 255f);
            label.fontStyle = selected ? FontStyles.Bold : FontStyles.Normal;
            frame.SetSelected(selected);
        }

        internal void SetFrameVisible(bool visible) => frame.SetSelected(visible);
        public void OnPointerEnter(PointerEventData e) => hover?.Invoke(index);
        public void OnPointerClick(PointerEventData e)
        {
            if (e.button == PointerEventData.InputButton.Left) click?.Invoke(index);
        }
    }
}
