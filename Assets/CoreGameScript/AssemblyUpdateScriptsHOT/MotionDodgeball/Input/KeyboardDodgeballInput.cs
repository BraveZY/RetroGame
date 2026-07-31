using UnityEngine;

namespace MotionDodgeball.Gameplay
{
    /// <summary>
    /// 把共享键盘按键翻译成 C、2P、3P 可读取的动作请求。
    ///
    /// 职责：
    /// - 让 1P 用 A/D 只控制中间躲避者的横向移动。
    /// - 让 2P 用 J/L/I、3P 用方向键分别瞄准和出手。
    /// - 保持查询边界稳定，未来体感输入可替换此实现而不改回合规则。
    /// </summary>
    public static class KeyboardDodgeballInput
    {
        public static float GetDodgerAxis()
        {
            return GetSignedAxis(KeyCode.A, KeyCode.D);
        }

        public static float GetThrowerAxis(ThrowerControl control)
        {
            return control == ThrowerControl.PlayerTwo
                ? GetSignedAxis(KeyCode.J, KeyCode.L)
                : control == ThrowerControl.PlayerThree
                    ? GetSignedAxis(KeyCode.LeftArrow, KeyCode.RightArrow)
                    : 0f;
        }

        public static bool IsThrowRequested(ThrowerControl control)
        {
            return control == ThrowerControl.PlayerTwo && Input.GetKeyDown(KeyCode.I)
                || control == ThrowerControl.PlayerThree && Input.GetKeyDown(KeyCode.UpArrow);
        }

        private static float GetSignedAxis(KeyCode negative, KeyCode positive)
        {
            return (Input.GetKey(positive) ? 1f : 0f) - (Input.GetKey(negative) ? 1f : 0f);
        }
    }
}
