using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HundredHour.UI.Choices.Demo
{
    /// <summary>移植したメニューを実際に使う英語サンプル。ゲームロジックはモジュールの外に置く。</summary>
    public sealed class ChoiceMenuDemo : MonoBehaviour
    {
        [SerializeField] ChoiceMenuController menu;
        [SerializeField] TextMeshProUGUI status;
        static readonly string[] Labels = { "Sound of fate  --  100%", "Walk into the unknown", "Stay here a little longer" };
        public int ConfirmationCount { get; private set; }
        public int LastConfirmedIndex { get; private set; } = -1;
        public ChoiceMenuController Menu => menu;

        public void Configure(ChoiceMenuController controller, TextMeshProUGUI statusLabel)
        {
            menu = controller;
            status = statusLabel;
        }
        void Start()
        {
            menu.Confirmed += OnConfirmed;
            menu.SelectionChanged += OnSelectionChanged;
            Reopen();
        }
        void OnDestroy()
        {
            if (menu == null) return;
            menu.Confirmed -= OnConfirmed;
            menu.SelectionChanged -= OnSelectionChanged;
        }
        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame) Reopen();
        }
        public void Reopen()
        {
            if (!Application.isPlaying || menu == null) return;
            menu.Show(Labels);
            status.text = "Choose a path.";
        }
        void OnSelectionChanged(int index) => status.text = "Focused: " + Labels[index];
        void OnConfirmed(int index)
        {
            ConfirmationCount++;
            LastConfirmedIndex = index;
            status.text = "Selected: " + Labels[index] + "\nPress R or click REOPEN to try again.";
        }
    }
}
