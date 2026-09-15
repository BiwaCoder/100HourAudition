using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
namespace HundredHour.RealityShow.Cinema.Editor
{
    public static class ShowSupportUpdate
    {
        public static void ApplyScenario(ShowScenario scenario)
        {
            foreach(var beat in scenario.beats)
            {
                bool support=beat.id=="awakening"||beat.id.StartsWith("route")||beat.id=="reward"||beat.id=="timeleap";
                if(!support)continue;beat.speaker=ShowSupportVoice.Label;
                if(beat.id=="awakening")beat.text=ShowSupportVoice.Awakening;
                else if(beat.id=="reward")beat.text=ShowSupportVoice.Reward;
                else if(beat.id=="timeleap")beat.text=ShowSupportVoice.Rewind;
                else beat.text=ShowSupportVoice.Route(int.Parse(beat.id.Substring(5)));
                if(beat.camera!=null&&beat.camera.title.Contains("アリア")){beat.camera.title=beat.camera.title.Replace("アリアとの同調","沙織のご挨拶").Replace("アリア","沙織");beat.camera.instruction=beat.camera.instruction.Replace("アリアとの同調","沙織のご挨拶").Replace("アリア","沙織");EditorUtility.SetDirty(beat.camera);}
            }
            EditorUtility.SetDirty(scenario);
        }
        public static string Apply()
        {
            if(EditorApplication.isPlaying)throw new System.InvalidOperationException("Stop Play first.");
            ApplyScenario(AssetDatabase.LoadAssetAtPath<ShowScenario>("Assets/RealityShow/Film/AuditionScenario.asset"));
            foreach(var path in new[]{"Assets/RealityShow/UI/RealityShowUI.prefab","Assets/RealityShow/Film/UI/AuditionBattleUI.prefab"})
            {var root=PrefabUtility.LoadPrefabContents(path);Relabel(root);PrefabUtility.SaveAsPrefabAsset(root,path);PrefabUtility.UnloadPrefabContents(root);}
            foreach(var v in Object.FindObjectsByType<ShowView>(FindObjectsInactive.Include,FindObjectsSortMode.None))Relabel(v.gameObject);
            foreach(var actor in Object.FindObjectsByType<HundredHour.Cutscenes.CutsceneActor>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(actor.actorId=="aria"){actor.displayName=ShowSupportVoice.Label;EditorUtility.SetDirty(actor);}
            AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());return "Saori voice, scenario, UI prefabs and current scene updated";
        }
        static void Relabel(GameObject root)
        {foreach(var t in root.GetComponentsInChildren<TMP_Text>(true))if(t.text=="ARIA / MEMORY SYSTEM"){t.text="沙織 / ご相談ノート";EditorUtility.SetDirty(t);if(PrefabUtility.IsPartOfPrefabInstance(t))PrefabUtility.RecordPrefabInstancePropertyModifications(t);}}
    }
}
