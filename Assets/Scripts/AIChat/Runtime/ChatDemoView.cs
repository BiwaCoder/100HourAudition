using System;
using UnityEngine;
using UnityEngine.UI;

namespace HundredHour.AIChat
{
    /// <summary>入力欄・送信ボタン・結果表示のUnityEngine.UIラッパー。ロジックは持たない。</summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("AIChat/Chat Demo View")]
    public sealed class ChatDemoView : MonoBehaviour
    {
        [SerializeField] InputField messageInput;
        [SerializeField] Button sendButton;
        [SerializeField] Text resultText;

        public event Action<string> Submitted;

        void OnEnable() => sendButton.onClick.AddListener(HandleClick);
        void OnDisable() => sendButton.onClick.RemoveListener(HandleClick);

        void HandleClick()
        {
            string message = messageInput.text;
            if (string.IsNullOrWhiteSpace(message)) return;
            Submitted?.Invoke(message);
        }

        public void SetBusy(bool busy)
        {
            sendButton.interactable = !busy;
            if (busy) resultText.text = "送信中...";
        }

        public void ShowResult(string text) => resultText.text = text;
        public void ShowError(string message) => resultText.text = $"エラー: {message}";
    }
}
