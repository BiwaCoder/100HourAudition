var content=AssetDatabase.LoadAssetAtPath<HundredHour.RealityShow.ShowContent>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:ShowContent")[0]));
var g=new HundredHour.RealityShow.ShowGame(content,null,true){TelemetryEnabled=false};g.Begin(7,HundredHour.RealityShow.ShowDifficulty.Easy);g.StartLoop();g.ChooseRoute(0);
var old=HundredHour.Localization.GameLanguage.Current;var notes=new List<string>();
try{
foreach(var locale in new[]{HundredHour.Localization.GameLocale.Japanese,HundredHour.Localization.GameLocale.English}){
HundredHour.Localization.GameLanguage.Select(locale,false);var s=g.State;s.phase=HundredHour.RealityShow.ShowPhase.Conversation;
s.conversations.Clear();s.conversations.Add(new HundredHour.RealityShow.ConversationMemory{id="final-date-memory",partner=s.target,topic=locale==HundredHour.Localization.GameLocale.English?"a quiet cafe":"静かなカフェ",reply="",loop=s.loop,timeline=s.timeLeaps});s.recallA="final-date-memory";s.recallB="";
foreach(int chapter in new[]{0,1,2}){s.chapter=chapter;
for(int turn=0;turn<4;turn++){s.turn=turn;s.focus=3;s.composedThisTurn=false;
if(!g.ComposeRecall()||s.focus!=2)throw new Exception("Focus action failed");
var line=s.actions.First(a=>a.id=="recall_memory").line;
if(chapter<2&&(!string.IsNullOrEmpty(g.FinalDateTopicDirection(true))||line.Contains("marry")||line.Contains("結婚")))throw new Exception("Earlier chapter changed");
if(chapter==2){if(turn==3&&!(line.Contains("marry me")||line.Contains("結婚して")))throw new Exception("No proposal");notes.Add(locale+" turn="+turn+" "+line);}
}
}
}
notes.Add("PASS: both locales, early chapter isolation, 4 final-date beats, Focus cost and explicit proposal");
}finally{HundredHour.Localization.GameLanguage.Select(old,false);}
System.IO.File.WriteAllLines("Docs/FinalDateTopics/LocalVerification.txt",notes);return notes;
