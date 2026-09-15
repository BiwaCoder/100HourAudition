using System;
using TMPro;
using UnityEngine;

namespace HundredHour.UI.Choices
{
    /// <summary>既存Canvasの下に作る。Canvas/EventSystem/フォントの所有権は呼び出し側。</summary>
    public static class ChoiceMenuFactory
    {
        public static ChoiceMenuController Create(RectTransform parent, TMP_FontAsset font, bool keyboardInput = true)
        {
            if (parent == null) throw new ArgumentNullException(nameof(parent));
            if (font == null) throw new ArgumentNullException(nameof(font));
            if (parent.GetComponentInParent<Canvas>() == null)
                throw new ArgumentException("Parent must be inside a Canvas.", nameof(parent));
            var go = new GameObject("ChoiceMenu", typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var view = go.AddComponent<ChoiceMenuView>();
            view.Font = font;
            var controller = go.AddComponent<ChoiceMenuController>();
            if (keyboardInput) go.AddComponent<ChoiceMenuKeyboardInput>();
            return controller;
        }
    }
}
