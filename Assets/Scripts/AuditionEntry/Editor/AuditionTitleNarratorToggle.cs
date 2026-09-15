using UnityEditor;

namespace HundredHour.AuditionEntry.Editor
{
    // Kill switch for the title-screen voice tutorial (gpt-live-1). It costs real API time whenever
    // it runs, so it defaults on but can be switched off while iterating in the Editor.
    public static class AuditionTitleNarratorToggle
    {
        const string MenuPath="Tools/100 Hour/タイトル音声チュートリアル";
        const string Key="100Hour.TitleNarrator.Enabled";

        [MenuItem(MenuPath,false,100)]
        static void Toggle()
        {
            EditorPrefs.SetBool(Key,!EditorPrefs.GetBool(Key,true));
        }

        [MenuItem(MenuPath,true)]
        static bool ToggleValidate()
        {
            Menu.SetChecked(MenuPath,EditorPrefs.GetBool(Key,true));
            return true;
        }
    }
}
