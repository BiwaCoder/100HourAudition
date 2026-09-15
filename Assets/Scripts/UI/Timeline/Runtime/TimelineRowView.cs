using System.Globalization;
using TMPro;
using UnityEngine;
namespace HundredHour.UI.Timeline
{
    public sealed class TimelineRowView : MonoBehaviour
    {
        [SerializeField] UnityEngine.UI.Image avatar;
        [SerializeField] UnityEngine.UI.AspectRatioFitter avatarFit;
        [SerializeField] TMP_Text initial,speaker,action,dialogue,timeLabel,likes,stars,views;
        [SerializeField] RectTransform reactions;
        [SerializeField] UnityEngine.UI.LayoutElement layout;
        public float PreferredHeight=>layout.preferredHeight;
        public void Configure(UnityEngine.UI.Image image,UnityEngine.UI.AspectRatioFitter fit,TMP_Text initialText,TMP_Text name,TMP_Text summary,TMP_Text quote,TMP_Text time,
            TMP_Text likeText,TMP_Text starText,TMP_Text viewText,RectTransform badges,UnityEngine.UI.LayoutElement element)
        {avatar=image;avatarFit=fit;initial=initialText;speaker=name;action=summary;dialogue=quote;timeLabel=time;likes=likeText;stars=starText;views=viewText;reactions=badges;layout=element;}
        public void Bind(TimelineEntry entry,float width)
        {
            avatar.sprite=entry.avatar;avatar.enabled=entry.avatar!=null;
            if(entry.avatar!=null)avatarFit.aspectRatio=entry.avatar.rect.width/entry.avatar.rect.height;
            initial.gameObject.SetActive(entry.avatar==null);initial.text=string.IsNullOrEmpty(entry.speaker)?"?":entry.speaker.Substring(0,1);
            speaker.text=entry.speaker??"";action.text=entry.action??"";dialogue.text=entry.dialogue??"";timeLabel.text=entry.timeLabel??"";
            float textWidth=Mathf.Max(80,width-112);
            float nameWidth=Mathf.Min(textWidth*.35f,speaker.GetPreferredValues(speaker.text).x+4);
            speaker.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,nameWidth);
            var ar=action.rectTransform;ar.anchoredPosition=new Vector2(92+nameWidth+8,-13);ar.sizeDelta=new Vector2(Mathf.Max(30,width-(92+nameWidth+8)-95),23);
            dialogue.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,textWidth);
            float quoteHeight=Mathf.Max(24,dialogue.GetPreferredValues(dialogue.text,textWidth,0).y);
            dialogue.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,quoteHeight);
            float height=42+quoteHeight+(entry.showReactions?38:14);
            layout.minHeight=layout.preferredHeight=Mathf.Max(86,height);
            reactions.gameObject.SetActive(entry.showReactions);
            likes.text="LIKES "+Compact(entry.likes);stars.text="STARS "+Compact(entry.stars);views.text="VIEWS "+Compact(entry.views);
        }
        static string Compact(long value)
        {
            value=System.Math.Max(0,value);
            if(value>=1000000000)return (value/1000000000d).ToString("0.#",CultureInfo.InvariantCulture)+"B";
            if(value>=1000000)return (value/1000000d).ToString("0.#",CultureInfo.InvariantCulture)+"M";
            if(value>=1000)return (value/1000d).ToString("0.#",CultureInfo.InvariantCulture)+"K";
            return value.ToString(CultureInfo.InvariantCulture);
        }
    }
}
