var d=UnityEngine.Object.FindFirstObjectByType<HundredHour.Environments.MansionSignalDirector>();
System.Collections.IEnumerator Inspect(){
 var results=new System.Collections.Generic.List<string>();
 d.saveEnabled=false;d.useAI=false;
 d.GetComponent<HundredHour.Environments.MansionConversationStage>().React(d.Game.State);
 yield return new WaitForSecondsRealtime(1.8f);var close=d.arrival.controls.outputCamera.transform.position;
 yield return new WaitForSecondsRealtime(4f);var wide=d.arrival.controls.outputCamera.transform.position;
 if(Vector3.Distance(close,wide)<.2f)throw new Exception("Reaction camera did not pull back");results.Add("PASS camera pushes in then returns to wide");
 d.Game.State.phase=HundredHour.RealityShow.ShowPhase.Reward;d.Game.State.rewardFromAbility=false;d.Game.State.offers=d.Game.Balance.cards.Where(c=>c.archetype=="strategic"&&c.rarity>0).Take(3).Select(c=>c.id).ToList();d.Refresh();yield return null;yield return null;
 foreach(var t in d.view.choices.GetComponentsInChildren<TMPro.TMP_Text>()){t.ForceMeshUpdate();if(t.isTextOverflowing)throw new Exception("Reward text overflow: "+t.text);}
 results.Add("PASS full rare reward descriptions fit");ScreenCapture.CaptureScreenshot("Docs/ConversationDeck/06-rare-reward.png");yield return null;
 System.IO.File.WriteAllLines("Docs/ConversationDeck/PresentationVerification.txt",results);
}
d.StartCoroutine(Inspect());return "Presentation verification running";
