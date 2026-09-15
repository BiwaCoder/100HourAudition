using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace HundredHour.UI.Timeline
{
    public sealed class TimelineFeedView : MonoBehaviour
    {
        [SerializeField] RectTransform rowsRoot;
        [SerializeField] CanvasGroup contentFade;
        [SerializeField] TimelineRowView rowPrefab;
        [SerializeField] UnityEngine.UI.Button nextButton;
        [SerializeField] TMP_Text buttonLabel,pageLabel,emptyLabel;
        [SerializeField] UnityEngine.UI.LayoutElement bodyLayout;
        readonly List<TimelineRowView> rows=new List<TimelineRowView>();
        bool hasNext;
        public UnityEngine.UI.Button NextButton=>nextButton;
        public float Opacity {get=>contentFade.alpha;set=>contentFade.alpha=value;}
        public int DisplayedCount=>rows.Count;
        public float ContentWidth=>Mathf.Max(160,((RectTransform)transform).rect.width-40);
        public void Configure(RectTransform content,CanvasGroup fade,TimelineRowView prefab,UnityEngine.UI.Button button,TMP_Text buttonText,TMP_Text pageText,TMP_Text empty,UnityEngine.UI.LayoutElement body)
        {rowsRoot=content;contentFade=fade;rowPrefab=prefab;nextButton=button;buttonLabel=buttonText;pageLabel=pageText;emptyLabel=empty;bodyLayout=body;}
        public void Render(IReadOnlyList<TimelineEntry> entries,TimelinePaginationModel page)
        {
            ClearRows();
            float height=0,width=ContentWidth;
            for(int i=0;i<page.VisibleCount;i++)
            {
                var row=Instantiate(rowPrefab,rowsRoot,false);
                if(!Application.isPlaying)row.gameObject.hideFlags=HideFlags.DontSave;
                row.Bind(entries[page.StartIndex+i],width);rows.Add(row);height+=row.PreferredHeight;
            }
            height+=Mathf.Max(0,rows.Count-1)*8;
            bodyLayout.minHeight=bodyLayout.preferredHeight=Mathf.Max(52,height);
            emptyLabel.gameObject.SetActive(page.TotalCount==0);
            hasNext=page.HasNext;
            pageLabel.text=page.TotalCount==0?"0 / 0":$"{page.StartIndex+1}–{page.StartIndex+page.VisibleCount} / {page.TotalCount} 件";
            int nextCount=System.Math.Min(page.PageSize,page.TotalCount-page.StartIndex-page.VisibleCount);
            buttonLabel.text=hasNext?$"次の{nextCount}件を見る  →":page.TotalCount==0?"投稿を待っています":"これで全件です";
            SetBusy(false);
            UnityEngine.UI.LayoutRebuilder.MarkLayoutForRebuild((RectTransform)transform);
        }
        public void SetBusy(bool busy){if(nextButton!=null)nextButton.interactable=hasNext&&!busy;}
        public void ClearRows()
        {
            rows.Clear();if(rowsRoot==null)return;
            // このコンテナにはView自身が生成した行だけを置く。
            var children=new List<GameObject>();foreach(Transform t in rowsRoot)children.Add(t.gameObject);
            foreach(var child in children){child.SetActive(false);if(Application.isPlaying)Destroy(child);else DestroyImmediate(child);}
        }
        void OnDestroy()=>ClearRows();
    }
}
