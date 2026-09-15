var d=UnityEngine.Object.FindFirstObjectByType<HundredHour.Environments.MansionSignalDirector>();
if(!Application.isPlaying||!d)throw new Exception("Play SeasideMansion first");
d.saveEnabled=false;d.useAI=false;Application.runInBackground=true;
System.Collections.IEnumerator Verify(){
 var results=new System.Collections.Generic.List<string>();
 void Check(bool ok,string text){if(!ok)throw new Exception(text);results.Add("PASS "+text);}
 var language=UnityEngine.Object.FindFirstObjectByType<HundredHour.Localization.LanguageStartMenu>();
 if(language)language.Confirm();
 yield return null;yield return null;yield return null;
 d.arrival.cinema.Stop();d.arrival.view.ReleaseBlack();d.BeginEncounter();
 yield return null;
 d.Choose(1);yield return null;d.Choose(0);yield return null;d.Choose(0);yield return null;d.Choose(0);yield return null;
 while(d.Game.State.phase==HundredHour.RealityShow.ShowPhase.Awakening){d.Choose(0);yield return null;}
 d.Refresh();
 yield return new WaitForSecondsRealtime(1);
 Check(d.Game.State.phase==HundredHour.RealityShow.ShowPhase.Archetype,"archetype screen");
 ScreenCapture.CaptureScreenshot("Docs/ConversationDeck/00-archetype.png");yield return null;
 d.Choose(2);yield return null;d.Choose(0);yield return new WaitForSecondsRealtime(3);
 Check(d.Tutorial==1&&!d.view.choices.IsOpen,"AI support precedes cards and choices");
 int beforeTurn=d.Game.State.turn;d.UseTool(0);yield return null;
 Check(d.Tutorial==2&&d.Game.State.turn==beforeTurn,"forecast unlocks card explanation without advancing turn");
 Check(d.view.cards.Count(c=>c.gameObject.activeSelf)==3,"exactly three visible cards");
 Check(d.view.status.fontSize>=28&&d.view.cardLabels[0].fontSize>=26,"readable HUD and cards");
 ScreenCapture.CaptureScreenshot("Docs/ConversationDeck/01-introduction.png");yield return null;
 d.Game.State.chapter=1;d.Game.State.phase=HundredHour.RealityShow.ShowPhase.Route;
 // Match normal progression: one competitor has left, so the group is player + man + two rivals.
 d.Game.State.contestants.First(c=>c.id=="shiori").eliminated=true;
 d.Game.ChooseRoute(0);d.Refresh();yield return new WaitForSecondsRealtime(3);
 Check(GameObject.Find("Conversation Rival 0")&&GameObject.Find("Conversation Rival 1"),"two rival cubes active");
 Check(d.view.stage.GetComponentsInChildren<RectTransform>().Count(x=>x.name.StartsWith("Rival Portrait"))==2,"two rival portrait windows active");
 ScreenCapture.CaptureScreenshot("Docs/ConversationDeck/02-group.png");yield return null;
 d.Choose(0);yield return new WaitForSecondsRealtime(1);
 Check(d.Game.State.turn==1&&d.Game.State.interrupted,"group interruption telegraph");
 d.UseTool(1);yield return null;Check(GameObject.Find("Observed Conversation")!=null,"NPC conversation observation opens");
 ScreenCapture.CaptureScreenshot("Docs/ConversationDeck/03-observation.png");yield return null;
 UnityEngine.Object.Destroy(GameObject.Find("Observed Conversation"));
 d.Game.State.chapter=2;d.Game.State.phase=HundredHour.RealityShow.ShowPhase.Route;d.Game.ChooseRoute(0);d.Refresh();yield return new WaitForSecondsRealtime(3);
 Check(!GameObject.Find("Conversation Rival 0")&&!GameObject.Find("Conversation Rival 1"),"two-shot hides rivals");
 ScreenCapture.CaptureScreenshot("Docs/ConversationDeck/04-two-shot.png");yield return null;
 d.Game.State.hand=new System.Collections.Generic.List<string>{"garden_topic","flower","mystery"};d.Game.State.agi=8;d.Game.State.focus=3;d.Refresh();yield return null;
 d.PlayCard(0);yield return null;Check(d.Game.State.actions.Any(a=>a.id=="ai_topic")&&d.Game.State.agi==7,"AI card yields playable topic and charges once");
 d.testSavePath=System.IO.Path.GetFullPath("Temp/ConversationDeckSave.json");d.saveEnabled=true;d.PlayCard(0);yield return null;
 string saved=JsonUtility.ToJson(d.Game.State);d.Game.State.wisdom=99;d.Resume();yield return null;
 Check(saved==JsonUtility.ToJson(d.Game.State),"save/resume preserves mechanics");
 d.saveEnabled=false;
 System.IO.File.WriteAllLines("Docs/ConversationDeck/PlayVerification.txt",results);Debug.Log("DECK PLAY COMPLETE "+results.Count);
}
d.StartCoroutine(Verify());return "Deck Play checks running";
