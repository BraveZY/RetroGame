/* 
*   NatDevice
*   Copyright (c) 2022 NatML Inc. All Rights Reserved.
*/

namespace NatSuite.Examples {
    
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using System.Threading.Tasks;
    using NatSuite.Devices;
    using NatSuite.Devices.Outputs;

    public class MiniCam : MonoBehaviour {
        
        #region --Inspector--
        [Header(@"Preview")]
        public RawImage rawImage;
        public AspectRatioFitter aspectFitter;

        [Header(@"Buttons")]
        public CanvasGroup buttons;
        public Image flashIcon;
        public Image switchIcon;
        #endregion
        

        #region --Setup--
        MediaDeviceQuery query;
        CameraDevice cameraDevice => query?.current as CameraDevice;
        TextureOutput previewTextureOutput;

        async void Start () {
            // Request camera permissions
            var permissionStatus = await MediaDeviceQuery.RequestPermissions<CameraDevice>();
            if (permissionStatus != PermissionStatus.Authorized) {
                Debug.LogError("User did not grant camera permissions");
                return;
            }
            // Get a camera device
            query = new MediaDeviceQuery(MediaDeviceCriteria.CameraDevice);
            // Start camera preview
            previewTextureOutput = new TextureOutput();
            cameraDevice.StartRunning(previewTextureOutput);
            var previewTexture = await previewTextureOutput;
            Debug.Log($"Started camera preview with resolution {previewTexture.width}x{previewTexture.height}");
            // Display preview texture
            rawImage.texture = previewTexture;
            aspectFitter.aspectRatio = (float)previewTexture.width / previewTexture.height;
            // Set UI state
            switchIcon.color = query.count > 1 ? Color.white : Color.gray;
            flashIcon.color = cameraDevice.flashSupported ? Color.white : Color.gray;
        }

        void OnDestroy () {
            // Stop the camera preview
            if (cameraDevice?.running ?? false)
                cameraDevice.StopRunning();
            // Dispose the preview texture output
            previewTextureOutput?.Dispose();
        }
        #endregion


        #region --UI Handlers--

        public async void CapturePhoto () {
            // Capture photo
            using var photoTextureOutput = new TextureOutput();
            cameraDevice.CapturePhoto(photoTextureOutput);
            var photoTexture = await photoTextureOutput;
            Debug.Log($"Captured photo with resolution {photoTexture.width}x{photoTexture.height}");
            // Hide buttons
            buttons.alpha = 0f;
            buttons.interactable = false;
            // Display photo texture
            rawImage.texture = photoTexture;
            aspectFitter.aspectRatio = (float)photoTexture.width / photoTexture.height;
            // Wait a few seconds
            await Task.Delay(3_000);
            // Restore preview
            rawImage.texture = previewTextureOutput.texture;
            aspectFitter.aspectRatio = (float)previewTextureOutput.texture.width / previewTextureOutput.texture.height;
            buttons.alpha = 1f;
            buttons.interactable = true;
        }

        public async void SwitchCamera () {
            // Check that there is another camera to switch to
            if (query.count < 2)
                return;
            // Stop current camera
            cameraDevice.StopRunning();
            // Advance to next available camera // The value of `cameraDevice` will automatically reflect this
            query.Advance();
            // Create new texture output
            previewTextureOutput.Dispose();
            previewTextureOutput = new TextureOutput();
            // Start new camera
            cameraDevice.StartRunning(previewTextureOutput);
            var previewTexture = await previewTextureOutput;
            // Display preview texture
            rawImage.texture = previewTexture;
            aspectFitter.aspectRatio = (float)previewTexture.width / previewTexture.height;
        }

        public void FocusCamera (BaseEventData e) {
            // Check if focus is supported
            if (!cameraDevice.focusPointSupported)
                return;
            // Get the touch position in viewport coordinates
            var eventData = e as PointerEventData;
            var transform = eventData.pointerPress.GetComponent<RectTransform>();
            if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(
                transform,
                eventData.pressPosition,
                eventData.pressEventCamera,
                out var worldPoint
            ))
                return;
            var corners = new Vector3[4];
            transform.GetWorldCorners(corners);
            var point = worldPoint - corners[0];
            var size = new Vector2(corners[3].x, corners[1].y) - (Vector2)corners[0];
            // Focus camera at point
            cameraDevice.SetFocusPoint(point.x / size.x, point.y / size.y);
        }

        public void ToggleFlashMode () {
            // Check if flash is supported
            if (!cameraDevice.flashSupported)
                return;
            // Toggle
            if (cameraDevice.flashMode == CameraDevice.FlashMode.On) {
                cameraDevice.flashMode = CameraDevice.FlashMode.Off;
                flashIcon.color = Color.gray;
            } else {
                cameraDevice.flashMode = CameraDevice.FlashMode.On;
                flashIcon.color = Color.white;
            }
        }
        #endregion
    }
}