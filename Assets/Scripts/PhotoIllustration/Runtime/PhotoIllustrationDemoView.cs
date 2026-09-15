using System;
using HundredHour.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace HundredHour.PhotoIllustration
{
    /// <summary>写真選択・変換ボタン・元画像/結果画像表示のUnityEngine.UIラッパー。ロジックは持たない。</summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("PhotoIllustration/Photo Illustration Demo View")]
    public sealed class PhotoIllustrationDemoView : MonoBehaviour
    {
        [SerializeField] RawImage sourceImage;
        [SerializeField] RawImage resultImage;
        [SerializeField] Button pickButton;
        [SerializeField] Button convertButton;
        bool converted; // one conversion per chosen photo; pick a new photo (or re-enter) to convert again
        [SerializeField] Text statusText;

        public event Action<Texture2D> ResultCreated;
        public event Action PickRequested;
        public event Action ConvertRequested;

        void OnEnable()
        {
            pickButton.onClick.AddListener(HandlePick);
            convertButton.onClick.AddListener(HandleConvert);
            convertButton.interactable = false;
        }

        void OnDisable()
        {
            pickButton.onClick.RemoveListener(HandlePick);
            convertButton.onClick.RemoveListener(HandleConvert);
        }

        void HandlePick() => PickRequested?.Invoke();
        void HandleConvert() => ConvertRequested?.Invoke();

        public void ShowSourceTexture(Texture2D tex)
        {
            sourceImage.texture = tex;
            sourceImage.color=Color.white;
            Fit(sourceImage,tex);
            converted = false;
            convertButton.interactable = tex != null;
        }

        public void SetBusy(bool busy)
        {
            convertButton.interactable = !busy && sourceImage.texture != null && !converted;
            pickButton.interactable = !busy;
            if (busy) statusText.text = "変換しています……(20秒ほどかかります)";
        }

        public void ShowResult(Texture2D tex, string impression)
        {
            resultImage.texture = tex;
            resultImage.color=Color.white;
            Fit(resultImage,tex);
            ResultCreated?.Invoke(tex);
            converted = true; convertButton.interactable = false;
            // The model's impression of the photo is not shown; it read as an odd disclaimer under the portrait.
            statusText.text = "";
        }

        static void Fit(RawImage image,Texture2D texture)
        {
            if(!texture)return;
            var fit=image.GetComponent<AspectRatioFitter>();
            if(!fit)
            {
                var original=image.rectTransform;
                var holder=new GameObject(image.name+" Bounds",typeof(RectTransform)).GetComponent<RectTransform>();
                holder.SetParent(original.parent,false);holder.SetSiblingIndex(original.GetSiblingIndex());
                holder.anchorMin=original.anchorMin;holder.anchorMax=original.anchorMax;holder.pivot=original.pivot;
                holder.sizeDelta=original.sizeDelta;holder.anchoredPosition=original.anchoredPosition;
                original.SetParent(holder,false);original.anchorMin=Vector2.zero;original.anchorMax=Vector2.one;original.offsetMin=original.offsetMax=Vector2.zero;
                fit=image.gameObject.AddComponent<AspectRatioFitter>();
            }
            fit.aspectMode=AspectRatioFitter.AspectMode.FitInParent;fit.aspectRatio=(float)texture.width/texture.height;
        }
        public void ShowError(string message) => statusText.text = (GameLanguage.Current==GameLocale.English?"Error: ":"エラー: ") + message;
    }
}
