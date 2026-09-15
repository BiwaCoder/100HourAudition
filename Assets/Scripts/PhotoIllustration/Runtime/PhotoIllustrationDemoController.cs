using System;
using HundredHour.Localization;
using UnityEngine;

namespace HundredHour.PhotoIllustration
{
    /// <summary>PhotoIllustrationDemoViewの入力をPhotoIllustrationApiClientへ渡し、結果をViewへ戻す。</summary>
    [RequireComponent(typeof(PhotoIllustrationDemoView))]
    [AddComponentMenu("PhotoIllustration/Photo Illustration Demo Controller")]
    public sealed class PhotoIllustrationDemoController : MonoBehaviour
    {
        static bool English=>GameLanguage.Current==GameLocale.English;
#if UNITY_WEBGL && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")] static extern void PhotoPicker_Open(string objectName,string methodName);
#endif
        [SerializeField] PhotoIllustrationApiClient apiClient;
        [Tooltip("エディタ以外(ビルド後)でファイル選択が使えない場合に使う既定の写真")]
        [SerializeField] Texture2D fallbackTexture;

        PhotoIllustrationDemoView view;
        byte[] pendingJpegBytes;
        Texture2D ownedSource,ownedResult;
        int revision;
        /// <summary>Fires around SetBusy, so scene-specific wrappers (e.g. AuditionPhotoLabels) can
        /// show their own flavor text during the wait without this shared demo component knowing about it.</summary>
        public event Action<bool> BusyChanged;

        void OnEnable()
        {
            view = GetComponent<PhotoIllustrationDemoView>();
            view.PickRequested += HandlePickRequested;
            view.ConvertRequested += HandleConvertRequested;

            if (fallbackTexture != null) LoadFallbackTexture();
        }

        void OnDisable()
        {
            revision++;if(apiClient)apiClient.Cancel();
            if (view == null) return;
            view.PickRequested -= HandlePickRequested;
            view.ConvertRequested -= HandleConvertRequested;
        }

        void HandlePickRequested()
        {
#if UNITY_EDITOR
            string path = UnityEditor.EditorUtility.OpenFilePanel(English?"Choose a photo":"写真を選択", "", "png,jpg,jpeg");
            if (string.IsNullOrEmpty(path)) return;
            LoadPath(path);
#elif UNITY_WEBGL
            PhotoPicker_Open(gameObject.name,nameof(OnWebPhotoSelected));
#elif UNITY_STANDALONE_OSX
            StartCoroutine(PickMacPhoto());
#else
            view.ShowError(English?"File picking isn't supported in this environment. Please use the default photo.":"この環境ではファイル選択に未対応です。既定の写真をお使いください。");
#endif
        }
        // Called by PhotoPicker.jslib via SendMessage once the browser's file input resolves --
        // the WebGL sandbox has no filesystem path, so the JS side hands back the bytes directly.
        public void OnWebPhotoSelected(string base64)
        {
            try{LoadFromBytes(Convert.FromBase64String(base64));}
            catch(Exception e){view.ShowError(e.Message);}
        }
        public void OnWebPhotoSelectedFailed(string reason)=>view.ShowError(English?"Couldn't read that photo: "+reason:"写真を読み込めませんでした："+reason);

        void LoadPath(string path)
        {
            try{LoadFromBytes(System.IO.File.ReadAllBytes(path));}
            catch(Exception e){view.ShowError(e.Message);}
        }
#if UNITY_STANDALONE_OSX && !UNITY_EDITOR
        System.Collections.IEnumerator PickMacPhoto()
        {
            var info=new System.Diagnostics.ProcessStartInfo("/usr/bin/osascript") {UseShellExecute=false,RedirectStandardInput=true,RedirectStandardOutput=true,RedirectStandardError=true,CreateNoWindow=true};
            System.Diagnostics.Process process=null;
            try{process=System.Diagnostics.Process.Start(info);}catch(Exception e){view.ShowError(e.Message);}
            if(process==null)yield break;
            using(process){process.StandardInput.WriteLine("POSIX path of (choose file with prompt \"Choose a photo\" of type {\"public.image\"})");process.StandardInput.Close();var output=process.StandardOutput.ReadToEndAsync();var error=process.StandardError.ReadToEndAsync();while(!process.HasExited)yield return null;while(!output.IsCompleted||!error.IsCompleted)yield return null;if(process.ExitCode==0)LoadPath(output.Result.Trim());}
        }
#endif
        void OnDestroy(){if(ownedSource)Destroy(ownedSource);if(ownedResult)Destroy(ownedResult);}
        void LoadFallbackTexture()
        {
            pendingJpegBytes = fallbackTexture.EncodeToJPG(90);
            view.ShowSourceTexture(fallbackTexture);
        }

        void LoadFromBytes(byte[] fileBytes)
        {
            var tex = new Texture2D(2, 2);
            if (!tex.LoadImage(fileBytes))
            {
                Destroy(tex);view.ShowError(English?"Failed to load the image":"画像の読み込みに失敗しました");
                return;
            }
            if(ownedSource)Destroy(ownedSource);ownedSource=tex;
            pendingJpegBytes = tex.EncodeToJPG(90);
            view.ShowSourceTexture(tex);
        }

        void HandleConvertRequested()
        {
            if (pendingJpegBytes == null)
            {
                view.ShowError(English?"Please choose a photo first":"先に写真を選んでください");
                return;
            }
            view.SetBusy(true);BusyChanged?.Invoke(true);
            int request=++revision;
            apiClient.SendPhoto(pendingJpegBytes, response=>{if(this&&isActiveAndEnabled&&request==revision)HandleResponse(response);});
        }

        void HandleResponse(PhotoIllustrationResponsePayload response)
        {
            view.SetBusy(false);BusyChanged?.Invoke(false);
            if (response==null||!response.success)
            {
                view.ShowError(response?.error??"No response");
                return;
            }

            Texture2D tex=null;
            try{
                byte[] imageBytes = Convert.FromBase64String(response.illustration_base64);
                tex = new Texture2D(2, 2);
                if(!tex.LoadImage(imageBytes))throw new FormatException(English?"Couldn't load the generated image":"生成画像を読み込めませんでした");
                if(ownedResult)Destroy(ownedResult);ownedResult=tex;
                view.ShowResult(tex, response.impression);
            }catch(Exception e){if(tex&&tex!=ownedResult)Destroy(tex);view.ShowError(e.Message);}
        }
    }
}
