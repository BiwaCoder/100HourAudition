#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.InputSystem;

namespace HundredHour.DebugTools
{
    /// <summary>デバッグ用の早送り。Ctrl+1〜9でTime.timeScaleを1〜9倍に切り替える。
    /// 素の数字キー(Digit/Numpad 1-9)は会話・メニュー選択(ChoiceMenuKeyboardInput等)が
    /// すでに使っているため、競合を避けてCtrl修飾つきにしている。
    /// シーンに置く必要はなく、起動時に自動生成される(DontDestroyOnLoad)。
    /// UNITY_EDITORでのみコンパイルされ、本番ビルドには一切含まれない。</summary>
    public sealed class DebugTimeScaleHotkeys : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            var go = new GameObject("[Debug] TimeScale Hotkeys (Ctrl+1-9)");
            go.AddComponent<DebugTimeScaleHotkeys>();
            DontDestroyOnLoad(go);
        }

        void Update()
        {
            var k = Keyboard.current;
            if (k == null) return;
            if (!(k.leftCtrlKey.isPressed || k.rightCtrlKey.isPressed)) return;
            for (int i = 0; i < 9; i++)
            {
                if (k[(Key)((int)Key.Digit1 + i)].wasPressedThisFrame || k[(Key)((int)Key.Numpad1 + i)].wasPressedThisFrame)
                {
                    Time.timeScale = i + 1;
                    Debug.Log($"[DebugTimeScale] timeScale = {Time.timeScale}x (Ctrl+{i + 1})");
                    return;
                }
            }
        }
    }
}
#endif
