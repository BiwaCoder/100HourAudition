using TMPro;
using UnityEngine;
namespace HundredHour.UI.Audition
{
    [ExecuteAlways,DisallowMultipleComponent]
    public sealed class AuditionTitleView : MonoBehaviour
    {
        [SerializeField] string firstLine="100Hour", secondLine="Audition";
        [SerializeField] Color titleColor=new Color(.08f,.22f,.32f),accentColor=new Color(.98f,.43f,.37f);
        [SerializeField] TMP_Text firstLabel,secondLabel;
        bool dirty=true;
        public void Configure(TMP_Text first,TMP_Text second){firstLabel=first;secondLabel=second;Refresh();}
        public void SetTitle(string first,string second){firstLine=first;secondLine=second;Refresh();}
        public void Refresh(){if(firstLabel==null||secondLabel==null)return;firstLabel.text=firstLine;firstLabel.color=titleColor;secondLabel.text=secondLine;secondLabel.color=accentColor;dirty=false;}
        void OnEnable()=>dirty=true;
        void OnValidate()=>dirty=true;
        void Update(){if(dirty)Refresh();}
    }
}
