// UI and transport fixtures; one real /build-character call with explicit sample answers. No microphone audio is sent.
var c=UnityEngine.Object.FindFirstObjectByType<HundredHour.RealtimeVoice.RealtimeVoiceDemoController>();
var v=c.GetComponent<HundredHour.RealtimeVoice.RealtimeVoiceDemoView>();
var s=UnityEngine.Object.FindFirstObjectByType<HundredHour.RealtimeVoice.RealtimeVoiceSession>();
System.Collections.IEnumerator Verify(){
 var log=new System.Collections.Generic.List<string>();
 void Check(bool valid,string label){if(!valid)throw new Exception("Voice UX: "+label);log.Add("PASS: "+label);}
 c.Cancel();
 Check(v.startButton.gameObject.activeSelf&&!v.interviewerPanel.activeSelf&&!v.userPanel.activeSelf&&!v.microphonePanel.activeSelf&&!v.resultPanel.activeSelf,"idle hides all unnecessary UI");
 var f=c.Flow;f.Begin();f.Connected();v.Present(f);
 Check(v.interviewerPanel.activeSelf&&!v.userPanel.activeSelf&&!v.microphonePanel.activeSelf,"assistant prompt alone is visible");
 yield return null;ScreenCapture.CaptureScreenshot("Docs/RealtimeVoiceUx/03-mic-prompt.png");yield return null;
 f.PromptFinished();v.Present(f);v.SetMicLevel(.6f);
 Check(v.userPanel.activeSelf&&v.microphonePanel.activeSelf&&!v.confirmButton.gameObject.activeSelf,"listening shows microphone and transcript only");
 var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
 var listen=typeof(HundredHour.RealtimeVoice.RealtimeVoiceSession).GetField("listening",flags);
 var parser=typeof(HundredHour.RealtimeVoice.RealtimeVoiceSession).GetMethod("HandleServerEvent",flags);
 listen.SetValue(s,true);
 parser.Invoke(s,new object[]{"{\"type\":\"input_audio_buffer.speech_started\",\"item_id\":\"mic-test\"}"});
 parser.Invoke(s,new object[]{"{\"type\":\"conversation.item.input_audio_transcription.completed\",\"item_id\":\"mic-test\",\"transcript\":\"こんにちは。マイクの確認です。\"}"});
 Check(f.State==HundredHour.RealtimeVoice.VoiceInterviewState.MicReview&&v.userText.text.Contains("こんにちは"),"transcription event appears on the user panel");
 Check(v.confirmButton.gameObject.activeSelf&&v.retryButton.gameObject.activeSelf&&!v.microphonePanel.activeSelf,"review opens confirm/retry and closes mic");
 parser.Invoke(s,new object[]{"{\"type\":\"conversation.item.input_audio_transcription.completed\",\"item_id\":\"mic-test\",\"transcript\":\"重複した発話\"}"});
 Check(!f.Candidate.Contains("重複"),"duplicate or late transcription does not replace candidate");
 yield return null;ScreenCapture.CaptureScreenshot("Docs/RealtimeVoiceUx/04-mic-review.png");yield return null;
 v.retryButton.onClick.Invoke();Check(f.Listening&&f.Candidate=="","retry button is wired");
 f.ReceiveTranscript("マイクOK");f.Confirm();f.PromptFinished();v.Present(f);
 yield return null;ScreenCapture.CaptureScreenshot("Docs/RealtimeVoiceUx/05-question1.png");yield return null;
 f.ReceiveTranscript("絵本と小さな花を育てることが好きです。");f.Confirm();f.PromptFinished();v.Present(f);
 f.ReceiveTranscript("穏やかで慎重な性格です。人への誠実さを大切にしています。");v.Present(f);
 Check(v.stepText.text.Contains("2 / 2")&&v.confirmLabel.text.Contains("二つの回答"),"second answer has a clear final action");
 yield return null;ScreenCapture.CaptureScreenshot("Docs/RealtimeVoiceUx/06-question2-review.png");yield return null;
 v.confirmButton.onClick.Invoke();
 Check(f.State==HundredHour.RealtimeVoice.VoiceInterviewState.Generating&&!v.microphonePanel.activeSelf&&!v.resultPanel.activeSelf,"final confirm starts JSON generation and hides input");
 yield return null;ScreenCapture.CaptureScreenshot("Docs/RealtimeVoiceUx/07-generating.png");
 float until=Time.realtimeSinceStartup+65;
 while(f.State==HundredHour.RealtimeVoice.VoiceInterviewState.Generating&&Time.realtimeSinceStartup<until)yield return null;
 Check(f.State==HundredHour.RealtimeVoice.VoiceInterviewState.Complete,"live build-character request completes");
 Check(v.resultPanel.activeSelf&&v.copyButton.gameObject.activeSelf&&v.startButton.gameObject.activeSelf&&!v.userPanel.activeSelf,"result/copy/restart visible only on completion");
 var json=Newtonsoft.Json.Linq.JObject.Parse(f.Result);Check(json["name"]!=null,"generated result is a JSON object");
 System.IO.File.WriteAllText("Docs/RealtimeVoiceUx/SampleCharacter.json",f.Result);
 yield return new WaitForSecondsRealtime(.3f);ScreenCapture.CaptureScreenshot("Docs/RealtimeVoiceUx/08-complete.png");yield return null;
 var scroll=v.resultPanel.GetComponentInChildren<UnityEngine.UI.ScrollRect>();
 Check(scroll.content.rect.height>scroll.viewport.rect.height,"long JSON scrolls instead of overflowing");
 c.Cancel();Check(f.State==HundredHour.RealtimeVoice.VoiceInterviewState.Ready&&!s.IsListening,"cancel/reset closes input and clears run");
 System.IO.File.WriteAllLines("Docs/RealtimeVoiceUx/PlayChecks.txt",log);Debug.Log("VOICE UX PLAY CHECKS: "+log.Count+" PASS");
}
c.StartCoroutine(Verify());return "UI and character generation check running";
