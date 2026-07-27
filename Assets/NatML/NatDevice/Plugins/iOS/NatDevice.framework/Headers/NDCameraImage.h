//
//  NDCameraImage.h
//  NatDevice
//
//  Created by Yusuf Olokoba on 10/28/2021.
//  Copyright © 2022 NatML Inc. All rights reserved.
//

#include "NDMediaTypes.h"

/*!
 @function NDCameraImageData

 @abstract Get the image data of a camera image.

 @discussion Get the image data of a camera image.
 If the camera image uses a planar format, this will return `NULL`.

 @param cameraImage
 Camera image.
*/
BRIDGE EXPORT void* APIENTRY NDCameraImageData (NDSampleBuffer* cameraImage);

/*!
 @function NDCameraImageDataSize

 @abstract Get the image data size of a camera image in bytes.

 @discussion Get the image data size of a camera image in bytes.

 @param cameraImage
 Camera image.
*/
BRIDGE EXPORT int32_t APIENTRY NDCameraImageDataSize (NDSampleBuffer* cameraImage);

/*!
 @function NDCameraImageFormat

 @abstract Get the format of a camera image.

 @discussion Get the format of a camera image.

 @param cameraImage
 Camera image.
*/
BRIDGE EXPORT NDImageFormat APIENTRY NDCameraImageFormat (NDSampleBuffer* cameraImage);

/*!
 @function NDCameraImageWidth

 @abstract Get the width of a camera image.

 @discussion Get the width of a camera image.

 @param cameraImage
 Camera image.
*/
BRIDGE EXPORT int32_t APIENTRY NDCameraImageWidth (NDSampleBuffer* cameraImage);

/*!
 @function NDCameraImageHeight

 @abstract Get the height of a camera image.

 @discussion Get the height of a camera image.

 @param cameraImage
 Camera image.
*/
BRIDGE EXPORT int32_t APIENTRY NDCameraImageHeight (NDSampleBuffer* cameraImage);

/*!
 @function NDCameraImageTimestamp

 @abstract Get the timestamp of a camera image.

 @discussion Get the timestamp of a camera image.

 @param cameraImage
 Camera image.

 @returns Image timestamp in nanoseconds.
*/
BRIDGE EXPORT int64_t APIENTRY NDCameraImageTimestamp (NDSampleBuffer* cameraImage);

/*!
 @function NDCameraImageVerticallyMirrored

 @abstract Whether the camera image is vertically mirrored.

 @discussion Whether the camera image is vertically mirrored.

 @param cameraImage
 Camera image.
*/
BRIDGE EXPORT bool APIENTRY NDCameraImageVerticallyMirrored (NDSampleBuffer* cameraImage);

/*!
 @function NDCameraImagePlaneCount

 @abstract Get the plane count of a camera image.

 @discussion Get the plane count of a camera image.
 If the image uses an interleaved format or only has a single plane, this function returns zero.

 @param cameraImage
 Camera image.

 @returns Number of planes in image.
*/
BRIDGE EXPORT int32_t APIENTRY NDCameraImagePlaneCount (NDSampleBuffer* cameraImage);

/*!
 @function NDCameraImagePlaneData

 @abstract Get the plane data for a given plane of a camera image.

 @discussion Get the plane data for a given plane of a camera image.

 @param cameraImage
 Camera image.
 
 @param planeIdx
 Plane index.
*/
BRIDGE EXPORT void* APIENTRY NDCameraImagePlaneData (
    NDSampleBuffer* cameraImage,
    int32_t planeIdx
);

/*!
 @function NDCameraImagePlaneDataSize

 @abstract Get the plane data size of a given plane of a camera image.

 @discussion Get the plane data size of a given plane of a camera image.

 @param cameraImage
 Camera image.
 
 @param planeIdx
 Plane index.
*/
BRIDGE EXPORT int32_t APIENTRY NDCameraImagePlaneDataSize (
    NDSampleBuffer* cameraImage,
    int32_t planeIdx
);

/*!
 @function NDCameraImagePlaneWidth

 @abstract Get the width of a given plane of a camera image.

 @discussion Get the width of a given plane of a camera image.

 @param cameraImage
 Camera image.
 
 @param planeIdx
 Plane index.
*/
BRIDGE EXPORT int32_t APIENTRY NDCameraImagePlaneWidth (
    NDSampleBuffer* cameraImage,
    int32_t planeIdx
);

/*!
 @function NDCameraImagePlaneHeight

 @abstract Get the height of a given plane of a camera image.

 @discussion Get the height of a given plane of a camera image.

 @param cameraImage
 Camera image.
 
 @param planeIdx
 Plane index.
*/
BRIDGE EXPORT int32_t APIENTRY NDCameraImagePlaneHeight (
    NDSampleBuffer* cameraImage,
    int32_t planeIdx
);

/*!
 @function NDCameraImagePlanePixelStride

 @abstract Get the plane pixel stride for a given plane of a camera image.

 @discussion Get the plane pixel stride for a given plane of a camera image.

 @param cameraImage
 Camera image.
 
 @param planeIdx
 Plane index.

 @returns Plane pixel stride in bytes.
*/
BRIDGE EXPORT int32_t APIENTRY NDCameraImagePlanePixelStride (
    NDSampleBuffer* cameraImage,
    int32_t planeIdx
);

/*!
 @function NDCameraImagePlaneRowStride

 @abstract Get the plane row stride for a given plane of a camera image.

 @discussion Get the plane row stride for a given plane of a camera image.

 @param cameraImage
 Camera image.
 
 @param planeIdx
 Plane index.

 @returns Plane row stride in bytes.
*/
BRIDGE EXPORT int32_t APIENTRY NDCameraImagePlaneRowStride (
    NDSampleBuffer* cameraImage,
    int32_t planeIdx
);

/*!
 @function NDCameraImageMetadata

 @abstract Get the metadata value for a given key in a camera image.

 @discussion Get the metadata value for a given key in a camera image.

 @param cameraImage
 Camera image.
 
 @param key
 Metadata key.

 @param value
 Destination value array.

 @param count
 Destination value array size.

 @returns Whether the metadata key was successfully looked up.
*/
BRIDGE EXPORT bool APIENTRY NDCameraImageMetadata (
    NDSampleBuffer* cameraImage,
    NDMetadataKey key,
    float* value,
    int32_t count
);
