using System;
using System.IO;
using System.Linq;
using UnityEngine;
using HundredHour.Environments;
using HundredHour.LiveInterview;
using HundredHour.PhotoIllustration;
using HundredHour.Localization;
namespace HundredHour.AuditionEntry
{
    [DefaultExecutionOrder(-500)]
    public sealed class AuditionSceneBridge : MonoBehaviour
    {
        public MansionSignalDirector game;
        public LiveInterviewController interview;
        public PhotoIllustrationDemoView photo;
        public AuditionPortal portal;
        AuditionTitleNarrator photoNarrator;
        PhotoIllustrationDemoController photoController;
        Sprite ownedPortrait;
        Material playerMaterial,heroineMaterial;
        void Awake()
        {
            if(game)
            {
                game.content=Instantiate(game.content);var originalPortrait=game.content.Person("himari").portrait;AuditionProfile.Apply(game.content);
                game.arrival.encounterCaption=new MansionArrivalDirector.Caption
                {
                    heading=AuditionPortal.L("FIRST CONTACT / 表情が見えた瞬間","FIRST CONTACT / A face comes into focus"),
                    text=AuditionProfile.IsMale
                        ? AuditionPortal.L("緊張でぼやけていた表情が、近づくとはっきり見えた。\n……きれいな人だ。笑うとこんなに可愛いんだ。思わず、目が離せなくなる。",
                            "As I draw closer, the face blurred by my nerves comes into focus.\nShe is beautiful—and that smile is so cute. I cannot help looking at her.")
                        : AuditionPortal.L("緊張でぼやけていた表情が、近づくとはっきり見えた。\n……整った顔立ちに、優しい微笑み。かっこいい人だ。目が合って、胸が高鳴る。",
                            "As I draw closer, the face blurred by my nerves comes into focus.\nSuch handsome features and a gentle smile. Our eyes meet, and my heart skips a beat.")
                };
                if(File.Exists(AuditionProfile.PortraitPath)&&game.content.Person("himari").portrait!=originalPortrait)ownedPortrait=game.content.Person("himari").portrait;
                game.testSavePath=Path.Combine(AuditionProfile.DirectoryPath,"game-"+(AuditionProfile.IsMale?"male-":"")+ProfileKey()+".json");
                Directory.CreateDirectory(AuditionProfile.DirectoryPath);
                if(AuditionProfile.IsMale)
                {
                    var renderer=game.arrival.controls.player.GetComponent<Renderer>();
                    playerMaterial=new Material(renderer.sharedMaterial){name="Yuma Black Cube"};
                    playerMaterial.color=new Color(.015f,.015f,.015f,1);
                    playerMaterial.SetColor("_EmissionColor",Color.black);playerMaterial.DisableKeyword("_EMISSION");
                    renderer.sharedMaterial=playerMaterial;
                    game.content.Person("himari").accent=playerMaterial.color;
                    // The heroine's cube is white; rival cubes take each character's key colour (MansionConversationStage).
                    var heroine=game.arrival.counterpart.GetComponentInChildren<Renderer>();
                    heroineMaterial=new Material(heroine.sharedMaterial){name="Heroine White Cube"};
                    heroineMaterial.color=Color.white;heroineMaterial.SetColor("_EmissionColor",Color.black);heroineMaterial.DisableKeyword("_EMISSION");
                    heroine.sharedMaterial=heroineMaterial;
                }
            }
        }
        string ProfileKey(){using(var sha=System.Security.Cryptography.SHA256.Create())return BitConverter.ToString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(AuditionProfile.Read()?.ToString()??"default"))).Replace("-","").Substring(0,16);}
        TMPro.TMP_Text[] labels;
        void Start()
        {
            if(photo&&portal)
            {
                photoNarrator=portal.gameObject.AddComponent<AuditionTitleNarrator>();
                photoNarrator.Initialize(portal,"portrait");
                photoController=photo.GetComponent<PhotoIllustrationDemoController>();
                if(!photoController)photoController=FindFirstObjectByType<PhotoIllustrationDemoController>();
                if(photoController)photoController.BusyChanged+=OnPhotoBusy;
            }
            if(interview&&interview.view&&portal)
            {
                // One footer slot: "中止する" while the interview runs, "← メニューへ" once it is over.
                var cancel=(RectTransform)interview.view.cancelButton.transform;
                portal.PlaceReturn(cancel.anchorMin,cancel.anchorMax);
                SyncInterviewFooter();
            }
            if(!game)return;
            // Wait until the portal has wired its button in Awake, before hiding it.
            if(portal&&portal.continueLabel)
                portal.continueLabel.GetComponentInParent<UnityEngine.UI.Button>(true).gameObject.SetActive(false);
            labels=game.arrival.view.GetComponentsInChildren<TMPro.TMP_Text>(true).Concat(game.view.GetComponentsInChildren<TMPro.TMP_Text>(true)).ToArray();
        }
        void SyncInterviewFooter()
        {
            var v=interview.view;
            // JSON is saved to the profile automatically here, so the demo's copy button would only fight the menu button for the slot.
            if(v.copyButton&&v.copyButton.gameObject.activeSelf)v.copyButton.gameObject.SetActive(false);
            portal.SetReturnVisible(!v.cancelButton.gameObject.activeSelf);
        }
        void LateUpdate()
        {
            if(interview&&interview.view&&portal)SyncInterviewFooter();
            if(!game||!game.content.customParticipant)return;
            string name=game.content.Person("himari").displayName;
            if(labels!=null)foreach(var text in labels)if(text){ReplaceName(text,name);text.text=game.content.AdaptText(text.text);}
        }
        static void ReplaceName(TMPro.TMP_Text text,string name)
        {
            if(string.IsNullOrEmpty(name)||string.IsNullOrEmpty(text.text)||text.text.Contains(name))return;
            if(text.text.Contains("ひまり")||text.text.Contains("Himari")||text.text.Contains("HIMARI"))text.text=text.text.Replace("ひまり",name).Replace("Himari",name).Replace("HIMARI",name);
        }
        void OnPhotoBusy(bool busy){if(busy&&photoNarrator)photoNarrator.PhotoCue("photo_busy");}
        void OnDestroy(){if(photoController)photoController.BusyChanged-=OnPhotoBusy;if(playerMaterial)Destroy(playerMaterial);if(heroineMaterial)Destroy(heroineMaterial);if(ownedPortrait){Destroy(ownedPortrait.texture);Destroy(ownedPortrait);}if(game&&game.content)Destroy(game.content);}
        void OnEnable(){if(interview)interview.CharacterCreated+=SaveJson;if(photo)photo.ResultCreated+=SavePortrait;}
        void OnDisable(){if(interview)interview.CharacterCreated-=SaveJson;if(photo)photo.ResultCreated-=SavePortrait;}
        void SaveJson(string json){try{AuditionProfile.SaveJson(json);portal.SetNotice(AuditionPortal.L("人物像を保存しました。メニューへ戻って参加できます。","Profile saved. Return to the menu to enter."));}catch(Exception e){portal.SetNotice(e.Message);}}
        void SavePortrait(Texture2D image){try{AuditionProfile.SavePortrait(image);if(photoNarrator)photoNarrator.PhotoCue("photo_done");portal.SetNotice(AuditionPortal.L("この姿を参加者に保存しました。","Portrait saved for your participant."));}catch(Exception e){portal.SetNotice(e.Message);}}
    }
}
