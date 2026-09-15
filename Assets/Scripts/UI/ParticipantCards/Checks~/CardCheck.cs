if(!UnityEngine.Application.isPlaying) throw new System.Exception("Run ParticipantCardsDemo in Play Mode first.");
var cards=UnityEngine.Object.FindObjectsByType<HundredHour.UI.Participants.ParticipantCardController>(UnityEngine.FindObjectsSortMode.None);
var a=cards.First(c=>c.Settings.number==1);
var b=cards.First(c=>c.Settings.number==2);
var c=cards.First(c=>c.Settings.number==7);
var av=a.GetComponent<HundredHour.UI.Participants.ParticipantCardView>();
var bv=b.GetComponent<HundredHour.UI.Participants.ParticipantCardView>();
var cv=c.GetComponent<HundredHour.UI.Participants.ParticipantCardView>();
var original=a.Settings.portrait;
var texture=new UnityEngine.Texture2D(128,64);
var sprite=UnityEngine.Sprite.Create(texture,new UnityEngine.Rect(0,0,128,64),new UnityEngine.Vector2(.5f,.5f));
try
{
    c.SetEliminated(true); UnityEngine.Canvas.ForceUpdateCanvases();
    if(!cv.OutVisible || cv.Portrait.materialForRendering.GetFloat("_Grayscale")!=1) throw new System.Exception("Elimination display failed");
    if(bv.Portrait.materialForRendering.GetFloat("_Grayscale")!=0 || av.Portrait.material==cv.Portrait.material) throw new System.Exception("Material isolation failed");
    c.SetEliminated(false); UnityEngine.Canvas.ForceUpdateCanvases();
    if(cv.OutVisible || cv.Portrait.materialForRendering.GetFloat("_Grayscale")!=0) throw new System.Exception("Restore failed");
    a.SetPortrait(sprite); UnityEngine.Canvas.ForceUpdateCanvases();
    if(av.Portrait.sprite!=sprite || av.Portrait.GetComponent<UnityEngine.UI.AspectRatioFitter>().aspectRatio!=2f) throw new System.Exception("Portrait replacement/ratio failed");
    a.SetPortrait(null);
    if(av.Portrait.sprite==null) throw new System.Exception("Fallback missing");
    a.SetStars(-10,2);
    if(a.Settings.stars!=0 || a.Settings.support!=1) throw new System.Exception("Bounds failed");
    a.SetStars(1234567,.42f);
    var texts=a.GetComponentsInChildren<TMPro.TMP_Text>().Select(t=>t.text).ToArray();
    if(!texts.Contains("1.2M STARS") || !texts.Contains("42%")) throw new System.Exception("Counts failed");
    var shader=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Shader>("Assets/UI/ParticipantCards/Shaders/PortraitGrayscale.shader");
    if(UnityEditor.ShaderUtil.ShaderHasError(shader)) throw new System.Exception("Shader compilation failed");
    return "PASS: OUT + grayscale, restore, per-card material isolation, image replacement/ratio, null fallback, count bounds/format, shader compilation";
}
finally
{
    a.SetPortrait(original);
    UnityEngine.Object.FindFirstObjectByType<HundredHour.UI.Participants.Demo.ParticipantCardsDemo>().ResetCards();
    UnityEngine.Object.Destroy(sprite);UnityEngine.Object.Destroy(texture);
}
