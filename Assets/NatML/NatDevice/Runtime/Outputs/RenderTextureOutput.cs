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
    /// Camera device output that streams camera images into a `RenderTexture` for display.
    /// The render texture output performs necessary conversions entirely on the GPU.
    /// This output can provide better performance than the `TextureOutput` when pixel data is not accessed on the CPU.
    /// </summary>
    internal sealed class RenderTextureOutput : IDisposable { // INCOMPLETE // Handle YUV and orientation in shader

        #region --Client API--
        /// <summary>
        /// RenderTexture containing the camera image.
        /// </summary>
        public RenderTextureOutput texture => null;

        /// <summary>
        /// Create a RenderTexture output.
        /// </summary>
        public RenderTextureOutput () {

        }

        /// <summary>
        /// Update the output with a new camera image.
        /// </summary>
        public void Update (CameraImage image) {

        }

        /// <summary>
        /// Dispose the RenderTexture output and release resources.
        /// </summary>
        public void Dispose () {

        }
        #endregion


        #region --Operations--

        public static implicit operator Action<CameraImage> (RenderTextureOutput output) => output.Update;
        #endregion
    }
}