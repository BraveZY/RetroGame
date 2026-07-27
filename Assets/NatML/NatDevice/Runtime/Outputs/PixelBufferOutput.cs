/* 
*   NatDevice
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatSuite.Devices.Outputs {

    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Internal;

    /// <summary>
    /// Camera device output that converts camera images into RGBA8888 pixel buffers.
    /// </summary>
    public sealed class PixelBufferOutput : IDisposable {

        #region --Client API--
        /// <summary>
        /// Pixel buffer with latest camera image.
        /// The pixel buffer is always laid out in RGBA8888 format.
        /// </summary>
        public NativeArray<byte> pixelBuffer { get; private set; }

        /// <summary>
        /// Pixel buffer width.
        /// </summary>
        public int width { get; private set; }

        /// <summary>
        /// Pixel buffer height.
        /// </summary>
        public int height { get; private set; }

        /// <summary>
        /// Get or set the pixel buffer orientation.
        /// </summary>
        public ScreenOrientation orientation;

        /// <summary>
        /// Create a pixel buffer output.
        /// </summary>
        public PixelBufferOutput () {
            this.orientation = OrientationSupport.Contains(Application.platform) ? Screen.orientation : 0;
        }

        /// <summary>
        /// Update the output with a new camera image.
        /// </summary>
        /// <param name="image">Camera image.</param>
        public unsafe void Update (CameraImage image) {
            // Create
            var bufferSize = image.width * image.height * 4;
            if (!pixelBuffer.IsCreated)
                pixelBuffer = new NativeArray<byte>(bufferSize, Allocator.Persistent);
            // Check
            if (pixelBuffer.Length != bufferSize)
                throw new ArgumentException($"PixelBufferOutput received image with size {bufferSize} but expected {pixelBuffer.Length}");
            // Shortcut
            if (image.format == CameraImage.Format.RGBA8888 && !image.verticallyMirrored && orientation == 0) {
                this.width = image.width;
                this.height = image.height;
                image.pixelBuffer.CopyTo(pixelBuffer);
                return;
            }
            // Create temp buffer
            if (!tempBuffer.IsCreated)
                tempBuffer = new NativeArray<byte>(image.width * image.height * 4, Allocator.Persistent);
            // Convert
            NatDeviceExt.Convert(
                image,
                (int)orientation,
                tempBuffer.GetUnsafePtr(),
                pixelBuffer.GetUnsafePtr(),
                out var width,
                out var height
            );
            this.width = width;
            this.height = height;
        }

        /// <summary>
        /// Dispose the pixel buffer output and release resources.
        /// </summary>
        public void Dispose () {
            if (pixelBuffer.IsCreated)
                pixelBuffer.Dispose();
            if (tempBuffer.IsCreated)
                tempBuffer.Dispose();
        }
        #endregion


        #region --Operations--
        private NativeArray<byte> tempBuffer;
        private static readonly List<RuntimePlatform> OrientationSupport = new List<RuntimePlatform> {
            RuntimePlatform.Android,
            RuntimePlatform.IPhonePlayer
        };

        public static implicit operator Action<CameraImage> (PixelBufferOutput output) => output.Update;
        #endregion
    }
}