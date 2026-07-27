//
//  NDMediaDevice.h
//  NatDevice
//
//  Created by Yusuf Olokoba on 3/02/2021.
//  Copyright © 2022 NatML Inc. All rights reserved.
//

#pragma once

#include "NDMediaTypes.h"

/*!
 @function NDReleaseMediaDevice

 @abstract Dispose a device and release resources.

 @discussion Dispose a device and release resources.

 @param device
 Opaque handle to a media device.
*/
BRIDGE EXPORT void APIENTRY NDReleaseMediaDevice (NDMediaDevice* device);

/*!
 @function NDMediaDeviceUniqueID

 @abstract Get the media device unique ID.

 @discussion Get the media device unique ID.

 @param device
 Opaque handle to a media device.

 @param dstString
 Destination UTF-8 string.
*/
BRIDGE EXPORT void APIENTRY NDMediaDeviceUniqueID (
    NDMediaDevice* device,
    char* dstString
);

/*!
 @function NDMediaDeviceName
 
 @abstract Media device name.
 
 @discussion Media device name.
 
 @param device
 Opaque handle to a media device.
 
 @param dstString
 Destination string.
 */
BRIDGE EXPORT void APIENTRY NDMediaDeviceName (
    NDMediaDevice* device,
    char* dstString
);

/*!
 @function NDMediaDeviceFlags
 
 @abstract Get the media device flags.
 
 @discussion Get the media device flags.
 
 @param device
 Opaque handle to a media device.
 
 @returns Device flags.
*/
BRIDGE EXPORT NDDeviceFlags APIENTRY NDMediaDeviceFlags (NDMediaDevice* device);

/*!
 @function NDMediaDeviceIsRunning
 
 @abstract Is the device running?
 
 @discussion Is the device running?
 
 @param device
 Opaque handle to a media device.
 
 @returns True if device is running.
*/
BRIDGE EXPORT bool APIENTRY NDMediaDeviceIsRunning (NDMediaDevice* device);

/*!
 @function NDMediaDeviceStartRunning
 
 @abstract Start running an media device.
 
 @discussion Start running an media device.
 
 @param device
 Opaque handle to a media device.
 
 @param handler
 Sample buffer delegate to receive sample buffers as the device produces them.
 
 @param context
 User-provided context to be passed to the sample buffer delegate.
 */
BRIDGE EXPORT void APIENTRY NDMediaDeviceStartRunning (
    NDMediaDevice* device,
    NDSampleBufferHandler handler,
    void* context
);

/*!
 @function NDMediaDeviceStopRunning
 
 @abstract Stop running device.
 
 @discussion Stop running device.
 
 @param device
 Opaque handle to a media device.
 */
BRIDGE EXPORT void APIENTRY NDMediaDeviceStopRunning (NDMediaDevice* device);
