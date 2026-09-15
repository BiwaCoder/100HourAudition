var d=UnityEngine.Object.FindFirstObjectByType<HundredHour.Environments.MansionSignalDirector>();
if(!Application.isPlaying||!d)throw new Exception("Play required");
System.Collections.IEnumerator VerifyAI(){
 d.useAI=true;d.saveEnabled=false;var s=d.Game.State;s.phase=HundredHour.RealityShow.ShowPhase.Conversation;s.hand=new System.Collections.Generic.List<string>{"garden_topic","flower","mystery"};s.agi=8;s.focus=3;s.pendingTopic="";d.Refresh();yield return null;
 int turn=s.turn;d.PlayCard(0);int agi=s.agi;d.PlayCard(0);d.Choose(0);
 if(s.agi!=agi||s.turn!=turn)throw new Exception("busy double activation");
 float end=Time.realtimeSinceStartup+45;while(d.Busy&&Time.realtimeSinceStartup<end)yield return null;
 if(d.Busy)throw new Exception("AI request timeout");
 if(s.agi!=7||!s.actions.Any(a=>a.id=="ai_topic"))throw new Exception("AI card not resolved exactly once");
 string source=d.LastDialogueSource;string topic=s.topic;d.Refresh();yield return null;ScreenCapture.CaptureScreenshot("Docs/ConversationDeck/05-generated-topic.png");yield return null;
 d.Choose(s.actions.FindIndex(a=>a.id=="ai_topic"));end=Time.realtimeSinceStartup+45;while(d.Busy&&Time.realtimeSinceStartup<end)yield return null;
 if(d.Busy||s.turn!=turn+1)throw new Exception("AI dialogue failed to advance once");
 System.IO.File.WriteAllText("Docs/ConversationDeck/AIVerification.txt","topicSource="+source+"\ntopic="+topic+"\ndialogueSource="+d.LastDialogueSource+"\nPASS busy guard, one charge, generated choice, one resolution\n");
 d.useAI=false;Debug.Log("DECK AI COMPLETE");
}
d.StartCoroutine(VerifyAI());return "AI verification running";
