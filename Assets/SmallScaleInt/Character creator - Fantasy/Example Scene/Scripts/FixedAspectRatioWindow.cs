#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;

namespace SmallScaleInc.CharacterCreatorFantasy
{
    /// <summary>
    /// Keeps manual Windows player resizing at 16:9 without affecting maximize.
    /// </summary>
    internal sealed class FixedAspectRatioWindow : MonoBehaviour
    {
        private delegate IntPtr SubclassProcedure(
            IntPtr window,
            uint message,
            UIntPtr wParam,
            IntPtr lParam,
            UIntPtr subclassId,
            UIntPtr referenceData);

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        private IntPtr windowHandle;
        private SubclassProcedure subclassProcedure;
        private GCHandle selfHandle;
        private UIntPtr subclassId;
        private bool isInstalled;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateController()
        {
            GameObject controller = new GameObject("Fixed 16x9 Window");
            DontDestroyOnLoad(controller);
            controller.AddComponent<FixedAspectRatioWindow>();
        }

        private IEnumerator Start()
        {
            // The native player window can take a few frames to become available.
            for (int attempt = 0; attempt < 120 && windowHandle == IntPtr.Zero; attempt++)
            {
                windowHandle = GetActiveWindow();
                if (windowHandle == IntPtr.Zero)
                {
                    using (Process process = Process.GetCurrentProcess())
                        windowHandle = process.MainWindowHandle;
                }

                if (windowHandle == IntPtr.Zero)
                    yield return null;
            }

            if (windowHandle == IntPtr.Zero)
            {
                UnityEngine.Debug.LogWarning("Could not find the Windows player window; 16:9 resize locking is unavailable.");
                yield break;
            }

            subclassProcedure = HandleWindowMessage;
            selfHandle = GCHandle.Alloc(this);
            subclassId = new UIntPtr(1);
            UIntPtr referenceData = new UIntPtr(unchecked((ulong)GCHandle.ToIntPtr(selfHandle).ToInt64()));

            isInstalled = SetWindowSubclass(
                windowHandle,
                subclassProcedure,
                subclassId,
                referenceData);

            if (!isInstalled)
            {
                selfHandle.Free();
                UnityEngine.Debug.LogWarning("Windows rejected the 16:9 resize constraint.");
            }
        }

        private void OnDestroy()
        {
            if (isInstalled && windowHandle != IntPtr.Zero)
                RemoveWindowSubclass(windowHandle, subclassProcedure, subclassId);

            if (selfHandle.IsAllocated)
                selfHandle.Free();

            isInstalled = false;
        }

        [MonoPInvokeCallback(typeof(SubclassProcedure))]
        private static IntPtr HandleWindowMessage(
            IntPtr window,
            uint message,
            UIntPtr wParam,
            IntPtr lParam,
            UIntPtr subclassId,
            UIntPtr referenceData)
        {
            const uint sizingMessage = 0x0214;

            if (message == sizingMessage && lParam != IntPtr.Zero)
            {
                GCHandle handle = GCHandle.FromIntPtr(new IntPtr(unchecked((long)referenceData.ToUInt64())));
                if (handle.Target is FixedAspectRatioWindow controller)
                    controller.ConstrainResize(wParam.ToUInt32(), lParam);
            }

            return DefSubclassProc(window, message, wParam, lParam);
        }

        private void ConstrainResize(uint resizeEdge, IntPtr rectanglePointer)
        {
            const double targetAspect = 16.0 / 9.0;
            const uint leftEdge = 1;
            const uint rightEdge = 2;
            const uint topEdge = 3;
            const uint topLeftCorner = 4;
            const uint topRightCorner = 5;
            const uint bottomLeftCorner = 7;
            const uint bottomRightCorner = 8;

            NativeRect proposed = Marshal.PtrToStructure<NativeRect>(rectanglePointer);
            if (!GetWindowRect(windowHandle, out NativeRect currentWindow) ||
                !GetClientRect(windowHandle, out NativeRect currentClient))
                return;

            int chromeWidth = (currentWindow.Right - currentWindow.Left) -
                              (currentClient.Right - currentClient.Left);
            int chromeHeight = (currentWindow.Bottom - currentWindow.Top) -
                               (currentClient.Bottom - currentClient.Top);

            int proposedClientWidth = Math.Max(1, proposed.Right - proposed.Left - chromeWidth);
            int proposedClientHeight = Math.Max(1, proposed.Bottom - proposed.Top - chromeHeight);
            int currentClientWidth = Math.Max(1, currentClient.Right - currentClient.Left);
            int currentClientHeight = Math.Max(1, currentClient.Bottom - currentClient.Top);

            bool isLeft = resizeEdge == leftEdge || resizeEdge == topLeftCorner || resizeEdge == bottomLeftCorner;
            bool isTop = resizeEdge == topEdge || resizeEdge == topLeftCorner || resizeEdge == topRightCorner;
            bool isCorner = resizeEdge == topLeftCorner || resizeEdge == topRightCorner ||
                            resizeEdge == bottomLeftCorner || resizeEdge == bottomRightCorner;

            double widthChange = Math.Abs(proposedClientWidth - currentClientWidth) / (double)currentClientWidth;
            double heightChange = Math.Abs(proposedClientHeight - currentClientHeight) / (double)currentClientHeight;
            bool deriveHeight = !isCorner
                ? resizeEdge == leftEdge || resizeEdge == rightEdge
                : widthChange >= heightChange;

            if (deriveHeight)
                proposedClientHeight = Math.Max(1, (int)Math.Round(proposedClientWidth / targetAspect));
            else
                proposedClientWidth = Math.Max(1, (int)Math.Round(proposedClientHeight * targetAspect));

            int outerWidth = proposedClientWidth + chromeWidth;
            int outerHeight = proposedClientHeight + chromeHeight;

            if (isLeft)
                proposed.Left = proposed.Right - outerWidth;
            else
                proposed.Right = proposed.Left + outerWidth;

            if (isTop)
                proposed.Top = proposed.Bottom - outerHeight;
            else
                proposed.Bottom = proposed.Top + outerHeight;

            Marshal.StructureToPtr(proposed, rectanglePointer, false);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr window, out NativeRect rectangle);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetClientRect(IntPtr window, out NativeRect rectangle);

        [DllImport("comctl32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowSubclass(
            IntPtr window,
            SubclassProcedure callback,
            UIntPtr subclassId,
            UIntPtr referenceData);

        [DllImport("comctl32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool RemoveWindowSubclass(
            IntPtr window,
            SubclassProcedure callback,
            UIntPtr subclassId);

        [DllImport("comctl32.dll")]
        private static extern IntPtr DefSubclassProc(
            IntPtr window,
            uint message,
            UIntPtr wParam,
            IntPtr lParam);
    }
}
#endif
