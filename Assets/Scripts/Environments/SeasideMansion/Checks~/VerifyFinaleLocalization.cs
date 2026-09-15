var old=HundredHour.Localization.GameLanguage.Current;
var c=UnityEngine.ScriptableObject.CreateInstance<HundredHour.RealityShow.ShowContent>();
int checks=0;
try {
foreach(var locale in new[]{HundredHour.Localization.GameLocale.English,HundredHour.Localization.GameLocale.Japanese}) {
HundredHour.Localization.GameLanguage.Select(locale,false);
foreach(bool female in new[]{false,true}) {
HundredHour.RealityShow.ShowContent.Populate(c);
HundredHour.RealityShow.RomanticLeadProfiles.Apply(c,female);
c.Person("himari").displayName=locale==HundredHour.Localization.GameLocale.English?"Player":"主人公";
var g=new HundredHour.RealityShow.ShowGame(c);g.Begin(77,HundredHour.RealityShow.ShowDifficulty.Easy);g.State.target="yuto";g.State.topic=c.Person("yuto").topics[0];
void Check(string text) {bool jp=HundredHour.Localization.GameLanguage.ContainsJapanese(text);if(jp!=(locale==HundredHour.Localization.GameLocale.Japanese))throw new Exception(text);checks++;}
foreach(bool known in new[]{false,true}) {g.State.introduced=known;Check(HundredHour.RealityShow.RomanticLeadProfiles.Opening(g));}
foreach(string id in new[]{"ask_name","introduce","greet","compliment","hold_hands","call_name","other"})Check(HundredHour.RealityShow.RomanticLeadProfiles.Local(g,new HundredHour.RealityShow.TalkAction{id=id}).reply);
foreach(string approach in new[]{"intro","greet","compliment","other"})foreach(int stage in new[]{0,1,2,3})foreach(int trust in new[]{0,45}) {g.State.firstApproach=approach;g.State.relationshipStage=stage;g.State.Player.trust=trust;Check(g.DeckEpilogue());}
}
}
return "PASS "+checks+" localized lead/ending cases";
} finally {HundredHour.Localization.GameLanguage.Select(old,false);UnityEngine.Object.DestroyImmediate(c);}
