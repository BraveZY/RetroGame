#import <CoreML/CoreML.h>
#import <Foundation/Foundation.h>

#include "MacYoloPose.h"

#include <algorithm>
#include <atomic>
#include <mutex>
#include <string>
#include <vector>

@interface MacYoloPoseBundleMarker : NSObject
@end

@implementation MacYoloPoseBundleMarker
@end

namespace
{
constexpr int32_t InputSize = 320;
constexpr int32_t ExpectedOutputCount = 56 * 2100;
constexpr float LetterboxPadding = 114.0f / 255.0f;

class Session
{
public:
    Session()
    {
        queue = dispatch_queue_create("com.motionsport.macyolopose", DISPATCH_QUEUE_SERIAL);
        @autoreleasepool
        {
            NSBundle* bundle = [NSBundle bundleForClass:[MacYoloPoseBundleMarker class]];
            NSURL* modelUrl = [bundle URLForResource:@"YoloPose" withExtension:@"mlmodelc"];
            if (modelUrl == nil)
            {
                error = "YoloPose.mlmodelc is missing from MacYoloPose.bundle";
                return;
            }

            MLModelConfiguration* configuration = [[MLModelConfiguration alloc] init];
            configuration.computeUnits = MLComputeUnitsAll;
            configuration.allowLowPrecisionAccumulationOnGPU = YES;
            NSError* loadError = nil;
            model = [MLModel modelWithContentsOfURL:modelUrl configuration:configuration error:&loadError];
            if (model == nil)
            {
                error = loadError.localizedDescription.UTF8String ?: "Core ML model could not be loaded";
                return;
            }

            NSError* arrayError = nil;
            input = [[MLMultiArray alloc] initWithShape:@[@1, @3, @(InputSize), @(InputSize)]
                                              dataType:MLMultiArrayDataTypeFloat32
                                                 error:&arrayError];
            if (input == nil)
            {
                error = arrayError.localizedDescription.UTF8String ?: "Core ML input allocation failed";
                return;
            }

            NSError* featureError = nil;
            MLFeatureValue* inputValue = [MLFeatureValue featureValueWithMultiArray:input];
            provider = [[MLDictionaryFeatureProvider alloc] initWithDictionary:@{@"images": inputValue} error:&featureError];
            if (provider == nil)
            {
                error = featureError.localizedDescription.UTF8String ?: "Core ML input provider failed";
            }
        }
    }

    ~Session()
    {
        if (queue != nullptr)
        {
            dispatch_sync(queue, ^{});
        }
    }

    bool Submit(const uint8_t* pixels, int32_t width, int32_t height, int32_t rowStride, bool mirror)
    {
        if (model == nil || input == nil || provider == nil)
        {
            return false;
        }
        if (pixels == nullptr || width <= 0 || height <= 0 || rowStride < width * 4)
        {
            error = "Invalid RGBA frame";
            return false;
        }

        bool expected = false;
        if (!busy.compare_exchange_strong(expected, true))
        {
            std::lock_guard<std::mutex> lock(resultMutex);
            error = "MacYoloPose session is busy";
            return false;
        }

        const size_t frameByteCount = static_cast<size_t>(rowStride) * height;
        inputFrame.resize(frameByteCount);
        std::copy_n(pixels, frameByteCount, inputFrame.begin());
        {
            std::lock_guard<std::mutex> lock(resultMutex);
            error.clear();
        }
        dispatch_async(queue, ^{
            RunInference(width, height, rowStride, mirror);
            busy.store(false);
        });
        return true;
    }

    bool IsBusy() const
    {
        return busy.load();
    }

    int64_t OutputVersion() const
    {
        return outputVersion.load();
    }

    int32_t OutputCount()
    {
        std::lock_guard<std::mutex> lock(resultMutex);
        return static_cast<int32_t>(outputValues.size());
    }

    int32_t CopyOutput(float* destination, int32_t destinationCount)
    {
        std::lock_guard<std::mutex> lock(resultMutex);
        if (destination == nullptr || destinationCount < static_cast<int32_t>(outputValues.size()))
        {
            return 0;
        }
        std::copy(outputValues.begin(), outputValues.end(), destination);
        return static_cast<int32_t>(outputValues.size());
    }

    const char* LastError()
    {
        static thread_local std::string copiedError;
        std::lock_guard<std::mutex> lock(resultMutex);
        copiedError = error;
        return copiedError.c_str();
    }

private:
    void RunInference(int32_t width, int32_t height, int32_t rowStride, bool mirror)
    {
        @autoreleasepool
        {
            const float scale = std::min(static_cast<float>(InputSize) / width, static_cast<float>(InputSize) / height);
            const float resizedWidth = width * scale;
            const float resizedHeight = height * scale;
            const float padX = (InputSize - resizedWidth) * 0.5f;
            const float padY = (InputSize - resizedHeight) * 0.5f;
            float* values = static_cast<float*>(input.dataPointer);
            std::fill(values, values + InputSize * InputSize * 3, LetterboxPadding);

            for (int32_t y = 0; y < InputSize; ++y)
            {
                const float targetY = y + 0.5f;
                if (targetY < padY || targetY >= padY + resizedHeight)
                {
                    continue;
                }

                const float sourceY = (y - padY + 0.5f) / scale - 0.5f;
                const int32_t sy = std::clamp(static_cast<int32_t>(sourceY + 0.5f), 0, height - 1);
                for (int32_t x = 0; x < InputSize; ++x)
                {
                    const float targetX = x + 0.5f;
                    if (targetX < padX || targetX >= padX + resizedWidth)
                    {
                        continue;
                    }

                    const float sourceX = (x - padX + 0.5f) / scale - 0.5f;
                    int32_t sx = std::clamp(static_cast<int32_t>(sourceX + 0.5f), 0, width - 1);
                    if (mirror)
                    {
                        sx = width - 1 - sx;
                    }
                    const uint8_t* pixel = inputFrame.data() + sy * rowStride + sx * 4;
                    const int32_t offset = y * InputSize + x;
                    values[offset] = pixel[0] / 255.0f;
                    values[InputSize * InputSize + offset] = pixel[1] / 255.0f;
                    values[InputSize * InputSize * 2 + offset] = pixel[2] / 255.0f;
                }
            }

            NSError* predictionError = nil;
            id<MLFeatureProvider> prediction = [model predictionFromFeatures:provider error:&predictionError];
            if (prediction == nil)
            {
                SetError(predictionError.localizedDescription.UTF8String ?: "Core ML prediction failed");
                return;
            }

            MLMultiArray* output = [prediction featureValueForName:@"output0"].multiArrayValue;
            if (output == nil || output.dataType != MLMultiArrayDataTypeFloat32 || output.shape.count != 3 ||
                output.shape[0].intValue != 1 || output.shape[1].intValue != 56 || output.shape[2].intValue != 2100 ||
                output.count != ExpectedOutputCount)
            {
                SetError("Core ML output0 has an unexpected shape");
                return;
            }

            const float* source = static_cast<const float*>(output.dataPointer);
            const int64_t batchStride = output.strides[0].longLongValue;
            const int64_t channelStride = output.strides[1].longLongValue;
            const int64_t candidateStride = output.strides[2].longLongValue;
            {
                std::lock_guard<std::mutex> lock(resultMutex);
                outputValues.resize(ExpectedOutputCount);
                for (int32_t channel = 0; channel < 56; ++channel)
                {
                    for (int32_t candidate = 0; candidate < 2100; ++candidate)
                    {
                        outputValues[channel * 2100 + candidate] = source[batchStride * 0 + channel * channelStride + candidate * candidateStride];
                    }
                }
                error.clear();
                outputVersion.fetch_add(1);
            }
        }
    }

    void SetError(const char* message)
    {
        std::lock_guard<std::mutex> lock(resultMutex);
        error = message;
    }

public:
    MLModel* model = nil;
    MLMultiArray* input = nil;
    MLDictionaryFeatureProvider* provider = nil;
    std::vector<uint8_t> inputFrame;
    std::vector<float> outputValues;
    std::string error;
    dispatch_queue_t queue = nullptr;
    std::mutex resultMutex;
    std::atomic<bool> busy = false;
    std::atomic<int64_t> outputVersion = 0;
};

Session* ToSession(MacYoloPoseSession handle)
{
    return static_cast<Session*>(handle);
}
}

MacYoloPoseSession MYLP_Create(void)
{
    return new Session();
}

void MYLP_Destroy(MacYoloPoseSession session)
{
    delete ToSession(session);
}

int32_t MYLP_SubmitRgba(MacYoloPoseSession session, const uint8_t* pixels, int32_t width, int32_t height, int32_t rowStride, int32_t mirror)
{
    Session* nativeSession = ToSession(session);
    return nativeSession != nullptr && nativeSession->Submit(pixels, width, height, rowStride, mirror != 0) ? 1 : 0;
}

int32_t MYLP_IsBusy(MacYoloPoseSession session)
{
    Session* nativeSession = ToSession(session);
    return nativeSession != nullptr && nativeSession->IsBusy() ? 1 : 0;
}

int64_t MYLP_GetOutputVersion(MacYoloPoseSession session)
{
    Session* nativeSession = ToSession(session);
    return nativeSession == nullptr ? 0 : nativeSession->OutputVersion();
}

int32_t MYLP_GetOutputCount(MacYoloPoseSession session)
{
    Session* nativeSession = ToSession(session);
    return nativeSession == nullptr ? 0 : nativeSession->OutputCount();
}

int32_t MYLP_CopyOutput(MacYoloPoseSession session, float* destination, int32_t destinationCount)
{
    Session* nativeSession = ToSession(session);
    return nativeSession == nullptr ? 0 : nativeSession->CopyOutput(destination, destinationCount);
}

const char* MYLP_GetLastError(MacYoloPoseSession session)
{
    Session* nativeSession = ToSession(session);
    return nativeSession == nullptr ? "MacYoloPose session is null" : nativeSession->LastError();
}
