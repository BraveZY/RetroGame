/* 
*   NatDevice
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatSuite.Devices {

    using System;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Internal;

    public partial class CameraImage {

        /// <summary>
        /// Image plane for planar formats.
        /// </summary>
        public sealed class Plane {

            #region --Client API--
            /// <summary>
            /// Pixel buffer.
            /// </summary>
            public unsafe NativeArray<byte> buffer {
                get {
                    var pixelBuffer = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(
                        image.CameraImagePlaneData(plane),
                        image.CameraImagePlaneDataSize(plane),
                        Allocator.None
                    );
                    #if ENABLE_UNITY_COLLECTIONS_CHECKS
                    NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref pixelBuffer, AtomicSafetyHandle.Create());
                    #endif
                    return pixelBuffer;
                }
            }

            /// <summary>
            /// Plane width.
            /// </summary>
            public int width => image.CameraImagePlaneWidth(plane);

            /// <summary>
            /// Plane height.
            /// </summary>
            public int height => image.CameraImagePlaneHeight(plane);
            
            /// <summary>
            /// Pixel stride in bytes.
            /// </summary>
            public int pixelStride => image.CameraImagePlanePixelStride(plane);

            /// <summary>
            /// Row stride in bytes.
            /// </summary>
            public int rowStride => image.CameraImagePlaneRowStride(plane);
            #endregion


            #region --Operations--
            private readonly IntPtr image;
            private readonly int plane;

            internal Plane (IntPtr image, int plane) {
                this.image = image;
                this.plane = plane;
            }
            #endregion
        }
    }
}