using System;
using HundredHour.Localization;
using System.Collections;
using HundredHour.Cutscenes;
using UnityEngine;
namespace HundredHour.Environments
{
    public sealed class MansionArrivalDirector : MonoBehaviour
    {
        public enum ArrivalPhase { Intro, Walking, Encounter, Exploration }
        [Serializable] public sealed class Caption
        {
            public string heading;
            [TextArea(2,4)] public string text;
        }
        public MansionSceneTour controls;
        public CutscenePlayer cinema;
        public MansionArrivalView view;
        [NonSerialized] public MansionSignalDirector director;
        public CutsceneSequence introduction;
        public CutsceneSequence firstEncounter;
        public Transform counterpart;
        public Caption[] openingCaptions;
        public Caption encounterCaption;
        [Min(.5f)] public float encounterDistance=3;
        public ArrivalPhase Phase { get; private set; }
        public bool EncounterSeen { get; private set; }
        [Min(.1f)] public float textFadeSeconds=.7f;
        float textElapsed;
        bool ready;
        int shownShot=-1;
        void OnEnable()=>cinema.Finished+=OnFilmFinished;
        IEnumerator Start()
        {
            // Let scene components initialize before taking ownership of the camera.
            view.PrepareOpening();
            controls.enabled=false;
            controls.SelectView(0);
            yield return LanguageStartMenu.WaitForChoice(view.navigationHint.font);
            yield return null;
            if(director)yield return ShowPrologue();
            director?.Music?.PlayMain();
            ready=true;Phase=ArrivalPhase.Intro;
            StartFilm(introduction);
        }
        IEnumerator ShowPrologue()
        {
            float waited=0;
            while(!director.PrologueReady&&waited<6f){waited+=Time.deltaTime;yield return null;}
            string hint=GameLanguage.Current==GameLocale.English?"(tap or press any key to continue)":"(タップ、または何かキーを押して進む)";
            view.ShowCaption(director.PrologueHeading,director.PrologueBody+"\n\n"+hint,1);
            float shown=0;
            while(shown<1.4f){shown+=Time.deltaTime;yield return null;}
            while(shown<8f)
            {
                if(UnityEngine.InputSystem.Keyboard.current!=null&&UnityEngine.InputSystem.Keyboard.current.anyKey.wasPressedThisFrame)break;
                if(UnityEngine.InputSystem.Mouse.current!=null&&UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)break;
                shown+=Time.deltaTime;yield return null;
            }
            view.caption.gameObject.SetActive(false);
        }
        void StartFilm(CutsceneSequence sequence)
        {
            controls.SelectView(0);
            controls.brain.enabled=true;
            shownShot=-1;
            textElapsed=0;
            cinema.Play(sequence);
            view.ReleaseBlack();
        }
        void Update()
        {
            if(!ready)return;
            if(cinema.IsPlaying)
            {
                if(Phase==ArrivalPhase.Intro && shownShot!=cinema.CurrentShot && (cinema.CurrentShot==0 || cinema.IsShotActive))
                {
                    shownShot=cinema.CurrentShot;
                    textElapsed=0;
                    int captionIndex=shownShot-1;
                    if(captionIndex>=0 && captionIndex<openingCaptions.Length)
                        view.ShowCaption(openingCaptions[captionIndex].heading,openingCaptions[captionIndex].text,0);
                }
                textElapsed+=Time.deltaTime;
                float visibility=(1-cinema.FadeAlpha)*Mathf.SmoothStep(0,1,textElapsed/textFadeSeconds);
                float titleExit=1-Mathf.SmoothStep(0,1,(cinema.ShotProgress-.75f)/.25f);
                view.SetTitleAlpha(Phase==ArrivalPhase.Intro && cinema.CurrentShot==0?visibility*titleExit:0);
                view.SetCaptionAlpha(visibility);
                return;
            }
            if(Phase!=ArrivalPhase.Walking||EncounterSeen)return;
            Vector3 delta=controls.player.position-counterpart.position;
            // Distance and floor height prevent an encounter through a ceiling.
            if(Mathf.Abs(delta.y)<1.5f && Vector3.ProjectOnPlane(delta,Vector3.up).magnitude<=encounterDistance)
            {
                EncounterSeen=true;Phase=ArrivalPhase.Encounter;
                view.ShowCaption(encounterCaption.heading,encounterCaption.text,0);
                StartFilm(firstEncounter);
            }
        }
        void OnFilmFinished()
        {
            if(!ready)return;
            Phase=Phase==ArrivalPhase.Intro?ArrivalPhase.Walking:ArrivalPhase.Exploration;
            controls.SelectView(3);
            view.ShowNavigation(Phase==ArrivalPhase.Walking);
        }
        void OnDisable()
        {
            ready=false;
            if(cinema!=null){cinema.Finished-=OnFilmFinished;cinema.Stop();}
            if(Application.isPlaying && controls!=null && controls.player!=null && controls.mover!=null && controls.joystickCanvas!=null && controls.followCamera!=null && controls.brain!=null)
                controls.SelectView(3);
            if(view!=null)view.ShowNavigation(false);
        }
    }
}
