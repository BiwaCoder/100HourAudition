using UnityEngine;
using UnityEngine.InputSystem;

namespace HundredHour.UI.Choices
{
    /// <summary>任意の入力アダプター。外部InputActionを使う場合はこのコンポーネントを外す。</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(ChoiceMenuController))]
    [AddComponentMenu("UI/Choices/Choice Menu Keyboard Input")]
    public sealed class ChoiceMenuKeyboardInput : MonoBehaviour
    {
        ChoiceMenuController controller;
        void Awake() => controller = GetComponent<ChoiceMenuController>();
        void Update()
        {
            var k = Keyboard.current;
            if (k == null || !controller.CanInteract) return;
            if (k.upArrowKey.wasPressedThisFrame || k.wKey.wasPressedThisFrame) controller.Move(-1);
            else if (k.downArrowKey.wasPressedThisFrame || k.sKey.wasPressedThisFrame) controller.Move(1);
            for (int i = 0; i < 9; i++)
            {
                if (k[(Key)((int)Key.Digit1 + i)].wasPressedThisFrame || k[(Key)((int)Key.Numpad1 + i)].wasPressedThisFrame)
                {
                    controller.SelectAndConfirm(i);
                    return;
                }
            }
            if (k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame || k.spaceKey.wasPressedThisFrame)
                controller.Confirm();
        }
    }
}
