using System.IO;
using UnityEditor;
using UnityEngine;

namespace HundredHour.AuditionEntry.Editor
{
    // Debug helper: once a character is created (voice or photo), it's saved to disk and keeps
    // showing up on every future play. These two menu items let you back it up and clear it (so
    // you can see the app's default "no character yet" state), then restore it again afterward.
    public static class AuditionProfileDebugTools
    {
        static string Dir=>AuditionProfile.DirectoryPath;
        static string BackupDir=>Path.Combine(Dir,"DebugBackup");
        static readonly string[] Files={"participant.json","participant-male.json","portrait.png","portrait-male.png"};

        [MenuItem("Tools/100 Hour/デバッグ/キャラをデフォルトに戻す(バックアップ)",false,200)]
        static void ClearWithBackup()
        {
            if(!Directory.Exists(Dir)){EditorUtility.DisplayDialog("100 Hour","保存済みのキャラクターが見つかりません(すでにデフォルト状態です)。","OK");return;}
            if(!EditorUtility.DisplayDialog("キャラをデフォルトに戻す","現在保存されているキャラクター(JSON・写真)をバックアップしてから削除します。\n次回プレイでデフォルトの参加者を確認できます。よろしいですか？","戻す","キャンセル"))return;
            Directory.CreateDirectory(BackupDir);
            int moved=0;
            foreach(var name in Files)
            {
                string src=Path.Combine(Dir,name);
                if(!File.Exists(src))continue;
                File.Copy(src,Path.Combine(BackupDir,name),true);
                File.Delete(src);
                moved++;
            }
            Debug.Log(moved>0
                ?$"[100Hour] キャラクターファイルを{moved}件バックアップして削除しました。Tools/100 Hour/デバッグ/バックアップしたキャラを復元 でいつでも戻せます。"
                :"[100Hour] 削除対象のファイルがありませんでした(すでにデフォルト状態です)。");
        }

        [MenuItem("Tools/100 Hour/デバッグ/バックアップしたキャラを復元",false,201)]
        static void RestoreFromBackup()
        {
            if(!Directory.Exists(BackupDir)){EditorUtility.DisplayDialog("100 Hour","復元できるバックアップが見つかりません。","OK");return;}
            int restored=0;
            foreach(var name in Files)
            {
                string backup=Path.Combine(BackupDir,name);
                if(!File.Exists(backup))continue;
                Directory.CreateDirectory(Dir);
                File.Copy(backup,Path.Combine(Dir,name),true);
                restored++;
            }
            Debug.Log(restored>0?$"[100Hour] キャラクターファイルを{restored}件復元しました。":"[100Hour] 復元できるファイルがありませんでした。");
        }
    }
}
