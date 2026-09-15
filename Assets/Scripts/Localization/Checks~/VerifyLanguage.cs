var report=new System.Text.StringBuilder();int checks=0;
void Check(bool ok,string name){if(!ok)throw new System.Exception("FAIL: "+name);checks++;report.AppendLine("PASS "+name);}
var old=HundredHour.Localization.GameLanguage.Current;
try{
 var asset=UnityEngine.Resources.Load<UnityEngine.TextAsset>("Localization/GameText");var catalog=UnityEngine.JsonUtility.FromJson<HundredHour.Localization.GameLanguage.Catalog>(asset.text);
 Check(catalog.entries.Length>100,"nonempty bilingual catalog");Check(catalog.entries.All(e=>!string.IsNullOrWhiteSpace(e.en)),"all registered entries have English");Check(catalog.entries.Select(e=>e.key).Distinct().Count()==catalog.entries.Length,"unique keys");
 HundredHour.Localization.GameLanguage.Select(HundredHour.Localization.GameLocale.English,false);
 Check(HundredHour.Localization.GameLanguage.Text("start.begin")=="Begin","stable key lookup");
 Check(HundredHour.Localization.GameLanguage.Text("ひまり")=="Himari","Japanese source lookup");
 Check(HundredHour.Localization.GameLanguage.Text("未登録の文章")=="未登録の文章","missing translation falls back");
 Check(HundredHour.Localization.GameLanguage.Text("ひまり / YOU")=="Himari / YOU","composed name label");
 Check(HundredHour.Localization.GameLanguage.Text("雨音の記憶のことを、今日は話してみたい。あなたはどう？")=="I’d like to talk about memories of rain today. What about you?","parameterized dialogue translated at display boundary");
 Check(HundredHour.Localization.GameLanguage.Format("mansion.status",6,3,20,12,"Clear").Contains("Focus 3/3"),"formatted counters");
 var director=UnityEngine.Object.FindFirstObjectByType<HundredHour.Environments.MansionArrivalDirector>();foreach(var c in director.openingCaptions){Check(HundredHour.Localization.GameLanguage.Text(c.heading)!=c.heading,"caption heading: "+c.heading);Check(HundredHour.Localization.GameLanguage.Text(c.text)!=c.text,"caption body");}
 var lines=(string[])typeof(HundredHour.Environments.MansionSignalDirector).GetField("RevealText",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static).GetValue(null);foreach(var line in lines)Check(HundredHour.Localization.GameLanguage.Text(line)!=line,"AI reveal translated");
 HundredHour.Localization.GameLanguage.Select(HundredHour.Localization.GameLocale.Japanese,false);Check(HundredHour.Localization.GameLanguage.Text("start.begin")=="ゲームを始める","return to Japanese");Check(HundredHour.Localization.GameLanguage.Text("ひまり / YOU")=="ひまり / YOU","source Japanese restored");
 report.AppendLine("Total "+checks+" passed; catalog entries "+catalog.entries.Length);
}finally{HundredHour.Localization.GameLanguage.Select(old,false);}
System.IO.File.WriteAllText("Docs/Localization/Verification.txt",report.ToString());return report.ToString();
