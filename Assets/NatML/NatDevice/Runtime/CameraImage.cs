/* 
*   NatDevice
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatSuite.Devices {

    using System;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Internal;
    using MetadataKey = Internal.NatDevice.MetadataKey;

    /// <summary>
    /// Camera image provided by camera device.
    /// The camera image always contains a pixel buffer along with image metadata.
    /// The format of the pixel buffer varies by platform and must be taken into consideration when using the pixel data.
    /// </summary>
    public sealed partial class CameraImage {

        #region --Types--
        /// <summary>
        /// Image buffer format.
        /// </summary>
        public enum Format { // CHECK // NatDevice.h
            /// <summary>
            /// Unknown image format.
            /// </summary>
            Unknown = 0,
            /// <summary>
            /// Generic YUV 420 planar format.
            /// </summary>
            YCbCr420 = 1,
            /// <summary>
            /// RGBA8888.
            /// </summary>
            RGBA8888 = 2,        
        }
        #endregion


        #region --Properties--
        /// <summary>
        /// Camera device that this image was generated from.
        /// </summary>
        public readonly CameraDevice device;

        /// <summary>
        /// Image pixel buffer.
        /// Some planar images might not have a contiguous pixel buffer.
        /// In this case, the buffer is uninitialized.
        /// </summary>
        public unsafe NativeArray<byte> pixelBuffer {
            get {
                var pixelBuffer = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(
                    image.CameraImageData(),
                    image.CameraImageDataSize(),
                    Allocator.None
                );
                #if ENABLE_UNITY_COLLECTIONS_CHECKS
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref pixelBuffer, AtomicSafetyHandle.Create());
                #endif
                return pixelBuffer;
            }
        }

        /// <summary>
        /// Image format.
        /// </summary>
        public Format format => image.CameraImageFormat();

        /// <summary>
        /// Image width.
        /// </summary>
        public int width => image.CameraImageWidth();

        /// <summary>
        /// Image height.
        /// </summary>
        public int height => image.CameraImageHeight();

        /// <summary>
        /// Image timestamp in nanoseconds.
        /// The timestamp is based on the system media clock.
        /// </summary>
        public long timestamp => image.CameraImageTimestamp();

        /// <summary>
        /// Whether the image is vertically mirrored.
        /// </summary>
        public bool verticallyMirrored => image.CameraImageVerticallyMirrored();

        /// <summary>
        /// Image plane for planar formats.
        /// This is `null` for interleaved formats.
        /// </summary>
        public readonly Plane[] planes;

        /// <summary>
        /// Camera intrinsics as a flattened row-major 3x3 matrix.
        /// This is `null` if the camera does not report it.
        /// </summary>
        public unsafe float[] intrinsics {
            get {
                var result = new float[9];
                fixed (float* dst = result)
                    if (image.CameraImageMetadata(MetadataKey.IntrinsicMatrix, dst, result.Length))
                        return result;
                    return null;
            }
        }

        /// <summary>
        /// Exposure bias value in EV.
        /// This is `null` if the camera does not report it.
        /// </summary>
        public unsafe float? exposureBias {
            get {
                var result = 0f;
                if (image.CameraImageMetadata(MetadataKey.ExposureBias, &result))
                    return result;
                return null;
            }
        }

        /// <summary>
        /// Image exposure duration in seconds.
        /// This is `null` if the camera does not report it.
        /// </summary>
        public unsafe float? exposureDuration {
            get {
                var result = 0f;
                if (image.CameraImageMetadata(MetadataKey.ExposureDuration, &result))
                    return result;
                return null;
            }
        }

        /// <summary>
        /// Sensor sensitivity ISO value.
        /// This is `null` if the camera does not report it.
        /// </summary>
        public unsafe float? ISO {
            get {
                var result = 0f;
                if (image.CameraImageMetadata(MetadataKey.ISO, &result))
                    return result;
                return null;
            }
        }

        /// <summary>
        /// Camera focal length in millimeters.
        /// This is `null` if the camera does not report it.
        /// </summary>
        public unsafe float? focalLength {
            get {
                var result = 0f;
                if (image.CameraImageMetadata(MetadataKey.FocalLength, &result))
                    return result;
                return null;
            }
        }

        /// <summary>
        /// Image aperture, in f-number.
        /// This is `null` if the camera does not report it.
        /// </summary>
        public unsafe float? fNumber {
            get {
                var result = 0f;
                if (image.CameraImageMetadata(MetadataKey.FNumber, &result))
                    return result;
                return null;
            }
        }

        /// <summary>
        /// Ambient brightness.
        /// This is `null` if the camera does not report it.
        /// </summary>
        public unsafe float? brightness {
            get {
                var result = 0f;
                if (image.CameraImageMetadata(MetadataKey.Brightness, &result))
                    return result;
                return null;
            }
        }
        #endregion


        #region --Operations--
        private readonly IntPtr image;

        internal CameraImage (CameraDevice device, IntPtr image) {
            this.device = device;
            this.image = image;
            // Get planes up front to prevent GC on access
            var planeCount = image.CameraImagePlaneCount();
            if (planeCount > 0) {
                this.planes = new Plane[planeCount];
                for (var i = 0; i < planeCount; ++i)
                    planes[i] = new Plane(image, i);
            }
        }

        public static implicit operator IntPtr (CameraImage image) => image.image;
        #endregion
    }
}