using AOT;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Android;
public class PermissionTool : MonoBehaviour
{
    static IPermissionBase permission;
    static PermissionTool instance;
    static void Init()
    {
        if (instance == null)
            instance = FindObjectOfType<PermissionTool>();
        if (instance == null)
            instance = new GameObject("PermissionTool").AddComponent<PermissionTool>();
        DontDestroyOnLoad(instance.gameObject);
        if (permission == null)
        {
            if (Application.platform == RuntimePlatform.Android)
                permission = new AndroidPermission();
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
                permission = new IOSPermission();
            else
                permission = new DefaultPermission();
        }
    }
    public const int GRANTED = 1;
    public const int DENIED = 2;
    public const int DENIED_DONOTASKAGAIN = 3;
    public static void RequestCamera()
    {
        Init();
        permission.RequestCamera();
    }
    public static void RequestStorage()
    {
        Init();
        permission.RequestStorage();
    }
    public static void RequestMicrophone()
    {
        Init();
        permission.RequestMicrophone();
    }
    public static void RequestPhotoLibrary()
    {
        Init();
        permission.RequestPhotoLibrary();
    }
    public static bool HasCamera
    {
        get
        {
            Init();
            return permission.HasCamera;
        }
    }
    public static bool HasStorage
    {
        get
        {
            Init();
            return permission.HasStorage;
        }
    }
    public static bool HasMicrophone
    {
        get
        {
            Init();
            return permission.HasMicrophone;
        }
    }
    public static bool HasPhotoLibrary
    {
        get
        {
            Init();
            return permission.HasPhotoLibrary;
        }
    }
    public static Action<int> OnCameraResult;
    public static Action<int> OnMicrophoneResult;
    public static Action<int> OnPhotoLibraryResult;
    public static Action<int> OnStorageResult;
    public static void OpenSetting()
    {
        Init();
        permission.OpenSetting();
    }
    public void Update()
    {
        lock (typeof(PermissionTool))
        {
            for (int i = 0; i < callbackList.Count; i++)
            {
                if (callbackList[i] != null)
                    callbackList[i]();
            }
            callbackList.Clear();
        }
    }
    public static void OnMainThread(Action callback)
    {
        lock (typeof(PermissionTool))
        {
            if (callback != null)
                callbackList.Add(callback);
        }
    }
    public static List<Action> callbackList = new List<Action>();
    public interface IPermissionBase
    {
        void RequestCamera();
        void RequestMicrophone();
        void RequestPhotoLibrary();
        void RequestStorage();
        bool HasCamera { get; }
        bool HasMicrophone { get; }
        bool HasPhotoLibrary { get; }
        bool HasStorage { get; }
        void OpenSetting();
    }
    public class AndroidPermission : IPermissionBase
    {
        AndroidJavaObject context;
        AndroidJavaClass tool;
        public AndroidPermission()
        {
            context = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
            tool = new AndroidJavaClass("com.jxhy.permission.PermissionTool");
        }
        public void RequestCamera()
        {
            // tool.CallStatic("RequestCamera", context, new OnCameraCallback());
            UnityEngine.Android.Permission.RequestUserPermission(Permission.Camera);
        }
        public void RequestStorage() { tool.CallStatic("RequestStorage", context, new OnStorageCallback()); }
        public void RequestMicrophone() { tool.CallStatic("RequestMicrophone", context, new OnMicrophoneCallback()); }
        public void RequestPhotoLibrary() { }
        public bool HasCamera
        {
            get
            {
                // return true;
                return UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera);
            }
        }
        public bool HasMicrophone { get { return UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Microphone); } }
        public bool HasPhotoLibrary { get { return true; } }
        public bool HasStorage { get { return UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.ExternalStorageWrite) && UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.ExternalStorageRead); } }
        public class OnCameraCallback : AndroidJavaProxy
        {
            public OnCameraCallback() : base("com.jxhy.permission.PermissionTool$OnCameraCallback") { }
            public void OnResult(int result)
            {
                OnMainThread(() =>
                {
                    if (OnCameraResult != null)
                        OnCameraResult(result);
                });
            }
        }
        public class OnMicrophoneCallback : AndroidJavaProxy
        {
            public OnMicrophoneCallback() : base("com.jxhy.permission.PermissionTool$OnMicrophoneCallback") { }
            public void OnResult(int result)
            {
                OnMainThread(() =>
                {
                    if (OnMicrophoneResult != null)
                        OnMicrophoneResult(result);
                });
            }
        }
        public class OnStorageCallback : AndroidJavaProxy
        {
            public OnStorageCallback() : base("com.jxhy.permission.PermissionTool$OnStorageCallback") { }
            public void OnResult(int result)
            {
                OnMainThread(() =>
                {
                    if (OnStorageResult != null)
                        OnStorageResult(result);
                });
            }
        }
        public void OpenSetting() { tool.CallStatic("OpenSetting", context); }
    }
    public class IOSPermission : IPermissionBase
    {
        public void RequestCamera()
        {
#if UNITY_IOS || UNITY_IPHONE
            OnCameraCallbackDelegate cameraCallback = new OnCameraCallbackDelegate(OnCameraCallback);
            IntPtr cameraCallbackPtr = Marshal.GetFunctionPointerForDelegate(cameraCallback);
            requestCamera(cameraCallbackPtr);
#endif
        }
        public void RequestMicrophone()
        {
#if UNITY_IOS || UNITY_IPHONE
            OnCameraCallbackDelegate microphoneCallback = new OnCameraCallbackDelegate(OnMicrophoneCallback);
            IntPtr microphoneCallbackPtr = Marshal.GetFunctionPointerForDelegate(microphoneCallback);
            requestMicrophone(microphoneCallbackPtr);
#endif
        }
        public void RequestPhotoLibrary()
        {
#if UNITY_IOS || UNITY_IPHONE
            OnCameraCallbackDelegate photoLibraryCallback = new OnCameraCallbackDelegate(OnPhotoLibraryCallback);
            IntPtr photoLibraryCallbackPtr = Marshal.GetFunctionPointerForDelegate(photoLibraryCallback);
            requestPhotoLibrary(photoLibraryCallbackPtr);
#endif
        }
        public void RequestStorage() { }
        public bool HasCamera
        {
            get
            {
#if UNITY_IOS || UNITY_IPHONE
                return checkCamera() == 1;
#endif
                return true;
            }
        }
        public bool HasMicrophone
        {
            get
            {
#if UNITY_IOS || UNITY_IPHONE
                return checkMicrophone() == 1;
#endif
                return true;
            }
        }
        public bool HasPhotoLibrary
        {
            get
            {
#if UNITY_IOS || UNITY_IPHONE
                return checkPhotoLibrary() == 1;
#endif 
                return true;
            }
        }
        public bool HasStorage { get { return true; } }
        public void OpenSetting()
        {
#if UNITY_IOS || UNITY_IPHONE
            openSetting();
#endif
        }
#if UNITY_IOS || UNITY_IPHONE
        [DllImport("__Internal", EntryPoint = "requestCamera")]
        static extern void requestCamera(IntPtr cameraCallback);
        [DllImport("__Internal", EntryPoint = "requestMicrophone")]
        static extern void requestMicrophone(IntPtr microphoneCallback);
        [DllImport("__Internal", EntryPoint = "requestPhotoLibrary")]
        static extern void requestPhotoLibrary(IntPtr photoLibraryCallback);
        [DllImport("__Internal", EntryPoint = "checkCamera")]
        static extern int checkCamera();
        [DllImport("__Internal", EntryPoint = "checkMicrophone")]
        static extern int checkMicrophone();
        [DllImport("__Internal", EntryPoint = "checkPhotoLibrary")]
        static extern int checkPhotoLibrary();
        [DllImport("__Internal", EntryPoint = "openSetting")]
        static extern int openSetting();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnCameraCallbackDelegate(int result);
        [MonoPInvokeCallback(typeof(OnCameraCallbackDelegate))]
        public static void OnCameraCallback(int result)
        {
            OnMainThread(() =>
            {
                if (OnCameraResult != null)
                    OnCameraResult(result);
            });
        }
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnMicrophoneCallbackDelegate(int result);
        [MonoPInvokeCallback(typeof(OnMicrophoneCallbackDelegate))]
        public static void OnMicrophoneCallback(int result)
        {
            OnMainThread(() =>
            {
                if (OnMicrophoneResult != null)
                    OnMicrophoneResult(result);
            });
        }
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void OnPhotoLibraryCallbackDelegate(int result);
        [MonoPInvokeCallback(typeof(OnPhotoLibraryCallbackDelegate))]
        public static void OnPhotoLibraryCallback(int result)
        {
            OnMainThread(() =>
            {
                if (OnPhotoLibraryResult != null)
                    OnPhotoLibraryResult(result);
            });
        }
#endif
    }
    public class DefaultPermission : IPermissionBase
    {
        public void RequestCamera() { }
        public void RequestStorage() { }
        public void RequestMicrophone() { }
        public void RequestPhotoLibrary() { }
        public bool HasCamera { get { return true; } }
        public bool HasMicrophone { get { return true; } }
        public bool HasPhotoLibrary { get { return true; } }
        public bool HasStorage { get { return true; } }
        public void OpenSetting() { }
    }
}