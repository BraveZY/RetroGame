using UnityEngine;

namespace PoseAI
{
    /// <summary>
    /// 网球推理配置类
    /// 对应 Python 端 app_config.py 的配置参数
    /// </summary>
    public static class TennisConfig
    {
        // 类别数量开关：设置为 true 启用6类（包含扣球），false 为5类（与当前模型一致）
        public const bool EnableSmashClass = false;

        // 标签名称映射（根据 EnableSmashClass 动态构建）
        private static readonly string[] BaseLabelNames = new string[]
        {
            "IDLE",
            "BACKSWING_FOREHAND",
            "HIT_FOREHAND",
            "BACKSWING_BACKHAND",
            "HIT_BACKHAND"
        };

        private static readonly string[] LabelNamesWithSmash = new string[]
        {
            "IDLE",
            "BACKSWING_FOREHAND",
            "HIT_FOREHAND",
            "BACKSWING_BACKHAND",
            "HIT_BACKHAND",
            "SMASH_FOREHAND"
        };

        public static string[] LabelNames => EnableSmashClass ? LabelNamesWithSmash : BaseLabelNames;

        // 状态机阈值配置（默认推荐配置）
        // 注意：速度阈值已更新为基于真实速度（单位：归一化单位/秒），而非位移量
        public static class StateMachineThresholds
        {
            // IDLE -> PREP 转换阈值
            public const float IdleToPrepModelConf = 0.5f;
            public const float IdleToPrepSpeed = 0.75f;  // 单位：归一化单位/秒（原值0.025位移 * 30fps ≈ 0.75）

            // PREP -> SWING 转换阈值
            public const float PrepToSwingModelConf = 0.5f;
            public const float PrepToSwingSpeed = 1.8f;  // 单位：归一化单位/秒（原值0.06位移 * 30fps ≈ 1.8）

            // SWING -> HIT 转换阈值
            public const float SwingToHitModelConf = 0.6f;
            public const float SwingToHitSpeedDecay = 0.5f;  // 速度衰减比例（0.5 表示衰减到 50%）
            public const float SwingToHitPeakSpeed = 1.8f;  // 单位：归一化单位/秒（原值0.06位移 * 30fps ≈ 1.8）

            // SMASH 扣球识别阈值
            public const float SmashModelConf = 0.5f;

            // TRANSITION 过渡状态识别阈值
            public const float TransitionConfThreshold = 0.4f;  // 过渡状态模型置信度上限
            public const float TransitionSpeedMin = 0.5f;  // 过渡状态最小速度要求（单位：归一化单位/秒）
            public const float TransitionSpeedMax = 2.5f;  // 过渡状态最大速度要求
            public const float TransitionDurationMax = 0.3f;  // 过渡状态最大持续时间（秒）
        }

        // 动作识别阈值
        public const float PrepYMinThreshold = 0.1f;
        public const float HitYMinThreshold = -0.5f;  // 放宽以允许采集完整动作流程
        public const float HitXMaxAbsThreshold = 0.5f;

        // 实时推理生命周期参数
        public const float HitCooldown = 0.35f;  // 状态锁定冷却时间（秒）
        public const int TrailMaxLen = 10;       // 拖尾历史帧数

        // 评分系数配置
        public static class ScoringThresholds
        {
            public const float SwingKFactor = 0.08f;  // 挥拍非线性系数
            public const float SwingMinSpeed = 0.5f;  // 挥拍起评分速度（Deadzone）
            public const float SmashKFactor = 0.06f;  // 扣球非线性系数
            public const float SmashMinSpeed = 1.0f;  // 扣球起评分速度（Deadzone）
            public const float SmashHeightCoeff = 42.8f;  // 扣球高度系数
            public const float StabilityPenaltyCoeff = 20.0f;  // 稳定性扣分系数
        }

        // 模型配置
        public const int InputSize = 24;         // 输入特征大小：12个关键点 × 2坐标
        public static int NumClasses => EnableSmashClass ? 6 : 5;  // 分类类别数：根据 EnableSmashClass 动态设置
    }
}

