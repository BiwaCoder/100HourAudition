using TMPro;
using UnityEngine;
namespace HundredHour.UI.Audition
{
    [ExecuteAlways,DisallowMultipleComponent]
    public sealed class AuditionDescriptionView : MonoBehaviour
    {
        public const string DefaultDescription="あなたは、覚悟と期待と共にこの舞台にやってきた。\n\nタイムリープして時間が巻き戻る。AGIによる解説――\n「これは代理戦争だ」と。\nあなたはAGIの力を借りてゲームを勝ち抜かないといけない。\n\nAGIエナジーを使い、次のことができる。";
        [SerializeField,TextArea(6,12)] string description=DefaultDescription;
        [SerializeField] string[] abilities={"タイムリープ","他人の視点","未来観測","偶然の発動"};
        [SerializeField] TMP_Text body;
        [SerializeField] TMP_Text[] abilityLabels;
        bool dirty=true;
        public void Configure(TMP_Text text,TMP_Text[] labels){body=text;abilityLabels=labels;Refresh();}
        public void SetDescription(string value){description=value;Refresh();}
        public void Refresh()
        {
            if(body==null)return;body.text=description;
            if(abilityLabels!=null)for(int i=0;i<abilityLabels.Length;i++)
                abilityLabels[i].text=abilities!=null&&i<abilities.Length?abilities[i]:"";
            dirty=false;
        }
        void OnEnable()=>dirty=true;
        void OnValidate()=>dirty=true;
        void Update(){if(dirty)Refresh();}
    }
}
