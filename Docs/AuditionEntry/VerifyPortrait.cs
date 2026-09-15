var view=UnityEngine.Object.FindFirstObjectByType<HundredHour.PhotoIllustration.PhotoIllustrationDemoView>();
if(System.IO.File.Exists(HundredHour.AuditionEntry.AuditionProfile.PortraitPath))throw new System.Exception("Do not overwrite existing portrait");

// Synthetic non-square image tests the display without using the network.
var texture=new UnityEngine.Texture2D(300,500);var pixels=new UnityEngine.Color[150000];for(int i=0;i<pixels.Length;i++)pixels[i]=new UnityEngine.Color(.12f+(i%300)/600f,.3f+(i/300)/1200f,.55f);texture.SetPixels(pixels);texture.Apply();
view.ShowSourceTexture(texture);view.ShowResult(texture,"縦横比検証用 / aspect test");
UnityEngine.Canvas.ForceUpdateCanvases();
foreach(var image in UnityEngine.Object.FindObjectsByType<UnityEngine.UI.RawImage>(UnityEngine.FindObjectsSortMode.None))
{
 if(image.texture==texture&&System.Math.Abs(image.rectTransform.rect.width/image.rectTransform.rect.height-.6f)>.01f)throw new System.Exception("Image stretched");
}
if(!System.IO.File.Exists(HundredHour.AuditionEntry.AuditionProfile.PortraitPath))throw new System.Exception("Portrait not saved");
System.IO.File.AppendAllText("Docs/AuditionEntry/verification.txt","PASS portrait result event saved PNG; source/result displayed at 3:5\n");
UnityEngine.ScreenCapture.CaptureScreenshot("Docs/AuditionEntry/portrait.png");
return "PASS";
