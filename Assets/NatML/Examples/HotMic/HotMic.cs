/* 
*   NatDevice
*   Copyright (c) 2022 Yusuf Olokoba.
*/

namespace NatSuite.Examples {
    
    using UnityEngine;
    using NatSuite.Devices;
    using NatSuite.Devices.Outputs;

    public class HotMic : MonoBehaviour {
        
        AudioDevice device;
        AudioClipOutput clipOutput;
        
        async void Start () {
            // Request mic permissions
            if (await MediaDeviceQuery.RequestPermissions<AudioDevice>() != PermissionStatus.Authorized) {
                Debug.LogError("User did not grant microphone permissions");
                return;
            }
            // Get the audio device
            var query = new MediaDeviceQuery(MediaDeviceCriteria.AudioDevice);
            device = query.current as AudioDevice;
            Debug.Log($"{device}");
        }

        public void StartRecording () {
            clipOutput = new AudioClipOutput();
            device.StartRunning(clipOutput);
        }

        public void StopRecording () {
            // Stop recording
            device.StopRunning();
            // Get the audio clip and dispose the output
            var audioClip = clipOutput.ToClip();
            clipOutput.Dispose();
            // Playback the recording
            AudioSource.PlayClipAtPoint(audioClip, Vector3.zero);
        }
    }
}