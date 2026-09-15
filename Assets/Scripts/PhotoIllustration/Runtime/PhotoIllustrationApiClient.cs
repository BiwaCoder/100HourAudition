using System;
using System.Collections;
using System.Collections.Generic;
using HundredHour.Net;
using UnityEngine;
using UnityEngine.Networking;

namespace HundredHour.PhotoIllustration
{
    /// <summary>Python(FastAPI)のOpenAI中継APIへ写真を送り、AIイラストに変換させる薄いクライアント。UIには依存しない。</summary>
    [AddComponentMenu("PhotoIllustration/Photo Illustration Api Client")]
    public sealed class PhotoIllustrationApiClient : MonoBehaviour
    {
        string baseUrl;
        [SerializeField] int timeoutSeconds = 60;
        [SerializeField] bool logRequestsAndResponses = true;

        public void Cancel()=>StopAllCoroutines();

        void Awake() => baseUrl = ApiEndpointConfig.Instance.Http;

        public void SendPhoto(byte[] jpegBytes, Action<PhotoIllustrationResponsePayload> onComplete)
        {
            if (jpegBytes == null || jpegBytes.Length == 0) throw new ArgumentException("jpegBytes is empty", nameof(jpegBytes));
            StartCoroutine(PostPhoto(jpegBytes, onComplete));
        }

        IEnumerator PostPhoto(byte[] jpegBytes, Action<PhotoIllustrationResponsePayload> onComplete)
        {
            string requestId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var form = new List<IMultipartFormSection>
            {
                new MultipartFormFileSection("file", jpegBytes, "photo.jpg", "image/jpeg"),
                new MultipartFormDataSection("player_gender",HundredHour.AuditionEntry.AuditionProfile.Gender),
            };

            if (logRequestsAndResponses) Debug.Log($"[PhotoIllustration:{requestId}] REQUEST POST {baseUrl}/api/photo-illustration ({jpegBytes.Length} bytes)", this);

            using (var request = UnityWebRequest.Post($"{baseUrl}/api/photo-illustration", form))
            {
                request.timeout = timeoutSeconds;
                ApiAuth.ApplyHeaders(request);
                yield return request.SendWebRequest();

                if (logRequestsAndResponses) Debug.Log($"[PhotoIllustration:{requestId}] RESPONSE HTTP {request.responseCode} / {request.result} / {request.error}", this);
                onComplete?.Invoke(Parse(request));
            }
        }

        static PhotoIllustrationResponsePayload Parse(UnityWebRequest request)
        {
            if (request.result != UnityWebRequest.Result.Success)
                return new PhotoIllustrationResponsePayload { success = false, error = $"{request.error} ({request.responseCode})" };

            try
            {
                var parsed = JsonUtility.FromJson<PhotoIllustrationResponsePayload>(request.downloadHandler.text);
                return parsed ?? new PhotoIllustrationResponsePayload { success = false, error = "empty response" };
            }
            catch (Exception e)
            {
                return new PhotoIllustrationResponsePayload { success = false, error = $"parse error: {e.Message}" };
            }
        }
    }
}
