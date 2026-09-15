var log=new System.Collections.Generic.List<string>();
void Check(bool valid,string label){if(!valid)throw new Exception(label);log.Add("PASS: "+label);}
var f=new HundredHour.RealtimeVoice.VoiceInterviewFlow();
Check(!f.ReceiveTranscript("早すぎる発話")&&!f.Confirm(),"idle ignores input");
f.Begin();Check(!f.ReceiveTranscript("接続中"),"connecting ignores input");f.Connected();
Check(f.Prompt.Contains("マイクチェック")&&!f.ReceiveTranscript("案内中の音声"),"assistant speaks first; prompt does not accept input");
f.PromptFinished();Check(!f.ReceiveTranscript("  ")&&f.Listening,"empty mic test stays listening");
f.ReceiveTranscript("マイクチェックです");Check(!f.ReceiveTranscript("重複"),"review ignores duplicate transcript");
f.Retry();Check(f.Listening&&f.Candidate=="","retry clears candidate");f.ReceiveTranscript("こんにちは");f.Confirm();
Check(f.State==HundredHour.RealtimeVoice.VoiceInterviewState.Question1Prompt&&f.FirstAnswer==""&&f.SecondAnswer=="","mic test excluded from both answers");
f.PromptFinished();f.ReceiveTranscript("絵本と花が好き");f.Confirm();Check(f.QuestionNumber==2&&f.FirstAnswer=="絵本と花が好き","only confirmed Q1 advances");
f.PromptFinished();f.ReceiveTranscript("慎重です");f.Retry();f.ReceiveTranscript("穏やかで誠実です");f.Confirm();
Check(f.State==HundredHour.RealtimeVoice.VoiceInterviewState.Generating&&f.SecondAnswer=="穏やかで誠実です","Q2 retry replaces rather than adds an answer");
Check(!f.BuildInput.Contains("こんにちは")&&!f.BuildInput.Contains("慎重")&&f.BuildInput.Contains("回答1")&&f.BuildInput.Contains("回答2"),"JSON input contains exactly the two confirmed answers");
Check(!f.Confirm()&&!f.ReceiveTranscript("遅延した音声"),"generation ignores late events");
f.Complete("{\"name\":\"花音\"}");Check(f.State==HundredHour.RealtimeVoice.VoiceInterviewState.Complete,"completion");
f.Begin();Check(f.FirstAnswer==""&&f.SecondAnswer==""&&f.Result=="","restart clears prior run");f.Fail("test");f.Reset();Check(f.State==HundredHour.RealtimeVoice.VoiceInterviewState.Ready,"error and cancel return to ready");
System.IO.File.WriteAllLines("Docs/RealtimeVoiceUx/FlowChecks.txt",log);return string.Join("\n",log);
