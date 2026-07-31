using System;

namespace MotionDodgeball.Gameplay
{
    /// <summary>本地一局中参与的玩家人数。</summary>
    public enum DodgeballMode { SinglePlayer = 1, TwoPlayers = 2, ThreePlayers = 3 }

    /// <summary>投手 A 或 B 当前由谁控制。</summary>
    public enum ThrowerControl { Ai, PlayerTwo, PlayerThree }

    /// <summary>一局结算时显示给玩家的获胜阵营。</summary>
    public enum DodgeballWinningTeam { None, Dodger, Throwers }

    /// <summary>
    /// 管住一局丢沙包从选人数到结算的规则，不直接操作场景里的角色。
    ///
    /// 职责：
    /// - 按 1、2、3 人模式分配两端投手由玩家或 AI 控制。
    /// - 用 3 秒倒计时、100 秒限时和 3 点生命推进一局比赛。
    /// - 汇总躲避、投手命中、胜负与 C 的生存评级，供 HUD 和场景控制器读取。
    /// </summary>
    public sealed class DodgeballSession
    {
        // ----------------------------------------------------------------------------
        // 固定规则与对外状态：场景只读取这些值，不在这里保存角色或沙包对象
        // ----------------------------------------------------------------------------
        private readonly float countdownSeconds;
        private readonly float roundSeconds;
        private readonly int startingLives;

        public DodgeballMode Mode { get; private set; }
        public RoundPhase Phase { get; private set; } = RoundPhase.Ready;
        public float RemainingCountdownSeconds { get; private set; }
        public float RemainingRoundSeconds { get; private set; }
        public int Lives { get; private set; }
        public int DodgeCount { get; private set; }
        public int ThrowerTeamHitCount { get; private set; }
        public string ResultReason { get; private set; } = string.Empty;
        public DodgeballWinningTeam WinningTeam { get; private set; }
        public string Rating => WinningTeam == DodgeballWinningTeam.Dodger
            ? Lives == startingLives ? "A" : "B"
            : Phase == RoundPhase.Result ? "C" : string.Empty;
        public string BreakTitle => Rating switch
        {
            "A" => "课间小冠军",
            "B" => "躲闪小能手",
            "C" => "再来一局",
            _ => string.Empty
        };

        public DodgeballSession(
            DodgeballMode mode = DodgeballMode.SinglePlayer,
            float countdownSeconds = 3f,
            float roundSeconds = 100f,
            int startingLives = 3)
        {
            Mode = mode;
            this.countdownSeconds = Math.Max(0f, countdownSeconds);
            this.roundSeconds = Math.Max(1f, roundSeconds);
            this.startingLives = Math.Max(1, startingLives);
            Reset();
        }

        /// <summary>返回当前模式中指定投手端的实际控制者。</summary>
        public ThrowerControl GetThrowerControl(bool isThrowerA)
        {
            if (Mode == DodgeballMode.SinglePlayer)
            {
                return ThrowerControl.Ai;
            }

            if (Mode == DodgeballMode.TwoPlayers)
            {
                return isThrowerA ? ThrowerControl.PlayerTwo : ThrowerControl.Ai;
            }

            return isThrowerA ? ThrowerControl.PlayerTwo : ThrowerControl.PlayerThree;
        }

        // ----------------------------------------------------------------------------
        // 选局与重开：仅准备态可换人数，重开保留已选人数供同局再来一轮
        // ----------------------------------------------------------------------------
        /// <summary>仅在准备态切换人数，避免比赛中改变投手归属。</summary>
        public void SelectMode(DodgeballMode mode)
        {
            if (Phase == RoundPhase.Ready)
            {
                Mode = mode;
            }
        }

        /// <summary>清空一局统计并保留已选人数，供同模式重新开始。</summary>
        public void Reset()
        {
            Phase = RoundPhase.Ready;
            RemainingCountdownSeconds = countdownSeconds;
            RemainingRoundSeconds = roundSeconds;
            Lives = startingLives;
            DodgeCount = 0;
            ThrowerTeamHitCount = 0;
            ResultReason = string.Empty;
            WinningTeam = DodgeballWinningTeam.None;
        }

        /// <summary>从准备态进入倒计时；没有倒计时配置时直接开始比赛。</summary>
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

        // ----------------------------------------------------------------------------
        // 对局推进与结算：时间耗尽由 C 获胜，第三次命中由投手阵营获胜
        // ----------------------------------------------------------------------------
        /// <summary>推进倒计时或比赛计时，并在 100 秒结束时结算躲避者胜利。</summary>
        public void Tick(float deltaSeconds)
        {
            var delta = Math.Max(0f, deltaSeconds);
            if (delta <= 0f)
            {
                return;
            }

            if (Phase == RoundPhase.Countdown)
            {
                RemainingCountdownSeconds = Math.Max(0f, RemainingCountdownSeconds - delta);
                if (RemainingCountdownSeconds <= 0f)
                {
                    Phase = RoundPhase.Playing;
                    RemainingRoundSeconds = roundSeconds;
                }

                return;
            }

            if (Phase == RoundPhase.Playing)
            {
                RemainingRoundSeconds = Math.Max(0f, RemainingRoundSeconds - delta);
                if (RemainingRoundSeconds <= 0f)
                {
                    FinishRound(DodgeballWinningTeam.Dodger, "Time Up");
                }
            }
        }

        public void RecordDodge()
        {
            if (Phase == RoundPhase.Playing) DodgeCount++;
        }

        /// <summary>记录一次投手阵营命中；第三次命中会立刻结束本局。</summary>
        public void RecordHit()
        {
            if (Phase != RoundPhase.Playing)
            {
                return;
            }

            ThrowerTeamHitCount++;
            Lives = Math.Max(0, Lives - 1);
            if (Lives <= 0)
            {
                FinishRound(DodgeballWinningTeam.Throwers, "No Lives");
            }
        }

        private void FinishRound(DodgeballWinningTeam winner, string reason)
        {
            if (Phase == RoundPhase.Result)
            {
                return;
            }
            Phase = RoundPhase.Result;
            RemainingCountdownSeconds = 0f;
            ResultReason = reason;
            WinningTeam = winner;
        }
    }
}
