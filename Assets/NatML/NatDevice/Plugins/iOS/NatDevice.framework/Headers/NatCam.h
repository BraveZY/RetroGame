//
//  NatCam.h
//  NatDevice
//
//  Created by Yusuf Olokoba on 1/14/2021.
//  Copyright © 2022 NatML Inc. All rights reserved.
//

#pragma once

#include "NDMediaDevice.h"

/*!
 @function NDCameraDeviceCount
 
 @abstract Get the number of available camera devices.
 
 @discussion Get the number of available camera devices.

 @returns Number of available camera devices.
*/
BRIDGE EXPORT int32_t APIENTRY NDCameraDeviceCount (void);

/*!
 @function NDCameraDevices

 @abstract Get all available camera devices.

 @discussion Get all available camera devices.

 @param devices
 Array populated with opaque pointers to camera devices.

 @param size
 Array size.
*/
BRIDGE EXPORT void APIENTRY NDCameraDevices (
    NDMediaDevice** devices,
    int32_t size
);

/*!
 @function NDCameraDeviceFieldOfView

 @abstract Camera field of view in degrees.

 @discussion Camera field of view in degrees.

 @param cameraDevice
 Opaque handle to a camera device.

 @param outWidth
 Output FOV width in degrees.

 @param outHeight
 Output FOV height in degrees.
 */
BRIDGE EXPORT void APIENTRY NDCameraDeviceFieldOfView (
    NDMediaDevice* cameraDevice,
    float* outWidth,
    float* outHeight
);

/*!
 @function NDCameraDeviceExposureBiasRange

 @abstract Camera exposure bias range in EV.

 @discussion Camera exposure bias range in EV.

 @param cameraDevice
 Opaque handle to a camera device.

 @param outMin
 Output minimum exposure bias.

 @param outMax
 Output maximum exposure bias.
 */
BRIDGE EXPORT void APIENTRY NDCameraDeviceExposureBiasRange (
    NDMediaDevice* cameraDevice,
    float* outMin,
    float* outMax
);

/*!
 @function NDCameraDeviceExposureDurationRange

 @abstract Camera exposure duration range in seconds.

 @discussion Camera exposure duration range in seconds.

 @param cameraDevice
 Opaque handle to a camera device.

 @param outMin
 Output minimum exposure duration in seconds.

 @param outMax
 Output maximum exposure duration in seconds.
 */
BRIDGE EXPORT void APIENTRY NDCameraDeviceExposureDurationRange (
    NDMediaDevice* cameraDevice,
    float* outMin,
    float* outMax
);

/*!
 @function NDCameraDeviceISORange

 @abstract Camera sensor sensitivity range.

 @discussion Camera sensor sensitivity range.

 @param cameraDevice
 Opaque handle to a camera device.

 @param outMin
 Output minimum ISO sensitivity value.

 @param outMax
 Output maximum ISO sensitivity value.
 */
BRIDGE EXPORT void APIENTRY NDCameraDeviceISORange (
    NDMediaDevice* cameraDevice,
    float* outMin,
    float* outMax
);

/*!
 @function NDCameraDeviceZoomRange

 @abstract Camera optical zoom range.

 @discussion Camera optical zoom range.

 @param cameraDevice
 Opaque handle to a camera device.

 @param outMin
 Output minimum zoom ratio. This is always 1.

 @param outMax
 Output maximum zoom ratio.
 */
BRIDGE EXPORT void APIENTRY NDCameraDeviceZoomRange (
    NDMediaDevice* cameraDevice, 
    float* outMin, 
    float* outMax
);

/*!
 @function NDCameraDevicePreviewResolution

 @abstract Get the camera preview resolution.

 @discussion Get the camera preview resolution.

 @param cameraDevice
 Opaque handle to a camera device.

 @param outWidth
 Output width in pixels.

 @param outHeight
 Output height in pixels.
 */
BRIDGE EXPORT void APIENTRY NDCameraDevicePreviewResolution (
    NDMediaDevice* cameraDevice,
    int32_t* outWidth,
    int32_t* outHeight
);

/*!
 @function NDCameraDeviceSetPreviewResolution

 @abstract Set the camera preview resolution.

 @discussion Set the camera preview resolution.

 Most camera devices do not support arbitrary preview resolutions, so the camera will
 set a supported resolution which is closest to the requested resolution that is specified.

 Note that this method should only be called before the camera preview is started.

 @param cameraDevice
 Opaque handle to a camera device.

 @param width
 Width in pixels.

 @param height
 Height in pixels.
 */
BRIDGE EXPORT void APIENTRY NDCameraDeviceSetPreviewResolution (
    NDMediaDevice* cameraDevice,
    int32_t width,
    int32_t height
);

/*!
 @function NDCameraDevicePhotoResolution

 @abstract Get the camera photo resolution.

 @discussion Get the camera photo resolution.

 @param cameraDevice
 Opaque handle to a camera device.

 @param outWidth
 Output width in pixels.

 @param outHeight
 Output height in pixels.
 */
BRIDGE EXPORT void APIENTRY NDCameraDevicePhotoResolution (
    NDMediaDevice* cameraDevice,
    int32_t* outWidth,
    int32_t* outHeight
);

/*!
 @function NDCameraDeviceSetPhotoResolution

 @abstract Set the camera photo resolution.

 @discussion Set the camera photo resolution.

 Most camera devices do not support arbitrary photo resolutions, so the camera will
 set a supported resolution which is closest to the requested resolution that is specified.

 Note that this method should only be called before the camera preview is started.

 @param cameraDevice
 Opaque handle to a camera device.

 @param width
 Width in pixels.

 @param height
 Height in pixels.
 */
BRIDGE EXPORT void APIENTRY NDCameraDeviceSetPhotoResolution (
    NDMediaDevice* cameraDevice,
    int32_t width,
    int32_t height
);

/*!
 @function NDCameraDeviceFrameRate

 @abstract Get the camera preview frame rate.

 @discussion Get the camera preview frame rate.

 @param cameraDevice
 Opaque handle to a camera device.

 @returns Camera preview frame rate.
 */
BRIDGE EXPORT int32_t APIENTRY NDCameraDeviceFrameRate (NDMediaDevice* cameraDevice);

/*!
 @function NDCameraDeviceSetFrameRate

 @abstract Set the camera preview frame rate.

 @discussion Set the camera preview frame rate.

 Note that this method should only be called before the camera preview is started.

 @param cameraDevice
 Opaque handle to a camera device.

 @param frameRate
 Frame rate to set.
 */
BRIDGE EXPORT void APIENTRY NDCameraDeviceSetFrameRate (
    NDMediaDevice* cameraDevice,
    int32_t frameRate
);

/*!
 @function NDCameraDeviceExposureMode

 @abstract Get the camera exposure mode.

 @discussion Get the camera exposure mode.

 @param cameraDevice
 Opaque handle to a camera device.

 @returns Exposure mode.
 */
BRIDGE EXPORT NDExposureMode APIENTRY NDCameraDeviceExposureMode (NDMediaDevice* cameraDevice);

/*!
 @function NDCameraDeviceSetExposureMode

 @abstract Set the camera exposure mode.

 @discussion Set the camera exposure mode.

 @param cameraDevice
 Opaque handle to a camera device.

 @param mode
 Exposure mode.
 */
BRIDGE EXPORT void APIENTRY NDCameraDeviceSetExposureMode (
    NDMediaDevice* cameraDevice,
    NDExposureMode mode
);

/*!
 @function NDCameraDeviceExposureBias

 @abstract Get the camera exposure bias.

 @discussion Get the camera exposure bias.

 @param cameraDevice
 Opaque handle to a camera device.

 @returns Camera exposure bias.
 */
BRIDGE EXPORT float APIENTRY NDCameraDeviceExposureBias (NDMediaDevice* cameraDevice);

/*!
 @function NDCameraDeviceSetExposureBias

 @abstract Set the camera exposure bias.

 @discussion Set the camera exposure bias.

 Note that the value MUST be in the camera exposure range.

 @param cameraDevice
 Opaque handle to a camera device.

 @param bias
 Exposure bias value to set.
 */
BRIDGE EXPORT void APIENTRY NDCameraDeviceSetExposureBias (
    NDMediaDevice* cameraDevice,
    float bias
);

/*!
 @function NDCameraDeviceSetExposurePoint

 @abstract Set the camera exposure point of interest.

 @discussion Set the camera exposure point of interest.
 The coordinates are specified in viewport space, with each value in range [0., 1.].

 @param cameraDevice
 Opaque handle to a camera device.

 @param x
 Exposure point x-coordinate in viewport space.

 @param y
 Exposure point y-coordinate in viewport space.
 */
BRIDGE EXPORT void APIENTRY NDCameraDeviceSetExposurePoint (
    NDMediaDevice* cameraDevice,
    float x,
    float y
);

/*!
 @function NDCameraDeviceSetExposureDuration

 @abstract Set the camera exposure duration.

 @discussion Set the camera exposure duration.
 This method will automatically change the camera's exposure mode to `MANUAL`.

 @param cameraDevice
 Opaque handle to a camera device.

 @param duration
 Exposure duration in seconds. MUST be in `ExposureDurationRange`.

 @param ISO
 Shutter sensitivity. MUST be in `ISORange`.
 */
BRIDGE EXPORT void APIENTRY NDCameraDeviceSetExposureDuration (
    NDMediaDevice* cameraDevice,
    float duration,
    float ISO
);

/*!
 @function NDCameraDeviceFlashMode

 @abstract Get the camera photo flash mode.

 @discussion Get the camera photo flash mode.

 @param cameraDevice
 Opaque handle to a camera device.

 @returns Camera photo flash mode.
 */
BRIDGE EXPORT NDFlashMode APIENTRY NDCameraDeviceFlashMode (NDMediaDevice* cameraDevice);

/*!
 @function NDCameraDeviceSetFlashMode

 @abstract Set the camera photo flash mode.

 @discussion Set the camera photo flash mode.

 @param cameraDevice
 Opaque handle to a camera device.

 @param mode
 Flash mode to set.
 */
BRIDGE EXPORT void APIENTRY NDCameraDeviceSetFlashMode (
    NDMediaDevice* cameraDevice,
    NDFlashMode mode
);

/*!
 @function NDCameraDeviceFocusLock

 @abstract Get the camera focus lock.

 @discussion Get the camera focus lock.

 @param cameraDevice
 Opaque handle to a camera device.

 @returns True if the camera focus is locked.
 */
BRIDGE EXPORT bool APIENTRY NDCameraDeviceFocusLock (NDMediaDevice* cameraDevice);

/*!
 @function NDCameraDeviceSetFocusLock

 @abstract Set the camera focus lock.

 @discussion Set the camera focus lock.
 This function should only be used if the camera supports setting the focus lock.
 See `NDCameraDeviceFocusLockSupported`.

 @param cameraDevice
 Opaque handle to a camera device.

 @param locked
 Lock value.
 */
BRIDGE EXPORT void APIENTRY NDCameraDeviceSetFocusLock (
    NDMediaDevice* cameraDevice,
    bool locked
);

/*!
 @function NDCameraDeviceSetFocusPoint

 @abstract Set the camera focus point.

 @discussion Set the camera focus point of interest.
 The coordinates are specified in viewport space, with each value in range [0., 1.].
 This function should only be used if the camera supports setting the focus point.
 See `NDCameraDeviceFocusPointSupported`.

 @param cameraDevice
 Opaque handle to a camera device.

 @param x
 Focus point x-coordinate in viewport space.

 @param y
 Focus point y-coordinate in viewport space.
 */
BRIDGE EXPORT void APIENTRY NDCameraDeviceSetFocusPoint (
    NDMediaDevice* cameraDevice,
    float x,
    float y
);

/*!
 @function NDCameraDeviceTorchEnabled

 @abstract Check whether the camera torch unit is enabled.

 @discussion Check whether the camera torch unit is enabled.

 @param cameraDevice
 Opaque handle to a camera device.

 @returns True if the camera's torch unit is enabled.
 */
BRIDGE EXPORT bool APIENTRY NDCameraDeviceTorchEnabled (NDMediaDevice* cameraDevice);

/*!
 @function NDCameraDeviceSetTorchEnabled

 @abstract Enable or disable the camera torch unit.

 @discussion Enable or disable the camera torch unit.
 This function should only be used if the camera has a torch unit.
 See `NDCameraDeviceTorchSupported`.

 @param cameraDevice
 Opaque handle to a camera device.

 @param enabled
 Torch enabled value.
 */
BRIDGE EXPORT void APIENTRY NDCameraDeviceSetTorchEnabled (
    NDMediaDevice* cameraDevice,
    bool enabled
);

/*!
 @function NDCameraDeviceWhiteBalanceLock

 @abstract Get the camera white balance lock.

 @discussion Get the camera white balance lock.

 @param cameraDevice
 Opaque handle to a camera device.

 @returns True if the camera white balance is locked.
 */
BRIDGE EXPORT bool APIENTRY NDCameraDeviceWhiteBalanceLock (NDMediaDevice* cameraDevice);

/*!
 @function NDCameraDeviceSetWhiteBalanceLock

 @abstract Set the camera white balance lock.

 @discussion Set the camera white balance lock.

 @param cameraDevice
 Opaque handle to a camera device.

 @param locked
 Lock value.
 */
BRIDGE EXPORT void APIENTRY NDCameraDeviceSetWhiteBalanceLock (
    NDMediaDevice* cameraDevice,
    bool locked
);

/*!
 @function NDCameraDeviceZoomRatio

 @abstract Get the camera zoom ratio.

 @discussion Get the camera zoom ratio.
 This value will always be within the minimum and maximum zoom values reported by the camera device.

 @param cameraDevice
 Opaque handle to a camera device.

 @returns Zoom ratio.
 */
BRIDGE EXPORT float APIENTRY NDCameraDeviceZoomRatio (NDMediaDevice* cameraDevice);

/*!
 @function NDCameraDeviceSetZoomRatio

 @abstract Set the camera zoom ratio.

 @discussion Set the camera zoom ratio.
 This value must always be within the minimum and maximum zoom values reported by the camera device.

 @param cameraDevice
 Opaque handle to a camera device.

 @param ratio
 Zoom ratio.
 */
BRIDGE EXPORT void APIENTRY NDCameraDeviceSetZoomRatio (
    NDMediaDevice* cameraDevice,
    float ratio
);

/*!
 @function NDCameraDeviceCapturePhoto

 @abstract Capture a still photo.

 @discussion Capture a still photo.

 @param cameraDevice
 Opaque handle to a camera device.

 @param handler
 Photo handler.

 @param context
 User-provided context.
 */
BRIDGE EXPORT void APIENTRY NDCameraDeviceCapturePhoto (
    NDMediaDevice* cameraDevice,
    NDSampleBufferHandler handler,
    void* context
);

/*!
 @function NDCameraDeviceSetDisconnectHandler

 @abstract Set the camera disconnect handler.

 @discussion Set the camera disconnect handler.
 This provided function pointer is invoked when the device disconnects.

 @param cameraDevice
 Opaque handle to a camera device.

 @param handler
 Disconnect handler.

 @param context
 User-provided context.
 */
BRIDGE EXPORT void APIENTRY NDCameraDeviceSetDisconnectHandler (
    NDMediaDevice* cameraDevice,
    NDDeviceDisconnectHandler handler,
    void* context
);
