using System;
using System.Collections.Generic;
using UnityEngine;
using HundredHour.Cutscenes;
namespace HundredHour.RealityShow.Cinema
{
    [Serializable] public sealed class ShowFilmBeat
    {
        public string id,speaker;
        [TextArea(3,8)] public string text;
        public string[] actors;
        public CutsceneSequence camera;
    }
    [CreateAssetMenu(menuName="100Hour/Film Scenario")]
    public sealed class ShowScenario : ScriptableObject
    {
        public List<ShowFilmBeat> beats=new List<ShowFilmBeat>();
        public ShowFilmBeat Beat(ShowState s)=>beats.Find(b=>b.id==Key(s));
        public static string Key(ShowState s)
        {
            if(s.phase==ShowPhase.Opening)return "opening"+s.openingPage;
            if(s.phase==ShowPhase.FirstReview)return "review"+s.openingPage;
            if(s.phase==ShowPhase.Route)return s.timeLeaps>0&&s.notice.StartsWith("タイムリープ")?"timeleap":"route"+s.chapter;
            return s.phase.ToString().ToLowerInvariant();
        }
    }
}
