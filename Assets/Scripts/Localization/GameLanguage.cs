using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
namespace HundredHour.Localization
{
 public enum GameLocale { Japanese, English }
 /// <summary>Small, presentation-only dictionary. No dependency on Unity Localization or Addressables.</summary>
 public static class GameLanguage
 {
  const string Preference="100Hour.Language";
  [Serializable] public sealed class Entry { public string key,ja,en; }
  [Serializable] public sealed class Catalog { public Entry[] entries; }
  static Dictionary<string,Entry> entries;
  sealed class Template {public Entry entry;public Regex pattern;public int count;}
  static List<Template> templates;
  static GameLocale current;
  static bool initialized;
  public static event Action Changed;
  public static GameLocale Current {get{Initialize();return current;}}
  public static int EntryCount {get{Initialize();return entries.Count;}}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
  static void Reset(){entries=null;initialized=false;Changed=null;}
  static void Initialize()
  {
   if(initialized)return;initialized=true;
   current=PlayerPrefs.GetString(Preference,"ja")=="en"?GameLocale.English:GameLocale.Japanese;
   entries=new Dictionary<string,Entry>(StringComparer.Ordinal);templates=new List<Template>();
   var asset=Resources.Load<TextAsset>("Localization/GameText");
   if(!asset){Debug.LogWarning("GameText translation catalog is missing; displaying source text.");return;}
   foreach(var e in JsonUtility.FromJson<Catalog>(asset.text).entries){
    if(string.IsNullOrEmpty(e.key)||string.IsNullOrEmpty(e.ja))continue;
    if(entries.ContainsKey(e.key)){Debug.LogWarning("Duplicate translation key: "+e.key);continue;}
    entries[e.key]=e;if(!entries.ContainsKey(e.ja))entries[e.ja]=e;
    if(e.ja.Contains("{0}")){
     string pattern=Regex.Escape(e.ja);int count=0;
     while(pattern.Contains("\\{"+count+"}")){pattern=pattern.Replace("\\{"+count+"}","(.*?)");count++;}
     if(count>0)templates.Add(new Template{entry=e,count=count,pattern=new Regex("^"+pattern+"$",RegexOptions.CultureInvariant)});
    }
   }
  }
  public static void Select(GameLocale locale,bool persist=true)
  {
   Initialize();if(!Enum.IsDefined(typeof(GameLocale),locale))throw new ArgumentOutOfRangeException(nameof(locale));
   bool changed=current!=locale;current=locale;
   if(persist){PlayerPrefs.SetString(Preference,locale==GameLocale.English?"en":"ja");PlayerPrefs.Save();}
   if(changed)Changed?.Invoke();
  }
  public static string Text(string source)
  {
   Initialize();if(string.IsNullOrEmpty(source))return source;
   if(entries.TryGetValue(source,out var e))return current==GameLocale.English&&!string.IsNullOrEmpty(e.en)?e.en:e.ja;
   if(current==GameLocale.Japanese)return source;
   foreach(var t in templates){var match=t.pattern.Match(source);if(!match.Success)continue;var args=new object[t.count];for(int i=0;i<args.Length;i++)args[i]=Text(match.Groups[i+1].Value);return string.Format(CultureInfo.GetCultureInfo("en-US"),string.IsNullOrEmpty(t.entry.en)?t.entry.ja:t.entry.en,args);}
   // Composed UI uses these explicit separators. Never rewrite game data or arbitrary substrings.
   foreach(var separator in new[]{"\n"," · "," / "," ／ ","："}){
    int at=source.IndexOf(separator,StringComparison.Ordinal);
    if(at>=0)return Text(source.Substring(0,at))+separator+Text(source.Substring(at+separator.Length));
   }
   // A Japanese sentence period joins two clauses; in English it reads better as ". " rather than a lone "。".
   {
    int at=source.IndexOf('。');
    if(at>=0&&at<source.Length-1)return Text(source.Substring(0,at+1).TrimEnd('。'))+". "+Text(source.Substring(at+1));
   }
   if(source.StartsWith("［予測］ ",StringComparison.Ordinal))return "[Forecast] "+Text(source.Substring(5));
   return source;
  }
  public static string Format(string key,params object[] args)=>string.Format(Current==GameLocale.English?CultureInfo.GetCultureInfo("en-US"):CultureInfo.GetCultureInfo("ja-JP"),Text(key),args);
  static readonly Regex JapaneseChars=new Regex("[぀-ヿ一-鿿]",RegexOptions.CultureInvariant);
  /// <summary>True if the text contains hiragana, katakana, or kanji -- used to catch an AI reply that ignored an English-only instruction.</summary>
  public static bool ContainsJapanese(string text)=>!string.IsNullOrEmpty(text)&&JapaneseChars.IsMatch(text);
 }
}
