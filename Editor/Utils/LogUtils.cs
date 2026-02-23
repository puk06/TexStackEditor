using System.Runtime.CompilerServices;
using UnityEngine;

namespace net.puk06.TexStackEditor.Editor.Utils
{
    internal static class LogUtils
    {
        private const string LOG_PREFIX = "<color=#0fd2d9>TexStackEditor</color> <color=#ffffff>>";

        /// <summary>
        /// 普通のログを表示する時に使います。
        /// </summary>
        /// <param name="message"></param>
        /// <param name="caller"></param>
        internal static void Log(string message, [CallerMemberName] string caller = "")
        {
            Debug.Log($"{LOG_PREFIX} <color=#0fd99c>{caller}</color> ></color> {message}");
        }

        /// <summary>
        /// 注意用のメッセージを表示する時に使います。
        /// </summary>
        /// <param name="message"></param>
        /// <param name="caller"></param>
        internal static void LogWarning(string message, [CallerMemberName] string caller = "")
        {
            Debug.LogWarning($"{LOG_PREFIX} <color=#0fd99c>{caller}</color> ></color> {message}");
        }

        /// <summary>
        /// エラーを表示する時に使います。
        /// </summary>
        /// <param name="message"></param>
        /// <param name="caller"></param>
        internal static void LogError(string message, [CallerMemberName] string caller = "")
        {
            Debug.LogError($"{LOG_PREFIX} <color=#0fd99c>{caller}</color> ></color> {message}");
        }
    }
}
