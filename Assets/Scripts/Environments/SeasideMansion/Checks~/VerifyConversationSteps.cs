var d=UnityEngine.Object.FindFirstObjectByType<HundredHour.Environments.MansionSignalDirector>();
if(!d||!Application.isPlaying)throw new Exception("Play mansion first");
d.saveEnabled=false;d.useAI=false;
System.Collections.IEnumerator CheckFlow(){
 var notes=new System.Collections.Generic.List<string>();
 void Check(bool ok,string label){if(!ok)throw new Exception(label);notes.Add("PASS "+label);}
 var lang=UnityEngine.Object.FindFirstObjectByType<HundredHour.Localization.LanguageStartMenu>();if(lang)lang.Confirm();yield return null;yield return null;yield return null;
 d.arrival.cinema.Stop();d.arrival.enabled=false;d.BeginEncounter();yield return null;
 d.Game.State.phase=HundredHour.RealityShow.ShowPhase.Awakening;d.Game.Advance();d.Game.ChooseArchetype(0);d.Game.ChooseRoute(0);
 typeof(HundredHour.Environments.MansionSignalDirector).GetProperty("Tutorial").SetValue(d,1);d.Refresh();yield return null;
 Check(d.ConversationStep==0&&d.view.toolsPanel.activeSelf&&!d.view.handPanel.activeSelf,"AI only at start");
 Check(!d.Game.RelationshipFlavor.Contains("主人公")&&!d.Game.RelationshipFlavor.Contains("名前を覚え"),"named contextual inner voice");
 int spoken=d.Game.State.turnsSpoken;d.PlayCard(0);d.Choose(0);Check(d.ConversationStep==0&&d.Game.State.turnsSpoken==spoken,"tutorial cannot bypass AI");yield return null;
 d.UseTool(0);yield return null;
 Check(d.ConversationStep==1&&!d.view.toolsPanel.activeSelf&&d.view.handPanel.activeSelf,"forecast opens only cards");
 d.Choose(0);Check(d.ConversationStep==1,"tutorial waits for card");yield return null;
 int card=d.Game.State.hand.FindIndex(id=>d.Game.CanPlayDeckCard(d.Game.Card(id)));d.PlayCard(card);yield return null;
 Check(d.ConversationStep==2&&!d.view.handPanel.activeSelf&&!d.view.toolsPanel.activeSelf,"card opens only dialogue choices");
 int focus=d.Game.State.focus;d.PlayCard(0);Check(d.Game.State.focus==focus,"no second card after advancing");yield return null;
 d.Choose(0);yield return null;
 Check(d.Game.State.turnsSpoken==spoken+1&&d.ConversationStep==0,"speech resolves then resets to AI");
 d.Game.State.agi=0;d.Refresh();yield return null;d.Choose(0);yield return null;
 Check(d.ConversationStep==1,"AI can be skipped without points");
 d.Choose(0);yield return null;Check(d.ConversationStep==2,"card can be skipped after tutorial");
 System.IO.File.WriteAllLines("Docs/ConversationDeck/StepVerification.txt",notes);
}
d.StartCoroutine(CheckFlow());return "Step check running";
