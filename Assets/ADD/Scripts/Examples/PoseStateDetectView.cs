// using System;
// using System.Collections;
// using System.Collections.Generic;
// using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
// using UnityEngine;
// using UnityEngine.UI;
//
// namespace GameCoreRuntime
// {
//     public class PoseStateDetectView : MonoBehaviour
//     {
//
//         public int area;
//         public PoseState poseState;
//         public Slider progressSlider;
//
//         private void OnEnable()
//         {
//             GameCore.Pose.OnPoseStateUpdated += OnPoseStateUpdated;
//         }
//
//         private void OnDisable()
//         {
//             GameCore.Pose.OnPoseStateUpdated -= OnPoseStateUpdated;
//         }
//
//
//         private void OnPoseStateUpdated(int area, PoseState poseState, float progress)
//         {
//             if (GameCore.Pose.IDMode == AllocateIDMode.MULTI) return;
//             if (this.area == area)
//             {
//                 if (this.poseState == poseState)
//                 {
//                     progressSlider.value = progress;
//                 }
//             }
//         }
//     }
// }