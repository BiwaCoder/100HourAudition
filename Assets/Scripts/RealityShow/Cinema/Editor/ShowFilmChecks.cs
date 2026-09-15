using System;
using System.Linq;
using UnityEngine;
using HundredHour.Cutscenes;
namespace HundredHour.RealityShow.Cinema.Editor
{
    public static class ShowFilmChecks
    {
        static void Check(bool ok,string name){if(!ok)throw new Exception("Film check: "+name);}
        public static string Run()
        {
            var c=UnityEditor.AssetDatabase.LoadAssetAtPath<ShowContent>("Assets/RealityShow/Data/ShowContent.asset");
            var scenario=UnityEditor.AssetDatabase.LoadAssetAtPath<ShowScenario>("Assets/RealityShow/Film/AuditionScenario.asset");
            foreach(var b in scenario.beats){CameraPlanValidation.Validate(b.camera.shots,b.actors);if(b.id.StartsWith("opening"))Check(b.text.Length==50,b.id+" has exactly 50 characters");}
            for(int choice=0;choice<3;choice++)
            {
                var g=new ShowGame(c);g.Begin(choice,ShowDifficulty.Easy);for(int n=0;n<3;n++)g.Advance();g.FirstChoice(choice);
                Check(g.State.phase==ShowPhase.FirstReview&&!g.State.Player.eliminated,"host review before loss");
                g.Advance();Check(g.State.openingPage==1,"analyst follows announcer");g.Advance();Check(g.State.phase==ShowPhase.FirstLoss&&g.State.Player.eliminated,"every first choice loses");
                g.Advance();Check(g.State.phase==ShowPhase.Awakening,"AGI after first loss");g.Advance();Check(g.State.phase==ShowPhase.Route&&!g.State.Player.eliminated,"revived for genuine battle");
                int trust=g.State.Player.trust,stars=g.State.Player.stars;g.ChooseRoute(0);g.ResolveTalk(0,ShowDialogue.Local(g,g.State.actions[0]));g.Remember("yuto","topic","リープを越えて覚えている約束","test");
                int memories=g.State.memories.Count;g.State.deck.Add("listen");int deck=g.State.deck.Count;int energy=g.State.agi;
                string p="Temp/FilmSaveCheck.json";ShowSaveStore.Save(g.State,p);Check(ShowSaveStore.TryLoad(c,out var saved,out _,p),"checkpoint saves");g=new ShowGame(c,saved);
                Check(g.TimeLeap(),"active leap succeeds");Check(g.State.phase==ShowPhase.Route&&g.State.turn==0&&g.State.Player.trust==trust&&g.State.Player.stars==stars,"chapter scores restored");
                Check(g.State.agi==energy-3&&g.State.deck.Count==deck&&g.State.memories.Count==memories&&g.State.timeLeaps==1,"knowledge and acquisition persist, energy spent");
                g.ChooseRoute(1);Check(!g.TimeLeap()&&g.State.agi>=0,"cannot repeat without energy");
                Check(UnityEngine.JsonUtility.FromJson<ShowState>(g.State.chapterCheckpoint).chapterCheckpoint=="","no recursive checkpoint expansion");
            }
            return "PASS: 3 × 50-character introductions; all 3 first choices lose after 2 host beats; awakening; chapter rewind; persistent memory/deck; energy cost; save/resume checkpoint; all shot actor bindings. "+HundredHour.RealityShow.Editor.ShowRulesChecks.Run();
        }
    }
}
