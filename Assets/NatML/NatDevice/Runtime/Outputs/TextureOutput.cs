/* 
*   NatDevice
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatSuite.Devices.Outputs {

    using System;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using UnityEngine;

    /// <summary>
    /// Camera device output that streams camera images into a `Texture2D`.
    /// The texture output uses the `PixelBufferOutput` to convert camera images to `RGBA8888` before uploading to the GPU.
    /// The rendered texture data is accessible on the CPU using the `Texture2D` data access methods.
    /// </summary>
    public sealed class TextureOutput : IDisposable {

        #region --Client API--
        /// <summary>
        /// Texture containing the camera image.
        /// </summary>
        public Texture2D texture => taskCompletionSource.Task.IsCompleted ? tex : null;

        /// <summary>
        /// Create a texture output.
        /// </summary>
        /// <param name="pixelBufferOutput">Pixel buffer output used to convert the image.</param>
        public TextureOutput (PixelBufferOutput pixelBufferOutput = null) {
            this.disposePBO = pixelBufferOutput == null;
            this.pixelBufferOutput = pixelBufferOutput ?? new PixelBufferOutput();
            this.taskCompletionSource = new TaskCompletionSource<Texture2D>();
            this.tex = new Texture2D(16, 16, TextureFormat.RGBA32, false, false);
        }

        /// <summary>
        /// Update the output with a new camera image.
        /// </summary>
        public void Update (CameraImage image) {
            // Update pixel buffer output
            pixelBufferOutput.Update(image);
            // Check size
            if (tex.width != pixelBufferOutput.width || tex.height != pixelBufferOutput.height)
                tex.Reinitialize(pixelBufferOutput.width, pixelBufferOutput.height);
            // Update texture
            tex.GetRawTextureData<byte>().CopyFrom(pixelBufferOutput.pixelBuffer);
            tex.Apply();
            // Complete task
            taskCompletionSource.TrySetResult(tex);
        }

        /// <summary>
        /// Dispose the texture output and release resources.
        /// </summary>
        public void Dispose () {
            if (disposePBO)
                pixelBufferOutput.Dispose();
            taskCompletionSource.TrySetCanceled();
            Texture2D.Destroy(tex);
        }
        #endregion


        #region --Operations--
        private readonly bool disposePBO;
        private readonly PixelBufferOutput pixelBufferOutput;
        private readonly TaskCompletionSource<Texture2D> taskCompletionSource;
        private readonly Texture2D tex;

        public TaskAwaiter<Texture2D> GetAwaiter () => taskCompletionSource.Task.GetAwaiter();

        public static implicit operator Action<CameraImage> (TextureOutput output) => output.Update;
        #endregion
    }
}