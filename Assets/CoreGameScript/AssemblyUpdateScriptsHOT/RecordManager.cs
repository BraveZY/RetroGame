using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FFmpeg;
using NatSuite.Recorders;
using NatSuite.Recorders.Clocks;
using NatSuite.Recorders.Inputs;
using NatSuite.Recorders.Internal;
using UnityEngine;
using UnityEngine.Video;

public class RecordManager : MonoBehaviour, IFFmpegHandler
{
    public static RecordManager Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        DeleteAll();
    }

    MP4Recorder recorder;
    CameraInput cameraInput;
    AudioInput audioInput;
    public string path;
    public string pathTrim;

    bool isRunning;

    List<float> times = new List<float>();
    Dictionary<float, float> time2Scores = new Dictionary<float, float>();
    public float duration = 60f;
    float timeWindowSize = 15f;

    public void AddTime(float duration, float time, float score)
    {
        this.duration = duration;
        float _time = duration - time;
        if (_time < 0)
            _time = 0;
        times.Add(_time);
        time2Scores[_time] = score;
    }

    public void AddTime(float time, float score)
    {
        times.Add(time);
        time2Scores[time] = score;
    }

    public (float start, float end, float delta) FindTimeWindow()
    {
        times.Sort();
        Dictionary<float, float> time2Deltas = new Dictionary<float, float>();
        if (times.Count > 0)
        {
            time2Deltas[times[0]] = time2Scores[times[0]];
            for (int i = 1; i < times.Count; i++)
                time2Deltas[times[i]] = time2Scores[times[i]] - time2Scores[times[i - 1]];
        }
        int windowCount = (int)Math.Ceiling(duration / timeWindowSize);
        float[] windowDeltas = new float[windowCount];
        for (int i = 0; i < times.Count; i++)
        {
            float time = times[i];
            int window = (int)(time / timeWindowSize);
            if (window < windowCount)
                windowDeltas[window] += time2Deltas[times[i]];
        }
        bool allZeros = true;
        for (int i = 0; i < windowCount; i++)
        {
            if (Math.Abs(windowDeltas[i]) >= float.Epsilon)
            {
                allZeros = false;
                break;
            }
        }
        if (allZeros)
        {
            float lastStart = Math.Max(0, duration - timeWindowSize);
            return (lastStart, duration, 0f);
        }
        int maxWindow = 0;
        for (int i = 1; i < windowCount; i++)
        {
            if (windowDeltas[i] > windowDeltas[maxWindow])
                maxWindow = i;
        }
        float start = maxWindow * timeWindowSize;
        float end = Math.Min((maxWindow + 1) * timeWindowSize, duration);
        return (start, end, windowDeltas[maxWindow]);
    }

    public void Begin(Camera camera = null)
    {
        if (!Directory.Exists(Utility.directory))
            Directory.CreateDirectory(Utility.directory);
        DeleteAll();
        if (!IsOpen)
            return;
        if (isRunning)
            return;
        var clock = new RealtimeClock();
        recorder = new MP4Recorder(720, 406, 15, AudioSettings.outputSampleRate, (int)AudioSettings.speakerMode, videoBitRate: 10000000, audioBitRate: 64000);
        cameraInput = new CameraInput(recorder, clock, camera ?? FindObjectOfType<Camera>());
        audioInput = new AudioInput(recorder, clock, FindObjectOfType<AudioListener>());
        isRunning = true;
        times.Clear();
        time2Scores.Clear();
    }
    TaskCompletionSource<bool> stopTCS;
    public async Task Stop(bool isDelete)
    {
        if (!isRunning)
            return;
        if (stopTCS != null)
            await stopTCS.Task;
        else
        {
            stopTCS = new TaskCompletionSource<bool>();
            audioInput?.Dispose();
            cameraInput?.Dispose();
            path = await recorder?.FinishWriting();
            Debug.Log($"Saved recording to: {path}");
            if (!isDelete)
            {
                var result = FindTimeWindow();
                FFmpegParser.Handler = this;
                Trim(result.start);
                await stopTCS.Task;
            }
            else
            {
                isRunning = false;
                stopTCS.SetResult(true);
                stopTCS = null;
            }
        }
    }

    public void Delete()
    {
        Delete(path);
        Delete(pathTrim);
    }

    public void Delete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch (Exception ex)
        {
        }
    }

    public void DeleteAll()
    {
        if (Directory.Exists(Utility.directory))
        {
            string[] files = Directory.GetFiles(Utility.directory, "*.mp4");
            for (int i = 0; i < files.Length; i++)
                Delete(files[i]);
        }
    }

    public void Trim(float seconds)
    {
        pathTrim = Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(path) + "_" + "trim" + Path.GetExtension(path);
        TrimData config = new TrimData();
        config.inputPath = path;
        config.fromTime = ToTimeFormat(seconds);
        config.durationSec = (int)timeWindowSize;
        config.outputPath = pathTrim;
        FFmpegCommands.Trim(config);
    }

    public string ToTimeFormat(float seconds)
    {
        int hours = (int)seconds / 3600;
        int minutes = ((int)seconds % 3600) / 60;
        int secs = (int)seconds % 60;
        return $"{hours:D2}:{minutes:D2}:{secs:D2}";
    }

    public void OnStart()
    {
        Debug.Log("FFmpegHandler.Start");
    }

    public void OnProgress(string msg)
    {
        // Debug.Log("FFmpegHandler.Progress: " + msg);
    }

    public void OnFailure(string msg)
    {
        Debug.Log("FFmpegHandler.Failure: " + msg);
    }

    public void OnSuccess(string msg)
    {
        Debug.Log("FFmpegHandler.Success: " + msg);
    }

    public void OnFinish()
    {
        Debug.Log("FFmpegHandler.Finish");
        FFmpegParser.Handler = null;
        isRunning = false;
        stopTCS.SetResult(true);
        stopTCS = null;
    }

    bool status
    {
        get
        {
            if (PlayerPrefs.HasKey("RecordStatus"))
                return PlayerPrefs.GetInt("RecordStatus") == 1;
            return false;
        }
        set
        {
            if (value)
                PlayerPrefs.SetInt("RecordStatus", 1);
            else
                PlayerPrefs.SetInt("RecordStatus", 0);
        }
    }

    public bool IsOpen
    {
        get
        {
            if (!status)
                return false;
            if (HttpServer.Instance != null && HttpServer.Instance.isNetworkError)
                return false;
            if (Application.internetReachability == NetworkReachability.NotReachable)
                return false;
            return true;
        }
    }

    VideoPlayer videoPlayer;
    public RenderTexture videoTexture;

    public void PlayVideo()
    {
        videoPlayer = this.GetComponent<VideoPlayer>();
        if (videoPlayer == null)
            videoPlayer = this.gameObject.AddComponent<VideoPlayer>();
        if (videoTexture == null)
        {
            videoTexture = RenderTexture.GetTemporary(new RenderTextureDescriptor(720, 406, RenderTextureFormat.ARGB32));
            videoPlayer.targetTexture = videoTexture;
        }
        videoPlayer.url = pathTrim;
        videoPlayer.isLooping = true;
        videoPlayer.Play();
    }

    public void StopVideo()
    {
        if (videoPlayer != null)
            videoPlayer.Stop();
        if (videoTexture != null)
        {
            RenderTexture.ReleaseTemporary(videoTexture);
            videoTexture = null;
        }
    }

    public bool VideoMute
    {
        set
        {
            if (videoPlayer != null)
                videoPlayer.SetDirectAudioMute(0, value);
        }
    }
}
