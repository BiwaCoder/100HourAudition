using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace HundredHour.Localization
{
 /// <summary>Scene-local start gate. Selection is saved; confirmation is required on each new game launch.</summary>
 public sealed class LanguageStartMenu : MonoBehaviour
 {
  public static bool Ready {get;private set;}
  static LanguageStartMenu current;
  TMP_Text startLabel;Button japanese,english;TMP_Text jaLabel,enLabel;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void Reset(){Ready=false;current=null;}
  public static IEnumerator WaitForChoice(TMP_FontAsset font)
  {
   if(UnityEngine.Object.FindFirstObjectByType<HundredHour.AuditionEntry.AuditionSceneBridge>()?.game != null && HundredHour.AuditionEntry.AuditionPortal.LanguageConfirmed){Ready=true;yield break;}
   Ready=false;
   if(!current){var go=new GameObject("Language selection",typeof(RectTransform));current=go.AddComponent<LanguageStartMenu>();current.Build(font);}
   while(!Ready&&current)yield return null;
  }
  void Build(TMP_FontAsset font)
  {
   var canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=32000;
   var scaler=gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1440,900);scaler.matchWidthOrHeight=.5f;gameObject.AddComponent<GraphicRaycaster>();
   var background=gameObject.AddComponent<Image>();background.color=new Color(.018f,.035f,.058f,.99f);
   Label("100 HOUR AUDITION",font,new Vector2(0,180),new Vector2(1000,70),42);
   Label("言語を選んでください / Choose your language",font,new Vector2(0,80),new Vector2(1200,70),28);
   japanese=MakeButton("日本語",font,new Vector2(-185,-25),new Vector2(330,86),out jaLabel);japanese.onClick.AddListener(()=>Choose(GameLocale.Japanese));
   english=MakeButton("English",font,new Vector2(185,-25),new Vector2(330,86),out enLabel);english.onClick.AddListener(()=>Choose(GameLocale.English));
   var start=MakeButton("",font,new Vector2(0,-165),new Vector2(440,82),out startLabel);start.onClick.AddListener(Confirm);
   Refresh();if(EventSystem.current)EventSystem.current.SetSelectedGameObject((GameLanguage.Current==GameLocale.Japanese?japanese:english).gameObject);
  }
  TMP_Text Label(string text,TMP_FontAsset font,Vector2 pos,Vector2 size,float fontSize)
  {
   var go=new GameObject("Label",typeof(RectTransform));go.transform.SetParent(transform,false);var rt=(RectTransform)go.transform;rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(.5f,.5f);rt.anchoredPosition=pos;rt.sizeDelta=size;
   var label=go.AddComponent<TextMeshProUGUI>();label.font=font;label.fontSize=fontSize;label.alignment=TextAlignmentOptions.Center;label.color=new Color(.91f,.94f,.96f);label.raycastTarget=false;label.text=text;return label;
  }
  Button MakeButton(string text,TMP_FontAsset font,Vector2 pos,Vector2 size,out TMP_Text label)
  {
   var go=new GameObject(text+" Button",typeof(RectTransform),typeof(Image),typeof(Button));go.transform.SetParent(transform,false);var rt=(RectTransform)go.transform;rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(.5f,.5f);rt.anchoredPosition=pos;rt.sizeDelta=size;
   var button=go.GetComponent<Button>();button.targetGraphic=go.GetComponent<Image>();label=Label(text,font,pos,size,30);label.transform.SetParent(go.transform,true);((RectTransform)label.transform).anchoredPosition=Vector2.zero;return button;
  }
  void Choose(GameLocale locale){GameLanguage.Select(locale);Refresh();}
  void Refresh(){bool ja=GameLanguage.Current==GameLocale.Japanese;japanese.GetComponent<Image>().color=ja?new Color(.21f,.42f,.45f):new Color(.08f,.14f,.20f);english.GetComponent<Image>().color=!ja?new Color(.21f,.42f,.45f):new Color(.08f,.14f,.20f);jaLabel.text=ja?"✓ 日本語":"日本語";enLabel.text=ja?"English":"✓ English";startLabel.text=GameLanguage.Text("start.begin");startLabel.transform.parent.GetComponent<Image>().color=new Color(.28f,.24f,.15f);}
  public void Confirm(){Ready=true;gameObject.SetActive(false);Destroy(gameObject);}
  void OnDestroy(){if(current==this)current=null;}
 }
}
