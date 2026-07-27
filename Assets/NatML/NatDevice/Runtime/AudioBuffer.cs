/* 
*   NatDevice
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatSuite.Devices {

    using System;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Internal;

    /// <summary>
    /// Audio buffer provided by audio device.
    /// The audio buffer always contains a linear PCM sample buffer interleaved by channel.
    /// </summary>
    public sealed class AudioBuffer {

        #region --Client API--
        /// <summary>
        /// Audio device that this buffer was generated from.
        /// </summary>
        public readonly AudioDevice device;

        /// <summary>
        /// Audio sample buffer.
        /// This is always linear PCM and interleaved by channel.
        /// </summary>
        public unsafe NativeArray<float> sampleBuffer {
            get {
                var sampleBuffer = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<float>(
                    audioBuffer.AudioBufferData(),
                    audioBuffer.AudioBufferSampleCount(),
                    Allocator.None
                );
                #if ENABLE_UNITY_COLLECTIONS_CHECKS
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref sampleBuffer, AtomicSafetyHandle.Create());
                #endif
                return sampleBuffer;
            }
        }

        /// <summary>
        /// Audio buffer sample rate.
        /// </summary>
        public int sampleRate => audioBuffer.AudioBufferSampleRate();

        /// <summary>
        /// Audio buffer channel count.
        /// </summary>
        public int channelCount => audioBuffer.AudioBufferChannelCount();

        /// <summary>
        /// Audio buffer timestamp in nanoseconds.
        /// The timestamp is based on the system media clock.
        /// </summary>
        public long timestamp => audioBuffer.AudioBufferTimestamp();
        #endregion


        #region --Operations--
        private readonly IntPtr audioBuffer;

        internal AudioBuffer (AudioDevice device, IntPtr audioBuffer) {
            this.device = device;
            this.audioBuffer = audioBuffer;
        }

        public static implicit operator IntPtr (AudioBuffer audioBuffer) => audioBuffer.audioBuffer;
        #endregion
    }
}