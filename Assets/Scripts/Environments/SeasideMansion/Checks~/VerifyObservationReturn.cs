var d=UnityEngine.Object.FindFirstObjectByType<HundredHour.Environments.MansionSignalDirector>();
if(!d)throw new Exception("Open mansion");
System.Collections.IEnumerator Verify(){
 var lang=UnityEngine.Object.FindFirstObjectByType<HundredHour.Localization.LanguageStartMenu>();if(lang)lang.Confirm();yield return null;yield return null;yield return null;
 d.Resume();yield return null;
 if(!d.Started||d.ConversationStep!=1)throw new Exception("Expected saved card step");
 int agi=d.Game.State.agi,turn=d.Game.State.turnsSpoken;
 d.view.ShowObservation(d.Game.PerspectiveScene());yield return null;
 var modal=d.view.stage.Find("Observed Conversation");var button=modal.GetComponentInChildren<UnityEngine.UI.Button>(true);
 if(!button.gameObject.activeInHierarchy||!button.IsInteractable())throw new Exception("Return button unavailable");
 button.OnPointerClick(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current){button=UnityEngine.EventSystems.PointerEventData.InputButton.Left});yield return null;
 if(d.view.stage.Find("Observed Conversation")||!d.view.handPanel.activeSelf||d.ConversationStep!=1)throw new Exception("Return failed");
 if(d.Game.State.agi!=agi||d.Game.State.turnsSpoken!=turn)throw new Exception("Return changed game progress");
 System.IO.File.WriteAllText("Docs/ConversationDeck/ObservationReturnVerification.txt","PASS return button visible and interactable\nPASS pointer click closes observation and restores card step\nPASS no extra AI cost or turn consumption\n");
}
d.StartCoroutine(Verify());return "Verifying saved observation return";
