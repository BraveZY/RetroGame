using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameCoreRuntime
{
    public class DemoView : MonoBehaviour
    {

        public GameObject[] p1Gos;
        public GameObject[] p2Gos;

        private void Start()
        {
            GameCore.Pose.OnIDModeChanged += OnIDModeChanged;
            Display(p1Gos, false);
            Display(p2Gos, false);
            OnIDModeChanged(GameCore.Pose.IDMode);
        }

        private void OnDestroy()
        {
            GameCore.Pose.OnIDModeChanged -= OnIDModeChanged;
        }

        private void OnIDModeChanged(AllocateIDMode idMode)
        {
            switch (idMode)
            {
                case AllocateIDMode.SINGLE:
                    Display(p1Gos, true);
                    Display(p2Gos, false);
                    break;
                case AllocateIDMode.DOUBLE:
                    Display(p1Gos, true);
                    Display(p2Gos, true);
                    break;
                default:
                    break;
            }
        }

        private void Display(GameObject[] gos, bool isDisplay)
        {
            foreach (var go in gos)
            {
                go.SetActive(isDisplay);
            }
        }

    }
}