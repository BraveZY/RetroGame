#pragma once
#ifdef WIN32
#define DETECT_INTERFACE_API __declspec(dllexport)
#elif defined __GNUC__ && __GNUC__ >= 4
#define DETECT_INTERFACE_API __attribute__((visibility("default"))) // 编译期加 g++ -shared -o libmy_class.so my_class.cpp -fvisibility=hidden -fPIC
#else
#define DETEC_INTERFACE_API
#endif

#include <memory>
#include <string>
#include <vector>

extern "C"
{
    enum class alignas(8) ImageFormat
    {
        IMAGE_FORMAT_GRAY8,
        IMAGE_FORMAT_RGB888,
        IMAGE_FORMAT_RGBA8888,
        IMAGE_FORMAT_YUV420SP_NV21,
        IMAGE_FORMAT_YUV420SP_NV12,
    };

    struct alignas(8)  DetectImage
    {
        int width;
        int height;
        ImageFormat format;

        // std::string id;
        int64_t timestamp{0};

        unsigned char *data{nullptr};
    };

    struct alignas(8)  DetectKeypoint
    {
        float x;
        float y;
        float z{0};
        float conf;
    };

    struct alignas(8) DetectResult
    {
        float rect[4]; // x, y, w, h
        float prop;
        int cls_id;
        DetectKeypoint keypoints[20];

        // std::string id;
        int64_t timestamp;
    };

    struct alignas(8) DetectResultArray
    {
        int size{0};
        DetectResult results[10];
    };

#ifdef WIN32
    typedef void(__stdcall *OutputDelegate)(DetectResultArray);
#elif defined __GNUC__ && __GNUC__ >= 4
    typedef void(*OutputDelegate)(DetectResultArray);
#else
    typedef void(*OutputDelegate)(DetectResultArray);
#endif

    class DetectModel;
    class DetectPose
    {
        std::shared_ptr<DetectModel> m_model;

    public:
        DETECT_INTERFACE_API DetectPose();
        DETECT_INTERFACE_API ~DetectPose();

        DETECT_INTERFACE_API void setOutputCallback(OutputDelegate outputCallback);
        DETECT_INTERFACE_API void setMediaPipeMaxNum(int max_num);

        DETECT_INTERFACE_API int loadModel(std::string model_path, std::string media_model = "");
        DETECT_INTERFACE_API int inference(DetectImage im, float box_threshold, float nms_threshold);
        DETECT_INTERFACE_API int getResults(std::vector<DetectResult> &results);

        DETECT_INTERFACE_API int inferenceAsync(DetectImage im, float box_threshold, float nms_threshold);
    };


    DETECT_INTERFACE_API int     setLogPath(std::string log_path);

    DETECT_INTERFACE_API int64_t createDetectPose();
    DETECT_INTERFACE_API void    destroyDetectPose(int64_t pose);

    DETECT_INTERFACE_API void SetOutputCallback(int64_t pose, OutputDelegate outputCallback);
    DETECT_INTERFACE_API void setMediaPipeMaxNum(int64_t pose, int max_num);

    DETECT_INTERFACE_API int loadModel(int64_t pose, const char *model_path, const char *media_model = "");
    DETECT_INTERFACE_API int inference(int64_t pose, DetectImage im, float box_threshold, float nms_threshold);

    DETECT_INTERFACE_API int inferenceAsync(int64_t pose, DetectImage im, float box_threshold, float nms_threshold);

} // extern "C"