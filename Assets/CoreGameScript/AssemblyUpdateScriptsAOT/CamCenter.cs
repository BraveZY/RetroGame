// #define NatCam_1_6
using System.Collections;
using UnityEngine;
using System;
using GameCoreRuntime;
using UnityEngine.Rendering;
using Unity.Collections;
using Resolution = UnityEngine.Resolution;

public class CamCenter : MonoBehaviour
{
    static CamCenter instance;
    public static CamCenter Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<CamCenter>();
            if (instance == null)
                instance = new GameObject("CamCenter").AddComponent<CamCenter>();
            if (instance != null)
            {
                instance.Init();
                DontDestroyOnLoad(instance.gameObject);
            }
            return instance;
        }
    }
    void Awake()
    {
        if (instance == null)
            instance = this;
        if (instance == this)
        {
            instance.Init();
            DontDestroyOnLoad(instance.gameObject);
        }
    }
    ICamBase impl;
    void Init()
    {

        if (impl != null)
            return;
        //if (Application.platform == RuntimePlatform.Android)
        //    impl = new WebCam();
        //else if (Application.platform == RuntimePlatform.IPhonePlayer)
        //    impl = new NatCam1();
        //else

        //===Add
        // impl = new WebCam();
        // if (GameCore.IsInit)
        if (!Application.isEditor)
        {
            impl = new AddCamera();
        }
        // else
        // {
        //     impl = new WebCam();
        // }
        //===
    }
    public IEnumerator IELaunch(bool front, int width, int height)
    {
        if (!PermissionTool.HasCamera)
            PermissionTool.RequestCamera();
        yield return new WaitUntil(() => PermissionTool.HasCamera);
        yield return StartCoroutine(impl.IELaunch(front, width, height));
    }
    void OnDestroy() { Stop(); }
    public Color32[] Pixels32 { get { return impl != null ? impl.Pixels32 : new Color32[0]; } }
    public int Width { get { return impl != null ? impl.Width : 0; } }
    public int Height { get { return impl != null ? impl.Height : 0; } }
    public Texture Preview { get { return impl != null ? impl.Preview : null; } }
    public ECamPluginType PluginType { get { return impl != null ? impl.PluginType : ECamPluginType.None; } }
    public ECamState State { get { return impl != null ? impl.State : ECamState.None; } }
    public bool Front { get { return impl != null ? impl.Front : false; } }
    public int Count { get { return impl != null ? impl.Count : 0; } }
    public void Pause() { if (impl != null) impl.Pause(); }
    public void Resume() { if (impl != null) { impl.Resume(); } }
    public void Stop() { if (impl != null) impl.Stop(); }
    public Color[] GetPixels(int x, int y, int width, int height) { return impl != null ? impl.GetPixels(x, y, width, height) : new Color[0]; }
    public int NativeCount
    {
        get
        {
            try
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    AndroidJavaClass buildClass = new AndroidJavaClass("android.os.Build");
                    AndroidJavaClass versionClass = new AndroidJavaClass("android.os.Build$VERSION");
                    int sdk_int = versionClass.GetStatic<int>("SDK_INT");
                    if (sdk_int >= 21)
                    {
                        AndroidJavaClass contextClass = new AndroidJavaClass("android.content.Context");
                        string camera_service = contextClass.GetStatic<string>("CAMERA_SERVICE");
                        AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                        AndroidJavaObject currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
                        AndroidJavaObject cameraManager = currentActivity.Call<AndroidJavaObject>("getSystemService", camera_service);
                        AndroidJavaObject cameraIdList = cameraManager.Call<AndroidJavaObject>("getCameraIdList");
                        AndroidJavaClass arrayClass = new AndroidJavaClass("java.lang.reflect.Array");
                        int length = arrayClass.CallStatic<int>("getLength", cameraIdList);
                        Debug.Log("camera2 count " + length);
                        return length;
                    }
                    else
                    {
                        AndroidJavaClass cameraClass = new AndroidJavaClass("android.hardware.Camera");
                        int numberOfCameras = cameraClass.CallStatic<int>("getNumberOfCameras");
                        Debug.Log("camera1 count " + numberOfCameras);
                        return numberOfCameras;
                    }
                }
                return WebCamTexture.devices.Length;
            }
            catch (AndroidJavaException ex)
            {
                Debug.LogError("camera count error " + ex.ToString());
            }
            return 0;
        }
    }
    public interface ICamBase
    {
        IEnumerator IELaunch(bool front, int width, int height);
        Color32[] Pixels32 { get; }
        int Width { get; }
        int Height { get; }
        int Angle { get; }
        bool Front { get; }
        Texture Preview { get; }
        ECamPluginType PluginType { get; }
        ECamState State { get; }
        int Count { get; }
        void Stop();
        void Pause();
        void Resume();
        public Color[] GetPixels(int x, int y, int width, int height);
    }
#if NatCam_1_6
    public class NatCam1 : ICamBase
    {
        public IEnumerator IELaunch(bool front, int width, int height)
        {
            if (front == this.Front && State == ECamState.Playing)
                yield break;
            if (NatCamU.Core.NatCam.Camera == null)
            {
                if (front)
                    NatCamU.Core.NatCam.Camera = NatCamU.Core.DeviceCamera.FrontCamera;
            }
            if (NatCamU.Core.NatCam.Camera == null)
            {
                if (!front)
                    NatCamU.Core.NatCam.Camera = NatCamU.Core.DeviceCamera.RearCamera;
            }
            if (NatCamU.Core.NatCam.Camera == null)
                yield break;
            NatCamU.Core.NatCam.Camera.SetPreviewResolution(NatCamU.Core.ResolutionPreset.Medium);
            NatCamU.Core.NatCam.Camera.SetPhotoResolution(NatCamU.Core.ResolutionPreset.Medium);
            NatCamU.Core.NatCam.Camera.SetFramerate(NatCamU.Core.FrameratePreset.Default);
            NatCamU.Core.NatCam.Play();
            State = ECamState.Playing;
        }
        public void Stop()
        {
            if (NatCamU.Core.NatCam.Camera != null)
                NatCamU.Core.NatCam.Release();
            State = ECamState.Stoped;
        }
        public void Pause()
        {
            if (NatCamU.Core.NatCam.Camera != null)
                NatCamU.Core.NatCam.Pause();
            State = ECamState.Paused;
        }
        public void Resume()
        {
            if (NatCamU.Core.NatCam.Camera != null)
                NatCamU.Core.NatCam.Play();
            State = ECamState.Playing;
        }
        public Color32[] Pixels32 { get { return NatCamU.Core.NatCam.Preview  != null ? (NatCamU.Core.NatCam.Preview  as Texture2D).GetPixels32() : new Color32[0]; } }
        public int Width { get { return NatCamU.Core.NatCam.Preview != null ? NatCamU.Core.NatCam.Preview.width : 0; } }
        public int Height { get { return NatCamU.Core.NatCam.Preview != null ? NatCamU.Core.NatCam.Preview.height : 0; } }
        public int Angle { get { return 0; } }
        public Texture Preview { get { return NatCamU.Core.NatCam.Preview; } }
        public ECamPluginType PluginType { get { return ECamPluginType.NatCam; } }
        public ECamState State { get; private set; }
        public bool Front { get { return NatCamU.Core.NatCam.Camera != null && NatCamU.Core.DeviceCamera.FrontCamera != null ? NatCamU.Core.NatCam.Camera == NatCamU.Core.DeviceCamera.FrontCamera : false; } }
        public int Count
        {
            get
            {
                int value = 0;
                if (NatCamU.Core.DeviceCamera.FrontCamera != null)
                    value++;
                if (NatCamU.Core.DeviceCamera.RearCamera != null)
                    value++;
                return value;
            }
        }
        public Color[] GetPixels(int x, int y, int width, int height) { return NatCamU.Core.NatCam.Preview != null ? (NatCamU.Core.NatCam.Preview as Texture2D).GetPixels(x, y, width, height) : new Color[0]; }
    }
#else
    /*    public class NatCam1 : ICamBase
        {
            Texture2D texture;
            NatCam.CameraDevice device;
            public IEnumerator IELaunch(bool front, int width, int height)
            {
                if (front == this.Front && State == ECamState.Playing)
                    yield break;
                if (device == null)
                    TryGetDevice(front, out device);
                if (device == null)
                    TryGetDevice(!front, out device);
                if (device == null)
                {
                    if (NatCam.CameraDevice.GetDevices() != null && NatCam.CameraDevice.GetDevices().Length > 0)
                        device = NatCam.CameraDevice.GetDevices()[0];
                    else
                        yield break;
                }
                device.PreviewResolution = new Resolution { width = width, height = height };
                device.StartPreview((texture) => this.texture = texture);
                State = ECamState.Playing;
            }
            void TryGetDevice(bool front, out NatCam.CameraDevice device)
            {
                device = null;
                var devices = NatCam.CameraDevice.GetDevices();
                if (devices == null)
                    return;
                for (int i = 0; i < devices.Length; i++)
                {
                    if (devices[i].IsFrontFacing == front)
                    {
                        device = devices[i];
                        continue;
                    }
                }
            }
            public void Stop()
            {
                if (device != null)
                    device.StopPreview();
                State = ECamState.Stoped;
            }
            public void Pause()
            {
                if (device != null)
                    device.StopPreview();
                State = ECamState.Paused;
            }
            public void Resume()
            {
                if (device != null)
                    device.StartPreview((texture) => this.texture = texture);
                State = ECamState.Playing;
            }
            public Color32[] Pixels32 { get { return texture != null ? texture.GetPixels32() : new Color32[0]; } }
            public int Width { get { return texture != null ? texture.width : 0; } }
            public int Height { get { return texture != null ? texture.height : 0; } }
            public int Angle { get { return 0; } }
            public Texture Preview { get { return texture; } }
            public ECamPluginType PluginType { get { return ECamPluginType.NatCam; } }
            public ECamState State { get; private set; }
            public bool Front { get { return device != null ? device.IsFrontFacing : false; } }
            public int Count { get { return NatCam.CameraDevice.GetDevices().Length; } }
            public Color[] GetPixels(int x, int y, int width, int height) { return texture != null ? texture.GetPixels(x, y, width, height) : new Color[0]; }
        }*/
#endif
    public class WebCam : ICamBase
    {
        WebCamDevice? device;
        Resolution? resolution;
        WebCamTexture cam;
        public IEnumerator IELaunch(bool front, int width, int height)
        {
            if (front == this.Front && State == ECamState.Playing)
                yield break;
#if UNITY_2018_4_OR_NEWER
            if (!device.HasValue || !resolution.HasValue)
                TryGetDevice(front, width, height, out device, out resolution);
            if (!device.HasValue || !resolution.HasValue)
                TryGetDevice(!front, width, height, out device, out resolution);
            if (!device.HasValue || !resolution.HasValue)
            {
                if (WebCamTexture.devices.Length > 0)
                    device = WebCamTexture.devices[0];
                else
                    device = new WebCamDevice();
                cam = new WebCamTexture(device.Value.name);
            }
            else
                cam = new WebCamTexture(device.Value.name, resolution.Value.width, resolution.Value.height);
#else
            if (!device.HasValue)
                TryGetDevice(front, out device);
            if (!device.HasValue)
                TryGetDevice(!front, out device);
            if (!device.HasValue)
            {
                if (WebCamTexture.devices.Length > 0)
                    device = WebCamTexture.devices[0];
                else
                    device = new WebCamDevice();
            }
            cam = new WebCamTexture(device.Value.name, bestWidth, bestHeight);
#endif
            if (cam != null)
                cam.Play();
            else
                yield break;
            yield return new WaitUntil(() => cam != null && cam.isPlaying && cam.width > 16 && cam.height > 16 && cam.GetPixels32().Length > 0);
            State = ECamState.Playing;
        }
#if UNITY_2018_4_OR_NEWER
        void TryGetDevice(bool front, int width, int height, out WebCamDevice? device, out Resolution? resolution)
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            for (int i = 0; i < devices.Length; i++)
            {
                Resolution[] resolutions = devices[i].availableResolutions;
                Debug.LogError("Camera " + devices[i].name);
                if (resolutions == null)
                    continue;
                for (int j = 0; j < resolutions.Length; j++)
                    Debug.LogError("Camera " + devices[i].name + " Resolution " + resolutions[j].width + "x" + resolutions[j].height);
            }
            device = null;
            resolution = null;
            for (int i = 0; i < devices.Length; i++)
            {
                if (devices[i].isFrontFacing != front)
                    continue;
                Resolution[] resolutions = devices[i].availableResolutions;
                if (resolutions == null || (resolution != null && resolutions.Length <= 0))
                    continue;
                for (int j = 0; j < resolutions.Length; j++)
                {
                    if (!resolution.HasValue)
                    {
                        resolution = resolutions[j];
                        device = devices[i];
                    }
                    else
                    {
                        if (width > height)
                        {
                            if (resolutions[j].width > resolutions[j].height)
                            {
                                if (Mathf.Abs(resolutions[j].width - width) <= Mathf.Abs(resolution.Value.width - width) &&
                                    Mathf.Abs(resolutions[j].height - height) <= Mathf.Abs(resolution.Value.height - height))
                                {
                                    resolution = resolutions[j];
                                    device = devices[i];
                                }
                            }
                            else
                            {
                                if (Mathf.Abs(resolutions[j].height - width) <= Mathf.Abs(resolution.Value.height - width) &&
                                    Mathf.Abs(resolutions[j].width - height) <= Mathf.Abs(resolution.Value.width - height))
                                {
                                    resolution = resolutions[j];
                                    device = devices[i];
                                }
                            }
                        }
                        else
                        {
                            if (resolutions[j].width < resolutions[j].height)
                            {
                                if (Mathf.Abs(resolutions[j].width - width) <= Mathf.Abs(resolution.Value.width - width) &&
                                    Mathf.Abs(resolutions[j].height - height) <= Mathf.Abs(resolution.Value.height - height))
                                {
                                    resolution = resolutions[j];
                                    device = devices[i];
                                }
                            }
                            else
                            {
                                if (Mathf.Abs(resolutions[j].height - width) <= Mathf.Abs(resolution.Value.height - width) &&
                                    Mathf.Abs(resolutions[j].width - height) <= Mathf.Abs(resolution.Value.width - height))
                                {
                                    resolution = resolutions[j];
                                    device = devices[i];
                                }
                            }
                        }
                    }
                }
            }
        }
#else
        void TryGetDevice(bool front, out WebCamDevice? device)
        {
            WebCamDevice[] devices = WebCamTexture.devices;
            device = null;
            for (int i = 0; i < devices.Length; i++)
            {
                if (devices[i].isFrontFacing != front)
                    continue;
                device = devices[i];
            }
        }
#endif
        public void Stop()
        {
            if (cam != null)
                cam.Stop();
            State = ECamState.Stoped;
        }
        public void Pause()
        {
            if (cam != null)
                cam.Stop();
            State = ECamState.Paused;
        }
        public void Resume()
        {
            if (cam != null)
                cam.Play();
            State = ECamState.Playing;
        }
        public Color32[] Pixels32 { get { return cam != null ? cam.GetPixels32() : new Color32[0]; } }
        public int Width { get { return cam != null ? cam.width : 0; } }
        public int Height { get { return cam != null ? cam.height : 0; } }
        public int Angle { get { return cam != null ? cam.videoRotationAngle : 0; } }
        public Texture Preview { get { return this.cam; } }
        public ECamPluginType PluginType { get { return ECamPluginType.WebCam; } }
        public ECamState State { get; private set; }
        public bool Front { get { return device.HasValue ? device.Value.isFrontFacing : false; } }
        public int Count { get { return WebCamTexture.devices.Length; } }
        public Color[] GetPixels(int x, int y, int width, int height) { return cam != null ? cam.GetPixels(x, y, width, height) : new Color[0]; }
    }
    public enum ECamPluginType
    {
        None,
        WebCam,
        NatCam,
    }
    public enum ECamState
    {
        None,
        Playing,
        Paused,
        Stoped,
    }
}