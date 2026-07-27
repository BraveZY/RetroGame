using System;
using System.Collections.Generic;
using UnityEngine;

namespace PoseAI
{
    /// <summary>
    /// 推理结果数据结构（精简版）
    /// 对应Python API返回的JSON格式
    /// </summary>
    [Serializable]
    public class PoseInferenceResult
    {
        public bool success;
        public bool detected;
        public string error;
        public ResultData result;
        public double timestamp;
        public List<ResultData> results;

        [Serializable]
        public class ResultDataWrapper
        {
            public ResultData result;
        }

        [Serializable]
        public class ResultData
        {
            public Landmark[] landmarks;
        }
    }
}

