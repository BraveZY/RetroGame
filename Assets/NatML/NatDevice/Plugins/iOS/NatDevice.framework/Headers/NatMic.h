//
//  NatMic.h
//  NatDevice
//
//  Created by Yusuf Olokoba on 1/14/2021.
//  Copyright © 2022 NatML Inc. All rights reserved.
//

#pragma once

#include "NDMediaDevice.h"

/*!
 @function NDAudioDeviceCount
 
 @abstract Get the number of available audio devices.
 
 @discussion Get the number of available audio devices.

 @returns Number of available audio devices.
*/
BRIDGE EXPORT int32_t APIENTRY NDAudioDeviceCount (void);

/*!
 @function NDAudioDevices
 
 @abstract Get all available audio devices.
 
 @discussion Get all available audio devices.
 
 @param audioDevices
 Array populated with opaque pointers to audio devices.
 
 @param size
 Array size.
 */
BRIDGE EXPORT void APIENTRY NDAudioDevices (
    NDMediaDevice** audioDevices,
    int32_t size
);

/*!
 @function NDEchoCancellation
 
 @abstract Get the device echo cancellation mode.
 
 @discussion Get the device echo cancellation mode.
 
 @param audioDevice
 Opaque handle to an audio device.
 
 @returns True if the device performs adaptive echo cancellation.
 */
BRIDGE EXPORT bool APIENTRY NDAudioDeviceEchoCancellation (NDMediaDevice* audioDevice);

/*!
 @function NDAudioDeviceSetEchoCancellation
 
 @abstract Enable or disable echo cancellation on the device.
 
 @discussion If the device does not support echo cancellation, this will be a nop.
 
 @param audioDevice
 Opaque handle to an audio device.
 
 @param echoCancellation
 Echo cancellation.
 */
BRIDGE EXPORT void APIENTRY NDAudioDeviceSetEchoCancellation (
    NDMediaDevice* audioDevice,
    bool echoCancellation
);

/*!
 @function NDAudioDeviceSampleRate
 
 @abstract Audio device sample rate.
 
 @discussion Audio device sample rate.
 
 @param audioDevice
 Opaque handle to an audio device.
 
 @returns Current sample rate.
 */
BRIDGE EXPORT int32_t APIENTRY NDAudioDeviceSampleRate (NDMediaDevice* audioDevice);

/*!
 @function NDAudioDeviceSetSampleRate
 
 @abstract Set the audio device sample rate.
 
 @discussion Set the audio device sample rate.
 
 @param audioDevice
 Opaque handle to an audio device.
 
 @param sampleRate
 Sample rate to set.
 */
BRIDGE EXPORT void APIENTRY NDAudioDeviceSetSampleRate (
    NDMediaDevice* audioDevice,
    int32_t sampleRate
);

/*!
 @function NDAudioDeviceChannelCount
 
 @abstract Audio device channel count.
 
 @discussion Audio device channel count.
 
 @param audioDevice
 Opaque handle to an audio device.
 
 @returns Current channel count.
 */
BRIDGE EXPORT int32_t APIENTRY NDAudioDeviceChannelCount (NDMediaDevice* audioDevice);

/*!
 @function NDAudioDeviceSetChannelCount
 
 @abstract Set the audio device channel count.
 
 @discussion Set the audio device channel count.
 
 @param audioDevice
 Opaque handle to an audio device.
 
 @param channelCount
 Channel count to set.
 */
BRIDGE EXPORT void APIENTRY NDAudioDeviceSetChannelCount (
    NDMediaDevice* audioDevice,
    int32_t channelCount
);
