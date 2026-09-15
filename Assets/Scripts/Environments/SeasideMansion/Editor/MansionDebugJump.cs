using HundredHour.RealityShow;
using UnityEditor;
using UnityEngine;

namespace HundredHour.Environments.Editor
{
    /// <summary>繰り返しプレイのためのデバッグ用ジャンプ。Playモードで、すでにBeginEncounter()が
    /// 走っている(=最初の出会いを終えている)MansionSignalDirectorの章・フェーズを直接書き換える。
    /// ChooseRoute()以降の初期化はプレイヤーがRoute選択した時点で通常通り走るため、最小限の書き換えで済む。</summary>
    public static class MansionDebugJump
    {
        static MansionSignalDirector Find()
        {
            var director = Object.FindFirstObjectByType<MansionSignalDirector>();
            if (director == null) { Debug.LogWarning("MansionSignalDirectorが見つかりません。SeasideMansionAuditionシーンでPlay中か確認してください。"); return null; }
            if (director.Game == null) { Debug.LogWarning("まだ番組が開始されていません(最初の出会いを終えてから使ってください)。"); return null; }
            return director;
        }

        static void JumpToRoute(int chapter)
        {
            var director = Find(); if (director == null) return;
            var s = director.Game.State;
            s.chapter = chapter; s.turn = 0; s.phase = ShowPhase.Route;
            s.notice = $"デバッグ: 章{chapter}へジャンプしました。";
            director.Refresh();
            Debug.Log($"[MansionDebugJump] 章{chapter}のRoute選択へジャンプしました。");
        }

        [MenuItem("Tools/100Hour/Debug/Jump To Chapter 0 (自己紹介)")]
        static void JumpChapter0() => JumpToRoute(0);

        [MenuItem("Tools/100Hour/Debug/Jump To Chapter 1 (グループデート)")]
        static void JumpChapter1() => JumpToRoute(1);

        [MenuItem("Tools/100Hour/Debug/Jump To Chapter 2 (ツーショット)")]
        static void JumpChapter2() => JumpToRoute(2);

        [MenuItem("Tools/100Hour/Debug/Jump To Victory (エンディング)")]
        static void JumpVictory()
        {
            var director = Find(); if (director == null) return;
            var s = director.Game.State;
            s.phase = ShowPhase.Victory; s.epilogue = s.deckMechanics ? "「デバッグから見た、ふたりの結末。」\n(Jump To Victoryから直接遷移)" : s.epilogue;
            director.Refresh();
            Debug.Log("[MansionDebugJump] Victoryへジャンプしました。");
        }

        [MenuItem("Tools/100Hour/Debug/Jump To Chapter 0 (自己紹介)", true)]
        [MenuItem("Tools/100Hour/Debug/Jump To Chapter 1 (グループデート)", true)]
        [MenuItem("Tools/100Hour/Debug/Jump To Chapter 2 (ツーショット)", true)]
        [MenuItem("Tools/100Hour/Debug/Jump To Victory (エンディング)", true)]
        static bool ValidateOnlyDuringPlay() => Application.isPlaying;
    }
}
