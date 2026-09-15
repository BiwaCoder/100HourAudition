using System;
using System.Collections;
using System.Text;
using HundredHour.Net;
using UnityEngine;
using UnityEngine.Networking;

namespace HundredHour.AIChat
{
    /// <summary>Python(FastAPI)のOpenAI中継APIへリクエストを送る薄いクライアント。UIには依存しない。</summary>
    [AddComponentMenu("AIChat/Chat Api Client")]
    public sealed class ChatApiClient : MonoBehaviour
    {
        string baseUrl;
        [SerializeField] int timeoutSeconds = 30;
        [SerializeField] bool logRequestsAndResponses = true;

        void Awake() => baseUrl = ApiEndpointConfig.Instance.Http;

        public void SendChatMessage(string message, string systemMessage, Action<ChatResponsePayload> onComplete)
        {
            if (string.IsNullOrEmpty(message)) throw new ArgumentException("message is empty", nameof(message));
            StartCoroutine(PostChat(message, systemMessage, onComplete));
        }

        IEnumerator PostChat(string message, string systemMessage, Action<ChatResponsePayload> onComplete)
        {
            var payload = new ChatRequestPayload { message = message, system_message = systemMessage };
            string requestId = Guid.NewGuid().ToString("N").Substring(0, 8);
            string json = JsonUtility.ToJson(payload);
            byte[] body = Encoding.UTF8.GetBytes(json);
            if (logRequestsAndResponses) Debug.Log($"[AIChat:{requestId}] REQUEST POST {baseUrl}/api/chat\n{json}", this);

            using (var request = new UnityWebRequest($"{baseUrl}/api/chat", UnityWebRequest.kHttpVerbPOST))
            {
                request.uploadHandler = new UploadHandlerRaw(body);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.timeout = timeoutSeconds;
                ApiAuth.ApplyHeaders(request);

                yield return request.SendWebRequest();

                if (logRequestsAndResponses) Debug.Log($"[AIChat:{requestId}] RESPONSE HTTP {request.responseCode} / {request.result}\n{request.downloadHandler?.text}\n{request.error}", this);
                onComplete?.Invoke(Parse(request));
            }
        }

        static ChatResponsePayload Parse(UnityWebRequest request)
        {
            if (request.result != UnityWebRequest.Result.Success)
                return new ChatResponsePayload { success = false, error = $"{request.error} ({request.responseCode})" };

            try
            {
                var parsed = JsonUtility.FromJson<ChatResponsePayload>(request.downloadHandler.text);
                return parsed ?? new ChatResponsePayload { success = false, error = "empty response" };
            }
            catch (Exception e)
            {
                return new ChatResponsePayload { success = false, error = $"parse error: {e.Message}" };
            }
        }
    }
}
