using UnityEngine;

namespace HundredHour.AIChat
{
    /// <summary>ChatDemoViewの入力をChatApiClientへ渡し、結果をViewへ戻す。</summary>
    [RequireComponent(typeof(ChatDemoView))]
    [AddComponentMenu("AIChat/Chat Demo Controller")]
    public sealed class ChatDemoController : MonoBehaviour
    {
        [SerializeField] ChatApiClient apiClient;
        [SerializeField] string systemMessage = "あなたは親切なアシスタントです。";
        ChatDemoView view;

        void OnEnable()
        {
            view = GetComponent<ChatDemoView>();
            view.Submitted += HandleSubmitted;
        }

        void OnDisable()
        {
            if (view != null) view.Submitted -= HandleSubmitted;
        }

        void HandleSubmitted(string message)
        {
            view.SetBusy(true);
            apiClient.SendChatMessage(message, systemMessage, HandleResponse);
        }

        void HandleResponse(ChatResponsePayload response)
        {
            view.SetBusy(false);
            if (response.success) view.ShowResult(response.response);
            else view.ShowError(response.error);
        }
    }
}
