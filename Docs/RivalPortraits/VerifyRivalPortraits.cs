var old=HundredHour.Localization.GameLanguage.Current;var results=new List<string>();
try{
foreach(var locale in new[]{HundredHour.Localization.GameLocale.Japanese,HundredHour.Localization.GameLocale.English}){
HundredHour.Localization.GameLanguage.Select(locale,false);
var c=ScriptableObject.CreateInstance<HundredHour.RealityShow.ShowContent>();HundredHour.RealityShow.ShowContent.Populate(c);
HundredHour.RealityShow.RomanticLeadProfiles.Apply(c,false);
var n=c.Person("konoa");var a=c.Person("hikari");
if(n.displayName!=(locale==HundredHour.Localization.GameLocale.English?"Nonoka Amamiya":"雨宮ののか"))throw new Exception("Nonoka locale");
if(n.portrait!=Resources.Load<Sprite>("AuditionCast/nonoka_romantic")||!n.portrait)throw new Exception("Nonoka portrait");
if(a.portrait!=Resources.Load<Sprite>("AuditionCast/anna_rival")||!a.portrait)throw new Exception("Anna portrait");
results.Add(locale+": "+n.displayName+" / "+AssetDatabase.GetAssetPath(n.portrait)+"; "+a.displayName+" / "+AssetDatabase.GetAssetPath(a.portrait));
UnityEngine.Object.DestroyImmediate(c);
var male=ScriptableObject.CreateInstance<HundredHour.RealityShow.ShowContent>();HundredHour.RealityShow.ShowContent.Populate(male);male.Person("konoa").displayName="Male Rival";var portrait=male.Person("konoa").portrait;HundredHour.RealityShow.RomanticLeadProfiles.Apply(male,true);
if(male.Person("konoa").displayName!="Male Rival"||male.Person("konoa").portrait!=portrait)throw new Exception("Male cast overwritten");UnityEngine.Object.DestroyImmediate(male);
}
results.Add("PASS both locale names, both Sprite references, male rival preservation");
}finally{HundredHour.Localization.GameLanguage.Select(old,false);}
System.IO.File.WriteAllLines("Docs/RivalPortraits/Verification.txt",results);return results;
