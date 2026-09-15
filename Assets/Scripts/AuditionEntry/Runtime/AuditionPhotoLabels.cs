using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HundredHour.Localization;
using HundredHour.PhotoIllustration;
namespace HundredHour.AuditionEntry
{
    public sealed class AuditionPhotoLabels:MonoBehaviour
    {
        public Text pick,convert,status;
        public TMP_Text title,source,result;
        public PhotoIllustrationDemoController controller;
        Coroutine waitFlavor;
        static readonly string[][] WaitLines={
            new[]{"変換中……ちょっと待っててくれ。","Working on it... hang tight."},
            new[]{"写真から人物像を組み立ててる。これが意外と時間かかるんだよな。","Building your portrait from the photo. Takes longer than you'd think."},
            new[]{"もう少し。いい感じに仕上がってくれよ……。","Almost there. Hoping this turns out good..."},
        };
        void OnEnable(){GameLanguage.Changed+=Refresh;Refresh();if(controller)controller.BusyChanged+=OnBusyChanged;}
        void OnDisable(){GameLanguage.Changed-=Refresh;if(controller)controller.BusyChanged-=OnBusyChanged;if(waitFlavor!=null){StopCoroutine(waitFlavor);waitFlavor=null;}}
        void Refresh()
        {
            pick.text=AuditionPortal.L("写真を選ぶ","Choose photo");convert.text=AuditionPortal.L("この写真からつくる","Create from photo");
            title.text=AuditionPortal.L("物語の中の、あなたの姿。","Your portrait in the story.");
            source.text=AuditionPortal.L("01 / 元の写真","01 / YOUR PHOTO");result.text=AuditionPortal.L("02 / AIとつくる姿","02 / AI PORTRAIT");
            status.text=AuditionPortal.L(
                "写真を選んで、あなただけの姿をつくりましょう。\nここに入れるのは自分の写真だけにしてくれよな。権利のない写真や、怒られちゃうような画像はナシだ。……仕上がりの中身は、正直オレも保証しないぜ？",
                "Choose a photo to create your portrait.\nOnly use your own photo here — nothing you don't have the rights to, nothing that'll get anyone in trouble. ...And no promises on exactly how it turns out.");
        }
        void OnBusyChanged(bool busy)
        {
            if(waitFlavor!=null){StopCoroutine(waitFlavor);waitFlavor=null;}
            if(busy)waitFlavor=StartCoroutine(WaitFlavorLoop());
        }
        IEnumerator WaitFlavorLoop()
        {
            // The generic component's own "変換しています……" message shows first; start rotating after that.
            yield return new WaitForSeconds(3.5f);
            int i=0;
            while(true)
            {
                status.text=WaitLines[i%WaitLines.Length][GameLanguage.Current==GameLocale.English?1:0];
                i++;
                yield return new WaitForSeconds(4f);
            }
        }
    }
}
