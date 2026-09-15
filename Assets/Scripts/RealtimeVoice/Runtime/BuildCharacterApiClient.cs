using System;
using System.Collections;
using System.Text;
using HundredHour.Net;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace HundredHour.RealtimeVoice
{
    /// <summary>PythonAPIの/api/build-characterへ、音声ヒアリングの回答を送りキャラクターJSONを生成させる。</summary>
    [AddComponentMenu("RealtimeVoice/Build Character Api Client")]
    public sealed class BuildCharacterApiClient : MonoBehaviour
    {
        string baseUrl;
        [SerializeField] int timeoutSeconds = 60;

        void Awake() => baseUrl = ApiEndpointConfig.Instance.Http;

        UnityWebRequest activeRequest;
        public void Cancel()
        {
            activeRequest?.Abort();
            activeRequest = null;
            StopAllCoroutines();
        }
        void OnDisable()=>Cancel();

        public void BuildCharacter(string voiceAnswer, string photoImpression, Action<BuildCharacterResponsePayload> onComplete, string language="ja", string profileMode="legacy")
        {
            Cancel();
            StartCoroutine(PostBuildCharacter(voiceAnswer, photoImpression, onComplete, language, profileMode));
        }

        IEnumerator PostBuildCharacter(string voiceAnswer, string photoImpression, Action<BuildCharacterResponsePayload> onComplete, string language="ja", string profileMode="legacy")
        {
            var payload = new JObject
            {
                ["voice_answer"] = voiceAnswer,
                ["photo_impression"] = photoImpression ?? "",
                ["language"] = language,
                ["profile_mode"] = profileMode,
                ["player_gender"] = HundredHour.AuditionEntry.AuditionProfile.Gender,
            };
            byte[] body = Encoding.UTF8.GetBytes(payload.ToString(Newtonsoft.Json.Formatting.None));

            using var request = new UnityWebRequest($"{baseUrl}/api/build-character", UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = profileMode == "interview" ? Mathf.Max(timeoutSeconds, 130) : timeoutSeconds;
            ApiAuth.ApplyHeaders(request);

            activeRequest = request;
            yield return request.SendWebRequest();
            activeRequest = null;

            onComplete?.Invoke(Parse(request));
        }

        static BuildCharacterResponsePayload Parse(UnityWebRequest request)
        {
            if (request.result != UnityWebRequest.Result.Success)
                return new BuildCharacterResponsePayload { success = false, error = $"{request.error} ({request.responseCode})" };

            try
            {
                var obj = JObject.Parse(request.downloadHandler.text);
                bool success = obj.Value<bool?>("success") == true && obj["character"] is JObject;
                return new BuildCharacterResponsePayload
                {
                    success = success,
                    character_json = success ? obj["character"]?.ToString(Newtonsoft.Json.Formatting.Indented) : null,
                    error = success ? null : "キャラクターを生成できませんでした。もう一度お試しください。",
                };
            }
            catch (Exception e)
            {
                return new BuildCharacterResponsePayload { success = false, error = $"parse error: {e.Message}" };
            }
        }
    }
}
