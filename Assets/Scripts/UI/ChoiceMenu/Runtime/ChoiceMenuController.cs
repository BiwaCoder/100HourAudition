using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HundredHour.UI.Choices
{
    /// <summary>ModelとViewの接続。無効化・削除時に入力と生成表示を解除する。</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(ChoiceMenuView))]
    [AddComponentMenu("UI/Choices/Choice Menu Controller")]
    public sealed class ChoiceMenuController : MonoBehaviour
    {
        [SerializeField] UnityEvent<int> onConfirmed = new UnityEvent<int>();
        public event Action<int> Confirmed;
        public event Action<int> SelectionChanged;
        readonly ChoiceMenuModel model = new ChoiceMenuModel();
        ChoiceMenuView view;
        Coroutine confirmation;
        bool open;
        int openedFrame;
        public int SelectedIndex => model.SelectedIndex;
        public bool IsOpen => open;
        public bool CanInteract => isActiveAndEnabled && open && !model.IsConfirmed && Time.frameCount > openedFrame;
        public UnityEvent<int> OnConfirmed => onConfirmed;

        void OnEnable()
        {
            view = GetComponent<ChoiceMenuView>();
            model.Changed += Refresh;
            view.Hovered += Select;
            view.Clicked += Click;
        }
        void OnDisable()
        {
            Close();
            model.Changed -= Refresh;
            if (view == null) return;
            view.Hovered -= Select;
            view.Clicked -= Click;
        }

        public void Show(IReadOnlyList<string> labels, int initialIndex = 0)
        {
            if (!isActiveAndEnabled) throw new InvalidOperationException("Enable ChoiceMenuController before Show().");
            if (labels == null) throw new ArgumentNullException(nameof(labels));
            Close();
            model.SetChoices(labels, initialIndex);
            if (labels.Count == 0) return;
            view.Build(model.Labels);
            open = true;
            openedFrame = Time.frameCount;
            Refresh();
        }

        public void Close()
        {
            open = false;
            if (confirmation != null) StopCoroutine(confirmation);
            confirmation = null;
            if (view != null) view.Clear();
        }

        public void Select(int index)
        {
            if (CanInteract && model.Select(index)) SelectionChanged?.Invoke(model.SelectedIndex);
        }
        public void Move(int delta)
        {
            if (CanInteract && model.Move(delta)) SelectionChanged?.Invoke(model.SelectedIndex);
        }
        public void Confirm()
        {
            if (CanInteract && model.TryConfirm(out int index)) confirmation = StartCoroutine(Finish(index));
        }
        public void SelectAndConfirm(int index)
        {
            if (!CanInteract || index < 0 || index >= model.Labels.Count) return;
            Select(index);
            // SelectionChanged側でShow/Closeされていた場合、同フレームの決定は抑止される。
            Confirm();
        }
        void Click(int index) => SelectAndConfirm(index);
        void Refresh() { if (open && view != null) view.Render(model.Labels, model.SelectedIndex); }

        IEnumerator Finish(int result)
        {
            for (int i = 0; i < 2; i++)
            {
                view.SetSelectedFrameVisible(result, false);
                yield return new WaitForSeconds(.05f);
                view.SetSelectedFrameVisible(result, true);
                yield return new WaitForSeconds(.05f);
            }
            confirmation = null;
            open = false;
            view.Clear();
            onConfirmed.Invoke(result);
            Confirmed?.Invoke(result);
        }
    }
}
