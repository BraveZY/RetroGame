/* 
*   NatDevice
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatSuite.Devices.Outputs {

    using System;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using UnityEngine;
    using Unity.Collections;

    /// <summary>
    /// </summary>
    internal sealed class AudioSpectrumOutput : IDisposable { // INCOMPLETE

        #region --Client API--
        /// <summary>
        /// Create an audio spectrum output.
        /// </summary>
        public AudioSpectrumOutput () {

        }

        /// <summary>
        /// Update the output with a new audio buffer.
        /// </summary>
        /// <param name="buffer">Audio buffer.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Update (AudioBuffer buffer) {

        }

        /// <summary>
        /// Dispose the output and release resources.
        /// </summary>
        public void Dispose () {

        }
        #endregion


        #region --Operations--

        public static implicit operator Action<AudioBuffer> (AudioSpectrumOutput output) => output.Update;
        #endregion
    }
}