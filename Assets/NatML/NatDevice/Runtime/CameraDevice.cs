/* 
*   NatDevice
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatSuite.Devices {

    using AOT;
    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using Internal;
    using DeviceFlags = Internal.NatDevice.DeviceFlags;

    using UnityEngine;
    #if UNITY_EDITOR
    using UnityEditor;
    #endif
    
    /// <summary>
    /// Hardware camera device.
    /// </summary>
    public sealed class CameraDevice : IMediaDevice {

        #region --Types--
        /// <summary>
        /// Exposure mode.
        /// </summary>
        public enum ExposureMode : int {
            /// <summary>
            /// Continuous auto exposure.
            /// </summary>
            Continuous  = 0,
            /// <summary>
            /// Locked auto exposure.
            /// </summary>
            Locked      = 1,
            /// <summary>
            /// Manual exposure.
            /// </summary>
            Manual      = 2
        }

        /// <summary>
        /// Focus mode.
        /// </summary>
        public enum FocusMode : int {
            /// <summary>
            /// Continuous autofocus.
            /// </summary>
            Continuous  = 0,
            /// <summary>
            /// Locked focus.
            /// </summary>
            Locked      = 1,
        }

        /// <summary>
        /// Photo flash mode.
        /// </summary>
        public enum FlashMode : int {
            /// <summary>
            /// Never use flash.
            /// </summary>
            Off     = 0,
            /// <summary>
            /// Always use flash.
            /// </summary>
            On      = 1,
            /// <summary>
            /// Let the sensor detect if it needs flash.
            /// </summary>
            Auto    = 2
        }
        #endregion


        #region --Properties--
        /// <summary>
        /// Device unique ID.
        /// </summary>
        public string uniqueID {
            get {
                var result = new StringBuilder(1024);
                device.UniqueID(result);
                return result.ToString();
            }
        }

        /// <summary>
        /// Display friendly device name.
        /// </summary>
        public string name {
            get {
                var result = new StringBuilder(1024);
                device.Name(result);
                return result.ToString();
            }
        }

        /// <summary>
        /// Device location.
        /// </summary>
        public DeviceLocation location => (DeviceLocation)((int)device.Flags() & 0x3);

        /// <summary>
        /// Device is the default device for its media type.
        /// </summary>
        public bool defaultForMediaType => device.Flags().HasFlag(DeviceFlags.Default);

        /// <summary>
        /// Is this camera front facing?
        /// </summary>
        public bool frontFacing => device.Flags().HasFlag(DeviceFlags.FrontFacing);

        /// <summary>
        /// Is flash supported for photo capture?
        /// </summary>
        public bool flashSupported => device.Flags().HasFlag(DeviceFlags.Flash);

        /// <summary>
        /// Is torch supported?
        /// </summary>
        public bool torchSupported => device.Flags().HasFlag(DeviceFlags.Torch);
        
        /// <summary>
        /// Is setting the exposure point supported?
        /// </summary>
        public bool exposurePointSupported => device.Flags().HasFlag(DeviceFlags.ExposurePoint);

        /// <summary>
        /// Is setting the focus point supported?
        /// </summary>
        public bool focusPointSupported => device.Flags().HasFlag(DeviceFlags.FocusPoint);

        /// <summary>
        /// Is white balance lock supported?
        /// </summary>
        public bool whiteBalanceLockSupported => device.Flags().HasFlag(DeviceFlags.WhiteBalanceLock);

        /// <summary>
        /// Field of view in degrees.
        /// </summary>
        public (float width, float height) fieldOfView {
            get {
                device.FieldOfView(out var width, out var height);
                return (width, height);
            }
        }

        /// <summary>
        /// Exposure bias range in EV.
        /// </summary>
        public (float min, float max) exposureBiasRange {
            get {
                device.ExposureBiasRange(out var min, out var max);
                return (min, max);
            }
        }

        /// <summary>
        /// Exposure duration range in seconds.
        /// </summary>
        public (float min, float max) exposureDurationRange {
            get {
                device.ExposureDurationRange(out var min, out var max);
                return (min, max);
            }
        }

        /// <summary>
        /// Sensor sensitivity range.
        /// </summary>
        public (float min, float max) ISORange {
            get {
                device.ISORange(out var min, out var max);
                return (min, max);
            }
        }

        /// <summary>
        /// Zoom ratio range.
        /// </summary>
        public (float min, float max) zoomRange {
            get {
                device.ZoomRange(out var min, out var max);
                return (min, max);
            }
        }

        /// <summary>
        /// Get or set the preview resolution.
        /// </summary>
        public (int width, int height) previewResolution {
            get { device.PreviewResolution(out var width, out var height); return (width, height); }
            set => device.SetPreviewResolution(value.width, value.height);
        }

        /// <summary>
        /// Get or set the photo resolution.
        /// </summary>
        public (int width, int height) photoResolution {
            get { device.PhotoResolution(out var width, out var height); return (width, height); }
            set => device.SetPhotoResolution(value.width, value.height);
        }

        /// <summary>
        /// Get or set the preview framerate.
        /// </summary>
        public int frameRate {
            get => device.FrameRate();
            set => device.SetFrameRate(value);
        }

        /// <summary>
        /// Get or set the exposure mode.
        /// If the requested exposure mode is not supported, the camera device will ignore.
        /// </summary>
        public ExposureMode exposureMode {
            get => device.ExposureMode();
            set => device.SetExposureMode(value);
        }

        /// <summary>
        /// Get or set the exposure bias.
        /// This value must be in the range returned by `exposureRange`.
        /// </summary>
        public float exposureBias {
            get => device.ExposureBias();
            set => device.SetExposureBias(value);
        }

        /// <summary>
        /// Get or set the focus mode.
        /// </summary>
        public FocusMode focusMode {
            get => device.FocusLock() ? FocusMode.Locked : FocusMode.Continuous;
            set => device.SetFocusLock(value == FocusMode.Locked);
        }

        /// <summary>
        /// Get or set the photo flash mode.
        /// </summary>
        public FlashMode flashMode {
            get => device.FlashMode();
            set => device.SetFlashMode(value);
        }

        /// <summary>
        /// Get or set the torch mode.
        /// </summary>
        public bool torchEnabled {
            get => device.TorchEnabled();
            set => device.SetTorchEnabled(value);
        }

        /// <summary>
        /// Get or set the white balance lock.
        /// </summary>
        public bool whiteBalanceLock {
            get => device.WhiteBalanceLock();
            set => device.SetWhiteBalanceLock(value);
        }

        /// <summary>
        /// Get or set the zoom ratio.
        /// This value must be in the range returned by `zoomRange`.
        /// </summary>
        public float zoomRatio {
            get => device.ZoomRatio();
            set => device.SetZoomRatio(value);
        }
        #endregion


        #region --Events--
        /// <summary>
        /// Event raised when the camera device is disconnected.
        /// </summary>
        public event Action onDisconnected;
        #endregion


        #region --Controls--
        /// <summary>
        /// Check if a given exposure mode is supported by the camera device.
        /// </summary>
        public bool ExposureModeSupported (ExposureMode exposureMode) {
            switch (exposureMode) {
                case ExposureMode.Continuous:   return true; // always
                case ExposureMode.Locked:       return device.Flags().HasFlag(DeviceFlags.LockedExposure);
                case ExposureMode.Manual:       return device.Flags().HasFlag(DeviceFlags.ManualExposure);
                default:                        return false;
            }
        }

        /// <summary>
        /// Check if a given focus mode is supported by the camera device.
        /// </summary>
        public bool FocusModeSupported (FocusMode focusMode) {
            switch (focusMode) {
                case FocusMode.Continuous:  return true; // always
                case FocusMode.Locked:      return device.Flags().HasFlag(DeviceFlags.FocusLock);
                default:                    return false;
            }
        }

        /// <summary>
        /// Set manual exposure.
        /// </summary>
        /// <param name="duration">Exposure duration in seconds. MUST be in `exposureDurationRange`.</param>
        /// <param name="ISO">Sensor sensitivity ISO value. MUST be in `ISORange`.</param>
        public void SetExposureDuration (float duration, float ISO) => device.SetExposureDuration(duration, ISO);

        /// <summary>
        /// Set the exposure point of interest.
        /// The point is specified in normalized coordinates in range [0.0, 1.0].
        /// </summary>
        /// <param name="x">Normalized x coordinate.</param>
        /// <param name="y">Normalized y coordinate.</param>
        public void SetExposurePoint (float x, float y) => device.SetExposurePoint(x, y);

        /// <summary>
        /// Set the focus point of interest.
        /// The point is specified in normalized coordinates in range [0.0, 1.0].
        /// </summary>
        /// <param name="x">Normalized x coordinate.</param>
        /// <param name="y">Normalized y coordinate.</param>
        public void SetFocusPoint (float x, float y) => device.SetFocusPoint(x, y);
        #endregion


        #region --Streaming--
        /// <summary>
        /// Is the device running?
        /// </summary>
        public bool running => device.Running();

        /// <summary>
        /// Start running.
        /// </summary>
        /// <param name="handler">Delegate to receive preview frames.</param>
        public void StartRunning (Action<CameraImage> handler) {
            var syncContext = SynchronizationContext.Current; // send to main thread if available
            Action<IntPtr> wrapper = sampleBuffer => {
                var cameraImage = new CameraImage(this, sampleBuffer);
                SendOrPostCallback callback = image => handler?.Invoke(image as CameraImage);
                if (syncContext != null)
                    syncContext.Send(callback, cameraImage);
                else
                    callback.Invoke(cameraImage);
            };
            previewHandle = GCHandle.Alloc(wrapper, GCHandleType.Normal);
            device.StartRunning(OnCameraImage, (IntPtr)previewHandle);
            #if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            #endif
        }

        /// <summary>
        /// Stop running.
        /// </summary>
        public void StopRunning () {
            #if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            #endif
            device.StopRunning();
            previewHandle.Free();
            previewHandle = default;
        }

        /// <summary>
        /// Capture a photo.
        /// </summary>
        /// <param name="handler">Delegate to receive high-resolution photo.</param>
        public void CapturePhoto (Action<CameraImage> handler) {
            // Check
            if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
                throw new NotImplementedException(@"Photo capture is currently not implemented on Windows");
            // Capture
            var syncContext = SynchronizationContext.Current; // send to main thread if available
            Action<IntPtr> wrapper = sampleBuffer => {
                var cameraImage = new CameraImage(this, sampleBuffer);
                SendOrPostCallback callback = image => handler?.Invoke((CameraImage)image);
                if (syncContext != null)
                    syncContext.Send(callback, cameraImage);
                else
                    callback.Invoke(cameraImage);
            };
            var handle = GCHandle.Alloc(wrapper, GCHandleType.Normal);
            device.CapturePhoto(OnCameraPhoto, (IntPtr)handle);
        }
        #endregion


        #region --Operations--
        private readonly IntPtr device;
        private readonly GCHandle weakSelf;
        private GCHandle previewHandle;

        internal CameraDevice (IntPtr device) {
            this.device = device;
            this.weakSelf = GCHandle.Alloc(this, GCHandleType.Weak);
            // Register handlers
            device.SetDisconnectHandler(OnDeviceDisconnect, (IntPtr)weakSelf);
        }

        ~CameraDevice () {
            device.Release();
            weakSelf.Free();
        }

        [MonoPInvokeCallback(typeof(NatDevice.SampleBufferHandler))]
        private static unsafe void OnCameraImage (IntPtr context, IntPtr sampleBuffer) {
            var handle = (GCHandle)context;
            var handler = handle.Target as Action<IntPtr>;
            handler?.Invoke(sampleBuffer);
        }

        [MonoPInvokeCallback(typeof(NatDevice.SampleBufferHandler))]
        private static unsafe void OnCameraPhoto (IntPtr context, IntPtr sampleBuffer) {
            var handle = (GCHandle)context;
            var handler = handle.Target as Action<IntPtr>;
            handle.Free();
            handler?.Invoke(sampleBuffer);
        }

        [MonoPInvokeCallback(typeof(NatDevice.DeviceDisconnectHandler))]
        private static void OnDeviceDisconnect (IntPtr context) {
            var handle = (GCHandle)context;
            var device = handle.Target as CameraDevice;
            device?.onDisconnected?.Invoke();
        }

        #if UNITY_EDITOR
        private void OnPlayModeStateChanged (PlayModeStateChange state) {
            if (state == PlayModeStateChange.ExitingPlayMode)
                StopRunning();
        }
        #endif
        #endregion


        #region --Utility--

        public bool Equals (IMediaDevice other) => other != null && other is CameraDevice && other.uniqueID == uniqueID;

        public override string ToString () => $"camera:{uniqueID}";

        public static implicit operator IntPtr (CameraDevice device) => device.device;
        #endregion


        #region --DEPRECATED--
        [Obsolete(@"Deprecated in NatDevice 1.2.0. Use `ExposureModeSupported(ExposureMode.Locked)` instead.", false)]
        public bool exposureLockSupported => ExposureModeSupported(ExposureMode.Locked);

        [Obsolete(@"Deprecated in NatDevice 1.2.0. Use `FocusModeSupported(FocusMode.Locked)` instead.", false)]
        public bool focusLockSupported => FocusModeSupported(FocusMode.Locked);

        [Obsolete(@"Deprecated in NatDevice 1.2.0. Use `exposureBiasRange` instead.")]
        public (float min, float max) exposureRange => exposureBiasRange;

        [Obsolete(@"Deprecated in NatDevice 1.2.0. Use `exposureMode` instead.", false)]
        public bool exposureLock {
            get => exposureMode == ExposureMode.Locked;
            set => exposureMode = ExposureMode.Locked;
        }

        [Obsolete(@"Deprecated in NatDevice 1.2.0. Use `focusMode` instead.", false)]
        public bool focusLock {
            get => focusMode == FocusMode.Locked;
            set => focusMode = FocusMode.Locked;
        }

        [Obsolete(@"Deprecated in NatDevice 1.2.0. Use `SetExposurePoint` instead.", false)]
        public (float x, float y) exposurePoint { set => SetExposurePoint(value.x, value.y); }

        [Obsolete(@"Deprecated in NatDevice 1.2.0. Use `SetFocusPoint` instead.", false)]
        public (float x, float y) focusPoint { set => SetFocusPoint(value.x, value.y); }
        #endregion
    }
}