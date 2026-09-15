var d=UnityEngine.Object.FindFirstObjectByType<HundredHour.Environments.MansionSignalDirector>();d.saveEnabled=false;d.useAI=false;
System.Collections.IEnumerator Verify(){
 var menu=UnityEngine.Object.FindFirstObjectByType<HundredHour.Localization.LanguageStartMenu>();if(menu)menu.Confirm();yield return null;yield return null;
 d.arrival.StopAllCoroutines();d.arrival.cinema.Stop();d.arrival.view.ReleaseBlack();
 d.BeginEncounter();var g=d.Game;g.StartLoop();g.ChooseRoute(0);
 var flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
 d.GetType().GetField("<StoryBeat>k__BackingField",flags).SetValue(d,2);d.GetType().GetField("<ConversationStep>k__BackingField",flags).SetValue(d,2);
 for(int i=0;i<20;i++)g.State.conversations.Add(new HundredHour.RealityShow.ConversationMemory{id="ui"+i,partner="yuto",loop=i<10?0:g.State.loop,topic=i%2==0?"カフェ巡り":"映画鑑賞",reply=i%2==0?"カフェ巡りが好き！ 静かなお店で、本を読みながらゆっくりする時間が大好きなんだ。":"映画鑑賞が好きで、とくに旅に出る物語が好きなの。今度、感想を聞かせてね。"});
 d.Refresh();d.view.CloseChoiceDialog();var journal=d.GetComponent<HundredHour.Environments.MansionMemoryJournal>();journal.Open();
 yield return null;Canvas.ForceUpdateCanvases();
 var panel=d.view.stage.Find("ConversationMemoryWindow");var scroll=panel.Find("MemoryList").GetComponent<UnityEngine.UI.ScrollRect>();
 if(scroll.content.childCount!=20||scroll.content.rect.height<=scroll.viewport.rect.height)throw new Exception("list is not scrollable");
 scroll.content.Find("Memory_ui19").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();yield return null;
 scroll.content.Find("Memory_ui18").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();yield return null;
 if(g.State.recallA!="ui19"||g.State.recallB!="ui18")throw new Exception("row selection failed");
 if(scroll.content.Find("Memory_ui17").GetComponent<UnityEngine.UI.Button>().interactable)throw new Exception("third selection allowed");
 if(panel.Find("PreviousMemory")||panel.Find("SelectMemoryA"))throw new Exception("legacy navigation present");
 if(((RectTransform)d.view.toolsPanel.transform).anchorMax.y>=journal.ToggleRect.anchorMin.y)throw new Exception("tools overlap memory button");
 ScreenCapture.CaptureScreenshot("/tmp/memory-list.png");yield return null;
 scroll.verticalNormalizedPosition=0;yield return null;
 ScreenCapture.CaptureScreenshot("/tmp/memory-list-bottom.png");yield return null;
 g.State.recallA="ui0";g.State.recallB="ui1";d.RecallChanged();
 panel.Find("RecallWithoutAI").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();yield return null;
 if(journal.IsOpen||!g.State.actions.Any(a=>a.id=="recall_memory"&&a.line.Contains("カフェ巡り")&&a.line.Contains("映画鑑賞")))throw new Exception("compose button failed");
 g.Log("未表示の設定","ログに出てはいけない");var log=d.GetComponent<HundredHour.Environments.MansionLogViewer>();log.Open();yield return null;
 var entries=d.view.stage.Find("ConversationLogWindow").GetComponentsInChildren<TMPro.TMP_Text>();
 if(entries.Any(t=>t.text.Contains("ログに出てはいけない")))throw new Exception("internal data leaked");
 ScreenCapture.CaptureScreenshot("/tmp/visible-log.png");log.Close();
 System.IO.File.WriteAllText("/tmp/memory-ui.txt","PASS scrollable rows, two selections, third blocked, no A/B navigation, non-overlapping toolbar, compose, visible-only log");
 // Exercise the real two-memory AI route without gameplay writes.
 g.State.acquired=false;g.State.recallA="ui0";g.State.recallB="ui1";g.State.agi=12;d.useAI=true;
 d.GenerateFromMemories();float deadline=Time.realtimeSinceStartup+85;
 while(d.Busy&&Time.realtimeSinceStartup<deadline)yield return null;
 System.IO.File.WriteAllText("/tmp/memory-ai.txt",d.LastDialogueSource+"\n"+d.LastAIError+"\n"+(g.State.actions.FirstOrDefault(a=>a.id=="ai_topic")?.line??"missing topic"));
}
d.StartCoroutine(Verify());return "verification running";
