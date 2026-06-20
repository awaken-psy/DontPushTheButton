using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DontPushTheButton.Abilities;
using DontPushTheButton.Binding;
using DontPushTheButton.Config;
using DontPushTheButton.Corruption;

namespace DontPushTheButton.Player
{
    /// <summary>
    /// 能力驱动的角色物理权威 + 输入权威（M2.5：按 LoadoutConfig 轮询 Keyboard 驱动能力 + 超载触发腐败）。
    /// 每帧轮询 LoadoutConfig 的槽位：移动方向读 isPressed，能力读 wasPressedThisFrame；
    /// 超载键按下 → 挂账待扣费(_pendingOverloadCharges) + 标记该次触发超载强化；
    /// 能力确认执行时经 ChargeOverload 扣腐败（未生效不扣，修空中连按连续触发）。
    /// 物理 Move 只在此处合并单次（保证 isGrounded 时机，规避双 Move 坑）。
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerAbilityController : MonoBehaviour, IAbilityContext
    {
        [Header("引用")]
        [SerializeField] private MovementTuning _tuning;
        [SerializeField] private Camera _relativeCamera;
        [Tooltip("关卡配置（M2.3）；Start 构建默认 LoadoutConfig。M2.8 改从 LoadoutUIController 取玩家配置")]
        [SerializeField] private LevelConfigSO _levelConfig;
        [Tooltip("腐败追踪（M2.5）；留空则自动 GetComponent")]
        [SerializeField] private CorruptionTracker _corruption;

        private CharacterController _controller;
        private JumpAbility _jump; // M3.5：缓落需读 IsSlowFalling 调重力
        private readonly List<AbilityBase> _abilities = new List<AbilityBase>();
        private float _verticalVelocity;
        private Vector3 _horizontalThisFrame;
        private LoadoutConfig _loadout;

        // 本帧输入状态（轮询 LoadoutConfig 填充）
        private Vector2 _moveInput;
        private readonly HashSet<AbilityKind> _pressedAbilities = new HashSet<AbilityKind>();
        private readonly HashSet<AbilityKind> _overloadAbilities = new HashSet<AbilityKind>();
        // 本帧按下、标记为超载、待扣费的能力（能力确认执行时 ChargeOverload 消费；未生效则帧末随 Clear 丢弃）
        private readonly HashSet<AbilityKind> _pendingOverloadCharges = new HashSet<AbilityKind>();

        // ---- IAbilityContext ----
        public bool IsGrounded => _controller.isGrounded;
        public MovementTuning Tuning => _tuning;
        public Transform Body => transform;
        public Camera RelativeCamera => _relativeCamera;
        public Vector2 MoveInput => _moveInput;
        public bool WasPressedThisFrame(AbilityKind k) => _pressedAbilities.Contains(k);
        public bool IsOverloadTrigger(AbilityKind k) => _overloadAbilities.Contains(k);
        public bool ChargeOverload(AbilityKind k)
        {
            if (!_pendingOverloadCharges.Remove(k)) return false; // 本次按下非超载或已扣过 → 不扣
            if (_corruption != null) _corruption.AddOverloadPress();
            return true;
        }
        public CharacterController Controller => _controller;
        public LoadoutConfig Loadout => _loadout;
        public void AddHorizontal(Vector3 d) => _horizontalThisFrame += d;
        public void SetVerticalVelocity(float v) => _verticalVelocity = v;

        private static readonly Dictionary<MoveDirection, Vector2> DirVec = new Dictionary<MoveDirection, Vector2>
        {
            { MoveDirection.Up, Vector2.up }, { MoveDirection.Down, Vector2.down },
            { MoveDirection.Left, Vector2.left }, { MoveDirection.Right, Vector2.right },
        };

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            GetComponents(_abilities);
            _jump = GetComponent<JumpAbility>();
            if (_tuning == null) Debug.LogError("[PlayerAbilityController] MovementTuning 未赋值", this);
            if (_relativeCamera == null) _relativeCamera = Camera.main;
            if (_corruption == null) _corruption = GetComponent<CorruptionTracker>();
        }

        private void Start()
        {
            // 默认 LoadoutConfig（关2典型：WASD 移动 + 超载键绑首个能力）；M2.8 改为 SetLoadout(玩家配置)
            if (_levelConfig != null && _loadout == null)
                _loadout = BuildDefaultLoadout(_levelConfig);
        }

        /// <summary>M2.8：从配置阶段传入玩家 LoadoutConfig。</summary>
        public void SetLoadout(LoadoutConfig cfg) => _loadout = cfg;

        /// <summary>默认绑定：WASD→移动方向，其余键按顺序绑能力（超载键优先）。</summary>
        private static LoadoutConfig BuildDefaultLoadout(LevelConfigSO so)
        {
            var cfg = new LoadoutConfig { SlotCount = so.SlotCount };
            var dirMap = new Dictionary<string, MoveDirection>
            {
                { "W", MoveDirection.Up }, { "A", MoveDirection.Left },
                { "S", MoveDirection.Down }, { "D", MoveDirection.Right },
            };
            var abs = new List<AbilityKind>(so.AvailableAbilities);
            int abIdx = 0;
            foreach (var k in so.AvailableKeys)
            {
                BindingItem? b = null;
                if (dirMap.TryGetValue(k.KeyName, out var d)) b = BindingItem.Move(d);
                else if (abIdx < abs.Count) { b = BindingItem.Of(abs[abIdx]); abIdx++; }
                cfg.Slots.Add(new KeySlot(k.KeyName, k.IsOverload, b));
            }
            foreach (var a in abs) cfg.AvailableAbilities.Add(a);
            return cfg;
        }

        private void Update()
        {
            if (_tuning == null) return;
            _horizontalThisFrame = Vector3.zero;
            PollInput();
            TickContinuousAbilities(); // 移动 / 跳跃（M3.5 Jump 改 Continuous 飞行状态机）
            TickInstantAbilities();   // 推动 / 拉动 / 冲刺
            ApplyGravity();           // 贴地钳制 + 重力（缓落：超载飞行后 IsSlowFalling 减重力）
            ApplyMove();              // 合并单次 Move（保证 isGrounded 时机）
        }

        private void TickContinuousAbilities()
        {
            foreach (var a in _abilities)
                if (a.Trigger == AbilityTrigger.Continuous) a.TickContinuous(this);
        }

        private void TickInstantAbilities()
        {
            foreach (var a in _abilities)
                if (a.Trigger == AbilityTrigger.Instant) a.TickInstant(this);
        }

        /// <summary>贴地钳制 + 重力；缓落态（超载飞行后）重力 × FallGravityScale。</summary>
        private void ApplyGravity()
        {
            bool grounded = _controller.isGrounded;
            if (grounded && _verticalVelocity < 0f)
                _verticalVelocity = _tuning.GroundStickVelocity;
            float g = _tuning.Gravity;
            if (_jump != null && _jump.IsSlowFalling)
                g *= _jump.FallGravityScale;
            _verticalVelocity += g * Time.deltaTime;
        }

        /// <summary>合并单次 Move（水平 + 垂直），保证下一帧 isGrounded 正确。</summary>
        private void ApplyMove()
        {
            _controller.Move(_horizontalThisFrame + Vector3.up * (_verticalVelocity * Time.deltaTime));
        }

        /// <summary>轮询 LoadoutConfig 各槽位键状态，填充本帧输入状态 + 超载触发腐败。</summary>
        private void PollInput()
        {
            _moveInput = Vector2.zero;
            _pressedAbilities.Clear();
            _overloadAbilities.Clear();
            _pendingOverloadCharges.Clear();
            if (_loadout == null) return;
            var kb = Keyboard.current;
            if (kb == null) return;

            foreach (var slot in _loadout.Slots)
            {
                if (!slot.Binding.HasValue) continue;
                if (!System.Enum.TryParse<Key>(slot.KeyName, out var key)) continue; // KeyName 直接用 InputSystem Key 枚举名（W/Space/E…），无需维护映射表
                var k = kb[key];
                var item = slot.Binding.Value;

                if (item.Type == BindingItemType.MoveDirection)
                {
                    if (k.isPressed)
                    {
                        if (DirVec.TryGetValue(item.Direction, out var v)) _moveInput += v;
                        if (slot.IsOverload) _overloadAbilities.Add(AbilityKind.Move);
                    }
                    // 移动绑超载：按下挂账（待 MoveAbility 启动超载加速时 ChargeOverload 扣费；未启动不扣）
                    if (slot.IsOverload && k.wasPressedThisFrame)
                        _pendingOverloadCharges.Add(AbilityKind.Move);
                }
                else // Ability
                {
                    if (k.wasPressedThisFrame)
                    {
                        _pressedAbilities.Add(item.Ability);
                        if (slot.IsOverload)
                        {
                            _overloadAbilities.Add(item.Ability);
                            _pendingOverloadCharges.Add(item.Ability); // 挂账，待能力确认执行时 ChargeOverload 扣费
                        }
                    }
                }
            }
            if (_moveInput.sqrMagnitude > 1f) _moveInput.Normalize();
        }
    }
}
