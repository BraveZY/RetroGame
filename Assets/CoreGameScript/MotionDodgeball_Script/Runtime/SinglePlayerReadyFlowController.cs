using UnityEngine;

namespace MotionDodgeball.Gameplay
{
    /// <summary>
    /// 把本地 1–3 人躲避回合规则接到场景里的角色、沙包和 HUD 上。
    ///
    /// 职责：
    /// - 启动和重开一局，并把核心规则状态交给 DodgeballSession 管理。
    /// - 读取键盘输入，让躲避者移动、投掷玩家瞄准并出手。
    /// - 驱动两名投掷者预警、出手、接球和沙包命中判定。
    /// - 每帧刷新场景文字，让测试样例能直接看到阶段、时间和结算。
    /// </summary>
    public sealed class SinglePlayerReadyFlowController : MonoBehaviour
    {
        // ----------------------------------------------------------------------------
        // 场景绑定与玩法参数：Inspector 可直接替换文字、角色、沙包容器和节奏数值
        // ----------------------------------------------------------------------------
        [SerializeField] private TextMesh statusText;
        [SerializeField] private TextMesh timerText;
        [SerializeField] private TextMesh livesText;
        [SerializeField] private TextMesh dodgeText;
        [SerializeField] private TextMesh hitText;
        [SerializeField] private TextMesh inputText;
        [SerializeField] private TextMesh resultText;
        [SerializeField] private Transform dodger;
        [SerializeField] private Transform throwerA;
        [SerializeField] private Transform throwerB;
        [SerializeField] private Transform projectileRoot;
        [SerializeField] private float playerSpeed = 3.4f;
        [SerializeField] private Vector2 dodgeZoneHalfExtents = new Vector2(2.05f, 1.05f);
        [SerializeField] private float throwIntervalSeconds = 1.8f;
        [SerializeField] private float warningSeconds = 0.45f;
        [SerializeField] private float sandbagSpeed = 4.4f;
        [SerializeField] private float throwerWindupOffset = 0.26f;
        [SerializeField] private float throwerAimSpeed = 3.2f;
        [SerializeField] private float throwerLaneHalfWidth = 3.55f;
        [SerializeField] private float catchDistance = 0.32f;
        [SerializeField] private float catchBoundaryY = 2.95f;
        [SerializeField] private float flightTimeoutSeconds = 3f;
        [SerializeField] private float catchTimeoutSeconds = 2.6f;

        // ----------------------------------------------------------------------------
        // 运行时状态：连接纯规则流、投掷预警、沙包归属和场景对象缓存
        // ----------------------------------------------------------------------------
        private readonly DodgeballSession session = new DodgeballSession();
        private float throwTimer;
        private float sandbagStateTimer;
        private GameObject warningObject;
        private GameObject sandbagObject;
        private Vector3 sandbagDirection;
        private Vector3 catchTargetPosition;
        private ThrowerId currentHolder = ThrowerId.A;
        private ThrowerId resetHolder = ThrowerId.A;
        private SandbagState sandbagState = SandbagState.Held;
        private bool sandbagHit;
        private Vector3 throwerAStartPosition;
        private Vector3 throwerBStartPosition;
        private Vector3 throwerAStartScale;
        private Vector3 throwerBStartScale;
        private const float DodgerHitDistance = 0.48f;
        private const float SandbagHoldOffset = 0.42f;

        public DodgeballSession Session => session;

        // ----------------------------------------------------------------------------
        // Unity 生命周期：开局解析场景引用，每帧处理重开、开局、规则推进和 HUD 刷新
        // ----------------------------------------------------------------------------
        private void Start()
        {
            ResolveSceneReferences();
            ResetRuntimeObjects();
            RefreshHud();
        }

        private void Update()
        {
            if (session.Phase == RoundPhase.Result && Input.GetKeyDown(KeyCode.Escape))
            {
                ReturnToModeSelection();
                return;
            }

            if (session.Phase == RoundPhase.Result && Input.GetKeyDown(KeyCode.R))
            {
                RestartRound();
                return;
            }

            if (session.Phase == RoundPhase.Ready)
            {
                SelectKeyboardMode();
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    BeginCountdown();
                }
            }

            session.Tick(Time.deltaTime);
            if (session.Phase == RoundPhase.Playing)
            {
                UpdatePlayerMovement(Time.deltaTime);
                UpdateThrowers(Time.deltaTime);
                UpdateSandbag(Time.deltaTime);
            }

            RefreshHud();
        }

        // ----------------------------------------------------------------------------
        // 场景引用与玩家移动：缺失引用时按占位物名称兜底，并把躲避者限制在场地内
        // ----------------------------------------------------------------------------
        private void ResolveSceneReferences()
        {
            if (dodger == null)
            {
                dodger = GameObject.Find("Dodger_C_Placeholder")?.transform;
            }

            if (throwerA == null)
            {
                throwerA = GameObject.Find("Thrower_A_Placeholder")?.transform;
            }

            if (throwerB == null)
            {
                throwerB = GameObject.Find("Thrower_B_Placeholder")?.transform;
            }

            if (projectileRoot == null)
            {
                projectileRoot = new GameObject("Runtime_Sandbags").transform;
            }

            projectileRoot.SetParent(transform.root, false);

            throwerAStartPosition = throwerA != null ? throwerA.position : Vector3.zero;
            throwerBStartPosition = throwerB != null ? throwerB.position : Vector3.zero;
            throwerAStartScale = throwerA != null ? throwerA.localScale : Vector3.one;
            throwerBStartScale = throwerB != null ? throwerB.localScale : Vector3.one;
        }

        private void UpdatePlayerMovement(float deltaSeconds)
        {
            if (dodger == null)
            {
                SetText(inputText, "Input: no dodger");
                return;
            }

            var move = new Vector2(KeyboardDodgeballInput.GetDodgerAxis(), 0f);

            var position = dodger.position;
            var next = new Vector3(
                position.x + move.x * playerSpeed * deltaSeconds,
                position.y + move.y * playerSpeed * deltaSeconds,
                position.z);

            var clamped = new Vector3(
                Mathf.Clamp(next.x, -dodgeZoneHalfExtents.x, dodgeZoneHalfExtents.x),
                Mathf.Clamp(next.y, -dodgeZoneHalfExtents.y, dodgeZoneHalfExtents.y),
                next.z);

            dodger.position = clamped;
            var atBoundary = Mathf.Abs(next.x - clamped.x) > 0.001f || Mathf.Abs(next.y - clamped.y) > 0.001f;
            SetText(inputText, atBoundary ? "Input: boundary" : move.sqrMagnitude > 0f ? "Input: active" : "Input: idle");
        }

        // ----------------------------------------------------------------------------
        // 投掷者节奏：从持球到预警再到出手，接球阶段则让接球者追向落点
        // ----------------------------------------------------------------------------
        private void UpdateThrowers(float deltaSeconds)
        {
            if (sandbagState == SandbagState.Warning)
            {
                sandbagStateTimer += deltaSeconds;
                UpdateCurrentHolderAim(deltaSeconds);
                AnimateActiveThrower(Mathf.Clamp01(sandbagStateTimer / warningSeconds));
                UpdateWarning();
                if ((IsCurrentHolderAi() && sandbagStateTimer >= warningSeconds)
                    || (IsCurrentHolderHuman() && IsCurrentHolderThrowPressed()))
                {
                    HideWarning();
                    ReleaseSandbag();
                    ResetThrowerPulse();
                }

                return;
            }

            ResetThrowerPulse();

            if (sandbagState == SandbagState.Held)
            {
                AttachSandbagToHolder();
                UpdateCurrentHolderAim(deltaSeconds);
                throwTimer -= deltaSeconds;
                if (throwTimer <= warningSeconds)
                {
                    SetSandbagState(SandbagState.Warning);
                    ShowWarning();
                }

                return;
            }

            if (sandbagState == SandbagState.Catching)
            {
                MoveReceiverTowardCatchTarget(deltaSeconds);
            }
        }

        // ----------------------------------------------------------------------------
        // 沙包飞行与命中：飞向躲避者后判定受击，越过对侧则进入接球和下一轮
        // ----------------------------------------------------------------------------
        private void UpdateSandbag(float deltaSeconds)
        {
            if (sandbagObject == null)
            {
                return;
            }

            if (sandbagState == SandbagState.Held)
            {
                AttachSandbagToHolder();
                return;
            }

            if (sandbagState == SandbagState.FlyingToDodger)
            {
                sandbagStateTimer += deltaSeconds;
                sandbagObject.transform.position += sandbagDirection * sandbagSpeed * deltaSeconds;
                if (IsSandbagOutOfBounds() || sandbagStateTimer >= flightTimeoutSeconds)
                {
                    BeginReset(GetReceiver(currentHolder));
                    return;
                }

                if (!sandbagHit && dodger != null && Vector3.Distance(sandbagObject.transform.position, dodger.position) < DodgerHitDistance)
                {
                    sandbagHit = true;
                    SetSandbagState(SandbagState.Hit);
                    return;
                }

                if (HasReachedReceiverSide())
                {
                    BeginCatching();
                }

                return;
            }

            if (sandbagState == SandbagState.Catching)
            {
                sandbagStateTimer += deltaSeconds;
                var nextPosition = Vector3.MoveTowards(sandbagObject.transform.position, catchTargetPosition, sandbagSpeed * deltaSeconds);
                sandbagObject.transform.position = nextPosition;

                var receiver = GetThrower(GetReceiver(currentHolder));
                if (receiver == null || IsSandbagOutOfBounds() || sandbagStateTimer >= catchTimeoutSeconds)
                {
                    BeginReset(GetReceiver(currentHolder));
                    return;
                }

                if (Vector3.Distance(receiver.position, catchTargetPosition) <= catchDistance
                    && Vector3.Distance(sandbagObject.transform.position, catchTargetPosition) <= catchDistance)
                {
                    SetSandbagState(SandbagState.Caught);
                }

                return;
            }

            if (sandbagState == SandbagState.Hit)
            {
                session.RecordHit();
                BeginReset(GetReceiver(currentHolder));
                return;
            }

            if (sandbagState == SandbagState.Caught)
            {
                session.RecordDodge();
                BeginReset(GetReceiver(currentHolder));
                return;
            }

            if (sandbagState == SandbagState.Reset)
            {
                CompleteReset();
            }
        }

        // ----------------------------------------------------------------------------
        // 回合重置与投掷者定位：重开时恢复角色、沙包、预警和投掷者初始姿态
        // ----------------------------------------------------------------------------
        private void ShowWarning()
        {
            if (warningObject == null)
            {
                warningObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            }

            warningObject.name = "Throw_Warning";
            warningObject.transform.SetParent(transform.root, false);
            warningObject.GetComponent<Renderer>().material.color = new Color(1f, 0.86f, 0.1f, 0.85f);
            warningObject.SetActive(true);
            UpdateWarning();
        }

        private void HideWarning()
        {
            if (warningObject != null)
            {
                warningObject.SetActive(false);
            }
        }

        private void RestartRound()
        {
            session.Reset();
            ResetRuntimeObjects();
            BeginCountdown();
            RefreshHud();
        }

        private void ReturnToModeSelection()
        {
            session.Reset();
            ResetRuntimeObjects();
            RefreshHud();
        }

        private void BeginCountdown()
        {
            session.StartRound();
            if (session.Phase == RoundPhase.Ready)
            {
                return;
            }

            currentHolder = ThrowerId.A;
            SetSandbagState(SandbagState.Warning);
            throwTimer = warningSeconds;
            AttachSandbagToHolder();
            ShowWarning();
        }

        private void ResetRuntimeObjects()
        {
            HideWarning();
            sandbagStateTimer = 0f;
            throwTimer = 0f;
            currentHolder = ThrowerId.A;
            resetHolder = ThrowerId.A;
            sandbagState = SandbagState.Held;
            sandbagHit = false;

            if (dodger != null)
            {
                dodger.position = new Vector3(0f, 0f, dodger.position.z);
            }

            ResetThrowerPose();
            EnsureSandbagObject();
            AttachSandbagToHolder();
        }

        private Transform GetActiveThrower()
        {
            return GetThrower(currentHolder);
        }

        private void AimCurrentHolderAtDodger(float deltaSeconds)
        {
            if (dodger == null)
            {
                return;
            }

            var targetX = Mathf.Clamp(dodger.position.x, -throwerLaneHalfWidth, throwerLaneHalfWidth);
            AimThrower(GetThrower(currentHolder), GetThrowerStartPosition(currentHolder), targetX, deltaSeconds);
        }

        private void UpdateCurrentHolderAim(float deltaSeconds)
        {
            if (IsCurrentHolderAi())
            {
                AimCurrentHolderAtDodger(deltaSeconds);
                return;
            }

            var axis = KeyboardDodgeballInput.GetThrowerAxis(GetCurrentHolderControl());
            var thrower = GetThrower(currentHolder);
            if (thrower == null)
            {
                return;
            }

            var targetX = Mathf.Clamp(
                thrower.position.x + axis * throwerAimSpeed * deltaSeconds,
                -throwerLaneHalfWidth,
                throwerLaneHalfWidth);
            AimThrower(thrower, GetThrowerStartPosition(currentHolder), targetX, deltaSeconds);
        }

        private bool IsCurrentHolderAi()
        {
            return GetCurrentHolderControl() == ThrowerControl.Ai;
        }

        private bool IsCurrentHolderHuman()
        {
            return !IsCurrentHolderAi();
        }

        private bool IsCurrentHolderThrowPressed()
        {
            return KeyboardDodgeballInput.IsThrowRequested(GetCurrentHolderControl());
        }

        private ThrowerControl GetCurrentHolderControl()
        {
            return session.GetThrowerControl(currentHolder == ThrowerId.A);
        }

        private void SelectKeyboardMode()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                session.SelectMode(DodgeballMode.SinglePlayer);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                session.SelectMode(DodgeballMode.TwoPlayers);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                session.SelectMode(DodgeballMode.ThreePlayers);
            }
        }

        private void AimThrower(Transform thrower, Vector3 basePosition, float targetX, float deltaSeconds)
        {
            if (thrower == null)
            {
                return;
            }

            var targetPosition = new Vector3(targetX, basePosition.y, basePosition.z);
            thrower.position = Vector3.MoveTowards(thrower.position, targetPosition, throwerAimSpeed * deltaSeconds);
        }

        private Transform GetThrower(ThrowerId throwerId)
        {
            return throwerId == ThrowerId.A ? throwerA : throwerB;
        }

        private Vector3 GetThrowerStartPosition(ThrowerId throwerId)
        {
            return throwerId == ThrowerId.A ? throwerAStartPosition : throwerBStartPosition;
        }

        private static ThrowerId GetReceiver(ThrowerId holder)
        {
            return holder == ThrowerId.A ? ThrowerId.B : ThrowerId.A;
        }

        private static Vector3 GetHolderForward(ThrowerId holder)
        {
            return holder == ThrowerId.A ? Vector3.down : Vector3.up;
        }

        // ----------------------------------------------------------------------------
        // 沙包生命周期：创建运行时沙包，随持球者移动，出手后计算方向与接球目标
        // ----------------------------------------------------------------------------
        private void EnsureSandbagObject()
        {
            if (sandbagObject != null)
            {
                return;
            }

            sandbagObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sandbagObject.name = "Prototype_Sandbag_Runtime";
            if (projectileRoot != null)
            {
                sandbagObject.transform.SetParent(projectileRoot, false);
            }

            sandbagObject.transform.localScale = new Vector3(0.24f, 0.24f, 0.1f);
            sandbagObject.GetComponent<Renderer>().material.color = new Color(0.92f, 0.66f, 0.24f);
        }

        private void AttachSandbagToHolder()
        {
            EnsureSandbagObject();
            var holder = GetThrower(currentHolder);
            if (holder == null || sandbagObject == null)
            {
                return;
            }

            sandbagObject.SetActive(true);
            sandbagObject.name = currentHolder == ThrowerId.A ? "Prototype_Sandbag_Held_By_A" : "Prototype_Sandbag_Held_By_B";
            sandbagObject.transform.position = holder.position + GetHolderForward(currentHolder) * SandbagHoldOffset + Vector3.back * 0.2f;
        }

        private void ReleaseSandbag()
        {
            EnsureSandbagObject();
            var source = GetActiveThrower();
            var sourcePosition = source != null ? source.position : GetThrowerStartPosition(currentHolder);
            var targetPosition = dodger != null ? dodger.position : Vector3.zero;
            var direction = targetPosition - sourcePosition;
            direction.z = 0f;
            if (direction.sqrMagnitude <= 0.001f)
            {
                direction = GetHolderForward(currentHolder);
            }

            sandbagDirection = direction.normalized;
            sandbagHit = false;
            SetSandbagState(SandbagState.FlyingToDodger);
            sandbagObject.SetActive(true);
            sandbagObject.name = currentHolder == ThrowerId.A ? "Prototype_Sandbag_A_To_B" : "Prototype_Sandbag_B_To_A";
            sandbagObject.transform.position = sourcePosition + sandbagDirection * SandbagHoldOffset + Vector3.back * 0.2f;
        }

        private bool HasReachedReceiverSide()
        {
            if (sandbagObject == null)
            {
                return false;
            }

            var receiver = GetReceiver(currentHolder);
            return currentHolder == ThrowerId.A
                ? sandbagObject.transform.position.y <= -catchBoundaryY
                : sandbagObject.transform.position.y >= catchBoundaryY;
        }

        private void BeginCatching()
        {
            var receiver = GetReceiver(currentHolder);
            var receiverBase = GetThrowerStartPosition(receiver);
            var targetX = sandbagObject != null ? sandbagObject.transform.position.x : receiverBase.x;
            catchTargetPosition = new Vector3(
                Mathf.Clamp(targetX, -throwerLaneHalfWidth, throwerLaneHalfWidth),
                receiverBase.y,
                receiverBase.z - 0.2f);
            SetSandbagState(SandbagState.Catching);
        }

        private void MoveReceiverTowardCatchTarget(float deltaSeconds)
        {
            var receiver = GetReceiver(currentHolder);
            AimThrower(GetThrower(receiver), GetThrowerStartPosition(receiver), catchTargetPosition.x, deltaSeconds);
        }

        private bool IsSandbagOutOfBounds()
        {
            if (sandbagObject == null)
            {
                return true;
            }

            var position = sandbagObject.transform.position;
            return Mathf.Abs(position.x) > throwerLaneHalfWidth + catchDistance
                || Mathf.Abs(position.y) > catchBoundaryY + 1f;
        }

        private void BeginReset(ThrowerId nextHolder)
        {
            resetHolder = nextHolder;
            sandbagHit = false;
            HideWarning();
            ResetThrowerPulse();
            SetSandbagState(SandbagState.Reset);
        }

        private void CompleteReset()
        {
            currentHolder = resetHolder;
            SetSandbagState(SandbagState.Held);
            throwTimer = Mathf.Max(0.8f, throwIntervalSeconds - (100f - session.RemainingRoundSeconds) * 0.012f);
            AttachSandbagToHolder();
        }

        private void SetSandbagState(SandbagState nextState)
        {
            sandbagState = nextState;
            sandbagStateTimer = 0f;
        }

        // ----------------------------------------------------------------------------
        // 表现与预警：投掷蓄力时放大投掷者，并用一条提示条指向当前目标
        // ----------------------------------------------------------------------------
        private void AnimateActiveThrower(float windupProgress)
        {
            var activeThrower = GetActiveThrower();
            if (activeThrower == null)
            {
                return;
            }

            var fromTop = activeThrower == throwerA;
            var basePosition = fromTop ? throwerAStartPosition : throwerBStartPosition;
            var baseScale = fromTop ? throwerAStartScale : throwerBStartScale;
            var pulse = Mathf.Sin(windupProgress * Mathf.PI);
            var lanePosition = new Vector3(activeThrower.position.x, basePosition.y, basePosition.z);
            activeThrower.position = lanePosition + (fromTop ? Vector3.down : Vector3.up) * throwerWindupOffset * pulse;
            activeThrower.localScale = baseScale * (1f + 0.18f * pulse);
        }

        private void UpdateWarning()
        {
            if (warningObject == null || !warningObject.activeSelf)
            {
                return;
            }

            var source = GetActiveThrower();
            var targetPosition = dodger != null ? dodger.position : Vector3.zero;
            if (source == null)
            {
                var fromTop = currentHolder == ThrowerId.A;
                warningObject.transform.position = new Vector3(0f, fromTop ? 2.1f : -2.1f, -0.6f);
                warningObject.transform.localScale = new Vector3(0.72f, 0.12f, 0.05f);
                warningObject.transform.rotation = Quaternion.identity;
                return;
            }

            var start = source.position;
            var end = targetPosition;
            start.z = -0.65f;
            end.z = -0.65f;
            var delta = end - start;
            var distance = Mathf.Max(0.35f, delta.magnitude);
            warningObject.transform.position = start + delta * 0.5f;
            warningObject.transform.localScale = new Vector3(distance, 0.045f, 0.05f);
            warningObject.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        }

        private void ResetThrowerPulse()
        {
            if (throwerA != null)
            {
                throwerA.localScale = throwerAStartScale;
            }

            if (throwerB != null)
            {
                throwerB.localScale = throwerBStartScale;
            }
        }

        private void ResetThrowerPose()
        {
            if (throwerA != null)
            {
                throwerA.position = throwerAStartPosition;
                throwerA.localScale = throwerAStartScale;
            }

            if (throwerB != null)
            {
                throwerB.position = throwerBStartPosition;
                throwerB.localScale = throwerBStartScale;
            }
        }

        // ----------------------------------------------------------------------------
        // HUD 文案：把规则状态翻译成场景里的阶段、时间、生命、统计和结算文字
        // ----------------------------------------------------------------------------
        private void RefreshHud()
        {
            SetText(statusText, GetStatusText());
            SetText(timerText, GetTimerText());
            SetText(livesText, $"Lives: {session.Lives}");
            SetText(dodgeText, $"Dodge: {session.DodgeCount}");
            SetText(hitText, $"Team Hit: {session.ThrowerTeamHitCount}");
            SetText(inputText, GetInputText());
            SetText(resultText, GetResultText());
        }

        private string GetStatusText()
        {
            return session.Phase switch
            {
                RoundPhase.Ready => $"READY {GetModeLabel()} - 1/2/3 Select, Enter/Space Start",
                RoundPhase.Countdown => $"COUNTDOWN {Mathf.CeilToInt(session.RemainingCountdownSeconds)}",
                RoundPhase.Playing => $"PLAYING {GetModeLabel()} - {GetPlayingControls()}",
                RoundPhase.Result => "RESULT - R Restart, Esc Mode Select",
                _ => session.Phase.ToString()
            };
        }

        private string GetTimerText()
        {
            return $"Time: {Mathf.CeilToInt(session.RemainingRoundSeconds)}";
        }

        private string GetResultText()
        {
            return session.Phase == RoundPhase.Result
                ? $"{session.WinningTeam} Win - {session.ResultReason}  {session.BreakTitle}  Rating: {session.Rating}  Team Hit: {session.ThrowerTeamHitCount}"
                : string.Empty;
        }

        private string GetModeLabel()
        {
            return session.Mode switch
            {
                DodgeballMode.SinglePlayer => "1P: C vs AI+AI",
                DodgeballMode.TwoPlayers => "2P: C vs P2+AI",
                DodgeballMode.ThreePlayers => "3P: C vs P2+P3",
                _ => session.Mode.ToString()
            };
        }

        private string GetPlayingControls()
        {
            return session.Mode switch
            {
                DodgeballMode.SinglePlayer => "P1 A/D",
                DodgeballMode.TwoPlayers => "P1 A/D | P2 J/L + I",
                DodgeballMode.ThreePlayers => "P1 A/D | P2 J/L + I | P3 ←/→ + ↑",
                _ => string.Empty
            };
        }

        private string GetInputText()
        {
            if (session.Phase == RoundPhase.Ready)
            {
                return "Choose 1/2/3, then Enter/Space";
            }

            if (session.Phase != RoundPhase.Playing)
            {
                return string.Empty;
            }

            var holderName = currentHolder == ThrowerId.A ? "A" : "B";
            var control = GetCurrentHolderControl();
            if (control == ThrowerControl.Ai)
            {
                return $"Holder {holderName}: AI";
            }

            var aimActive = Mathf.Abs(KeyboardDodgeballInput.GetThrowerAxis(control)) > 0f;
            var playerLabel = control == ThrowerControl.PlayerTwo ? "P2 J/L + I" : "P3 ←/→ + ↑";
            return $"Holder {holderName}: {playerLabel} {(aimActive ? "aiming" : "ready")}";
        }

        private static void SetText(TextMesh target, string value)
        {
            if (target != null)
            {
                target.text = value;
            }
        }

        // ----------------------------------------------------------------------------
        // 内部枚举：只描述当前持球方和沙包所处阶段，不暴露给外部系统
        // ----------------------------------------------------------------------------
        private enum ThrowerId
        {
            A,
            B
        }

        private enum SandbagState
        {
            Held,
            Warning,
            FlyingToDodger,
            Catching,
            Caught,
            Hit,
            Reset
        }
    }
}
