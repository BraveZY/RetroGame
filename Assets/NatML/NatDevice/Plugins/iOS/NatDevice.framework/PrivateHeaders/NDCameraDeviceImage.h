//
//  NDCameraDeviceImage.h
//  NatDevice
//
//  Created by Yusuf Olokoba on 10/30/2021.
//  Copyright © 2022 NatML Inc. All rights reserved.
//

@import AVFoundation;

@interface NDCameraDeviceImage : NSObject
@property (readonly, nonnull) CVPixelBufferRef pixelBuffer;
@property (readonly) UInt64 timestamp;
@property (readonly) bool verticallyMirrored;
@property (readonly) bool hasIntrinsicMatrix;
@property (readonly) matrix_float3x3 intrinsicMatrix;
@property (readonly, nonnull) NSDictionary* metadata;
- (nonnull instancetype) initWithSampleBuffer:(CMSampleBufferRef _Nonnull) sampleBuffer andMirror:(bool) mirror;
- (nonnull instancetype) initWithPhoto:(AVCapturePhoto* _Nonnull) photo andMirror:(bool) mirror;
@end
