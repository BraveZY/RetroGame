using UnityEngine;

namespace MotionDodgeball.Gameplay
{
    /// <summary>
    /// 表示单人回合当前走到哪一步。
    /// </summary>
    public enum RoundPhase
    {
        Ready,
        Countdown,
        Playing,
        Result
    }

    /// <summary>
    /// 管住一局单人躲避玩法从准备、倒计时、游玩到结算的规则状态。
    ///
    /// 职责：
    /// - 根据配置初始化倒计时、回合时长和生命数。
    /// - 推进回合阶段，并在时间耗尽或生命归零时进入结算。
    /// - 记录躲避、命中和结算原因，供界面与测试读取。
    /// - 根据表现给出简短评级，不直接操作场景对象。
    /// </summary>
    public sealed class SinglePlayerReadyFlow
    {
        private readonly float countdownSeconds;
        private readonly float roundSeconds;
        private readonly int startingLives;

        public RoundPhase Phase { get; private set; } = RoundPhase.Ready;
        public float RemainingCountdownSeconds { get; private set; }
        public float RemainingRoundSeconds { get; private set; }
        public int Lives { get; private set; }
        public int DodgeCount { get; private set; }
        public int HitCount { get; private set; }
        public string ResultReason { get; private set; } = string.Empty;
        public string Rating => DodgeCount >= 12 && Lives >= 2 ? "A" : DodgeCount >= 6 && Lives >= 1 ? "B" : "C";

        public SinglePlayerReadyFlow(float countdownSeconds = 3f, float roundSeconds = 60f, int startingLives = 3)
        {
            this.countdownSeconds = Mathf.Max(0f, countdownSeconds);
            this.roundSeconds = Mathf.Max(1f, roundSeconds);
            this.startingLives = Mathf.Max(1, startingLives);

            Reset();
        }

        /// <summary>把回合恢复到可重新开始的准备状态，并清空本局统计。</summary>
        public void Reset()
        {
            Phase = RoundPhase.Ready;
            RemainingCountdownSeconds = countdownSeconds;
            RemainingRoundSeconds = roundSeconds;
            Lives = startingLives;
            DodgeCount = 0;
            HitCount = 0;
            ResultReason = string.Empty;
        }

        /// <summary>从准备态进入倒计时；没有倒计时时直接进入游玩态。</summary>
        public void StartRound()
        {
            if (Phase != RoundPhase.Ready)
            {
                return;
            }

            Phase = countdownSeconds > 0f ? RoundPhase.Countdown : RoundPhase.Playing;
            RemainingCountdownSeconds = countdownSeconds;
            RemainingRoundSeconds = roundSeconds;
        }

        /// <summary>按经过时间推进倒计时或回合计时，并处理自动结算。</summary>
        public void Tick(float deltaSeconds)
        {
            var delta = Mathf.Max(0f, deltaSeconds);
            if (delta <= 0f)
            {
                return;
            }

            if (Phase == RoundPhase.Countdown)
            {
                RemainingCountdownSeconds = Mathf.Max(0f, RemainingCountdownSeconds - delta);
                if (RemainingCountdownSeconds <= 0f)
                {
                    Phase = RoundPhase.Playing;
                    RemainingRoundSeconds = roundSeconds;
                }

                return;
            }

            if (Phase == RoundPhase.Playing)
            {
                RemainingRoundSeconds = Mathf.Max(0f, RemainingRoundSeconds - delta);
                if (RemainingRoundSeconds <= 0f)
                {
                    FinishRound("Time Up");
                }
            }
        }

        public void RecordDodge()
        {
            if (Phase == RoundPhase.Playing)
            {
                DodgeCount++;
            }
        }

        /// <summary>记录一次命中并扣除生命；生命耗尽时提前结束本局。</summary>
        public void RecordHit()
        {
            if (Phase != RoundPhase.Playing)
            {
                return;
            }

            HitCount++;
            Lives = Mathf.Max(0, Lives - 1);
            if (Lives <= 0)
            {
                FinishRound("No Lives");
            }
        }

        /// <summary>用指定原因结束本局；重复调用不会覆盖已进入的结算态。</summary>
        public void FinishRound(string reason)
        {
            if (Phase == RoundPhase.Result)
            {
                return;
            }

            Phase = RoundPhase.Result;
            RemainingCountdownSeconds = 0f;
            RemainingRoundSeconds = Mathf.Max(0f, RemainingRoundSeconds);
            ResultReason = string.IsNullOrWhiteSpace(reason) ? "Round Complete" : reason;
        }
    }
}
