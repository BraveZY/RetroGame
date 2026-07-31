using System;
using System.Runtime.InteropServices;

namespace PoseAI
{
    /// <summary>
    /// macOS Core ML 姿态插件的最小托管边界。
    ///
    /// 职责：
    /// - 只负责会话、帧和原始 YOLO tensor 的互操作。
    /// - 不包含相机所有权、关键点语义或数据源生命周期规则。
    /// </summary>
    internal static class MacYoloPoseNative
    {
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        private const string LibraryName = "MacYoloPose";

        [DllImport(LibraryName)]
        internal static extern IntPtr MYLP_Create();

        [DllImport(LibraryName)]
        internal static extern void MYLP_Destroy(IntPtr session);

        [DllImport(LibraryName)]
        internal static extern int MYLP_SubmitRgba(IntPtr session, byte[] pixels, int width, int height, int rowStride, int mirror);

        [DllImport(LibraryName)]
        internal static extern int MYLP_IsBusy(IntPtr session);

        [DllImport(LibraryName)]
        internal static extern long MYLP_GetOutputVersion(IntPtr session);

        [DllImport(LibraryName)]
        internal static extern int MYLP_GetOutputCount(IntPtr session);

        [DllImport(LibraryName)]
        internal static extern int MYLP_CopyOutput(IntPtr session, float[] destination, int destinationCount);

        [DllImport(LibraryName)]
        private static extern IntPtr MYLP_GetLastError(IntPtr session);

        internal static string GetLastError(IntPtr session)
        {
            IntPtr message = MYLP_GetLastError(session);
            return message == IntPtr.Zero ? string.Empty : Marshal.PtrToStringAnsi(message);
        }
#endif
    }
}
