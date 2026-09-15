using TMPro;
using UnityEngine;
namespace HundredHour.Localization
{
 /// <summary>Opt-in binding for a static TMP or legacy Text label. Size, font and layout are untouched.</summary>
 [DisallowMultipleComponent]
 public sealed class LocalizedText : MonoBehaviour
 {
  public string key;
  [TextArea] public string japaneseFallback;
  TMP_Text tmp;UnityEngine.UI.Text legacy;
  void Awake(){tmp=GetComponent<TMP_Text>();legacy=GetComponent<UnityEngine.UI.Text>();if(string.IsNullOrEmpty(japaneseFallback))japaneseFallback=tmp?tmp.text:legacy?legacy.text:"";}
  void OnEnable(){GameLanguage.Changed+=Refresh;Refresh();}
  void OnDisable(){GameLanguage.Changed-=Refresh;}
  public void Refresh(){string source=string.IsNullOrEmpty(key)?japaneseFallback:key;string translated=GameLanguage.Text(source);if(translated==key&&!string.IsNullOrEmpty(japaneseFallback))translated=japaneseFallback;if(tmp)tmp.text=translated;else if(legacy)legacy.text=translated;}
 }
}
