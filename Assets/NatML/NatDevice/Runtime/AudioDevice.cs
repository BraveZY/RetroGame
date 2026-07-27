/* 
*   NatDevice
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatSuite.Devices {

    using System;
    using System.Runtime.InteropServices;
    using System.Text;
    using AOT;
    using UnityEngine;
    using Internal;
    using DeviceFlags = Internal.NatDevice.DeviceFlags;

    #if UNITY_EDITOR
    using UnityEditor;
    #endif

    /// <summary>
    /// Hardware audio input device.
    /// </summary>
    public sealed class AudioDevice : IMediaDevice {
        
        #region --Client API--
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
        /// Device location.
        /// </summary>
        public DeviceLocation location => (DeviceLocation)((int)device.Flags() & 0x3);

        /// <summary>
        /// Device is the default device for its media type.
        /// </summary>
        public bool defaultForMediaType => device.Flags().HasFlag(DeviceFlags.Default);

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
        /// Is echo cancellation supported?
        /// </summary>
        public bool echoCancellationSupported => device.Flags().HasFlag(DeviceFlags.EchoCancellation);

        /// <summary>
        /// Enable or disable Adaptive Echo Cancellation (AEC).
        /// </summary>
        public bool echoCancellation {
            get => device.EchoCancellation();
            set => device.SetEchoCancellation(value);
        }

        /// <summary>
        /// Audio sample rate.
        /// </summary>
        public int sampleRate {
            get => device.SampleRate();
            set => device.SetSampleRate(value);
        }

        /// <summary>
        /// Audio channel count.
        /// </summary>
        public int channelCount {
            get => device.ChannelCount();
            set => device.SetChannelCount(value);
        }

        /// <summary>
        /// Is the device running?
        /// </summary>
        public bool running => device.Running();

        /// <summary>
        /// Start running.
        /// </summary>
        /// <param name="handler">Delegate to receive audio buffers.</param>
        public void StartRunning (Action<AudioBuffer> handler) {
            Action<IntPtr> wrapper = sampleBuffer => {
                var audioBuffer = new AudioBuffer(this, sampleBuffer);
                handler?.Invoke(audioBuffer);
            };
            handle = GCHandle.Alloc(wrapper, GCHandleType.Normal);
            device.StartRunning(OnAudioBuffer, (IntPtr)handle);
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
            handle.Free();
            handle = default;
        }
        #endregion


        #region --Operations--
        private readonly IntPtr device;
        private GCHandle handle;

        internal AudioDevice (IntPtr device) => this.device = device;

        ~AudioDevice () => device.Release();

        [MonoPInvokeCallback(typeof(NatDevice.SampleBufferHandler))]
        private static void OnAudioBuffer (IntPtr context, IntPtr sampleBuffer) {
            try {
                var handle = (GCHandle)context;
                var handler = handle.Target as Action<IntPtr>;
                handler?.Invoke(sampleBuffer);
            }
            catch (Exception ex) {
                Debug.LogError($"NatDevice Error: Audio buffer handler raised exception");
                Debug.LogException(ex);
            }
        }

        #if UNITY_EDITOR
        private void OnPlayModeStateChanged (PlayModeStateChange state) {
            if (state == PlayModeStateChange.ExitingPlayMode)
                StopRunning();
        }
        #endif
        #endregion


        #region --Utility--

        public bool Equals (IMediaDevice other) => other != null && other is AudioDevice && other.uniqueID == uniqueID;

        public override string ToString () => $"microphone:{uniqueID}";

        public static implicit operator IntPtr (AudioDevice device) => device.device;
        #endregion
    }
}