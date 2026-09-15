using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HundredHour.AIChat;
using UnityEngine;
using UnityEngine.Networking;
namespace HundredHour.Cutscenes
{
    public interface ICameraPlanSource { Task<CameraPlan> Generate(string instruction,ICollection<string> actors,CancellationToken cancellation); }
    public sealed class PythonCameraPlanSource : ICameraPlanSource
    {
        readonly string baseUrl;
        public PythonCameraPlanSource(string url="http://127.0.0.1:8001"){baseUrl=url.TrimEnd('/');}
        // Refactors CinePlanPromptBuilder's schema-first contract onto the existing PythonAPI.
        public static string SystemPrompt(ICollection<string> actors)=>
            "あなたはゲームの撮影監督。自然言語の指示を次のJSONへ変換。JSONのみを出力。人物IDは "+string.Join(",",actors)+" のみ。コード・ファイルパスは出力しない。"+
            "形式 {\"title\":\"演出名\",\"shots\":[{\"label\":\"名前\",\"kind\":1,\"actorA\":\"A\",\"actorB\":\"B\",\"duration\":4,\"transition\":2,\"transitionSeconds\":0.6,\"distance\":5,\"yaw\":0,\"elevation\":1.2,\"startFov\":40,\"endFov\":40,\"orbitDegrees\":60}]}。各shotは全フィールド必須。"+
            "kind数値:0単独,1横に二人,2 Aの肩越しにB,3 Bの肩越しにA,4全員を背後から引き,5全員を正面から引き,6回り込み,7ドリーイン,8ドリーアウト。"+
            "transition数値:0カット,1滑らかに移動,2暗転フェード。durationは動作秒数0.2〜60、transitionSecondsは各フェード片道0〜3秒。"+
            "ゆっくりズームインはカメラ位置を固定しstartFov40→endFov25、ズームアウトは25→50。ズームとドリーは別。"+
            "distance1〜30m,yaw-180〜180,elevation0.1〜10m,FOV15〜90,orbitDegrees-180〜180。最大64ショット、全体10分以内。";
        public async Task<CameraPlan> Generate(string instruction,ICollection<string> actors,CancellationToken cancellation)
        {
            if(string.IsNullOrWhiteSpace(instruction))throw new ArgumentException("演出指示を入力してください。");
            var payload=new ChatRequestPayload{message=instruction,system_message=SystemPrompt(actors)};
            using(var request=new UnityWebRequest(baseUrl+"/api/chat","POST"))
            {
                request.uploadHandler=new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(payload)));request.downloadHandler=new DownloadHandlerBuffer();request.SetRequestHeader("Content-Type","application/json");request.timeout=60;
                var operation=request.SendWebRequest();while(!operation.isDone){if(cancellation.IsCancellationRequested){request.Abort();cancellation.ThrowIfCancellationRequested();}await Task.Yield();}
                cancellation.ThrowIfCancellationRequested();if(request.result!=UnityWebRequest.Result.Success)throw new InvalidOperationException("PythonAPI: "+request.error);
                var response=JsonUtility.FromJson<ChatResponsePayload>(request.downloadHandler.text);if(response==null||!response.success)throw new InvalidOperationException(response?.error??"空の応答");
                return Parse(response.response,actors);
            }
        }
        public static CameraPlan Parse(string json,ICollection<string> actors)
        {
            if(string.IsNullOrWhiteSpace(json)||json.Length>100000)throw new ArgumentException("応答JSONが空、または大きすぎます。");
            json=json.Trim();if(json.StartsWith("```")){int first=json.IndexOf('\n'),last=json.LastIndexOf("```");if(first<0||last<=first)throw new ArgumentException("JSON形式を確認してください。");json=json.Substring(first+1,last-first-1).Trim();}
            if(!json.StartsWith("{")||!json.EndsWith("}"))throw new ArgumentException("JSON形式を確認してください。");
            var plan=JsonUtility.FromJson<CameraPlan>(json);CameraPlanValidation.Validate(plan?.shots,actors);return plan;
        }
    }
}
