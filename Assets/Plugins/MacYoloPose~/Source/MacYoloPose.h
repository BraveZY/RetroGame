#pragma once

#include <stdint.h>

#ifdef __cplusplus
extern "C" {
#endif

typedef void* MacYoloPoseSession;

MacYoloPoseSession MYLP_Create(void);
void MYLP_Destroy(MacYoloPoseSession session);
int32_t MYLP_SubmitRgba(
    MacYoloPoseSession session,
    const uint8_t* pixels,
    int32_t width,
    int32_t height,
    int32_t rowStride,
    int32_t mirror);
int32_t MYLP_IsBusy(MacYoloPoseSession session);
int64_t MYLP_GetOutputVersion(MacYoloPoseSession session);
int32_t MYLP_GetOutputCount(MacYoloPoseSession session);
int32_t MYLP_CopyOutput(MacYoloPoseSession session, float* destination, int32_t destinationCount);
const char* MYLP_GetLastError(MacYoloPoseSession session);

#ifdef __cplusplus
}
#endif
