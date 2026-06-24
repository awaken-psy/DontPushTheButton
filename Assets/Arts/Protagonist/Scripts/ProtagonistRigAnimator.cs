using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DontPushTheButton.Abilities;
using DontPushTheButton.Player;

/// <summary>
/// 主角动画器
/// </summary>
/// <remarks>
/// 用于控制主角的各种动画效果
/// </remarks>
public class ProtagonistRigAnimator : MonoBehaviour
{
    #region 组件
    [Header("组件")]
    [Tooltip("需传入主角预制体上的ProtagonistRig组件")]
    [SerializeField] ProtagonistRig _rig;
    #endregion
    #region 控制器
    PlayerAbilityController _controller;
    JumpAbility _jump;
    PickupAbility _pick;
    #endregion
    #region 变量
    //[Tooltip("上帧方向")] Vector3 _lastDirect;
    [Tooltip("引擎当前转速")] float _ventSpeed;
    [Tooltip("模型变换")] Transform _modelTrans;
    [Tooltip("两帧方向夹角")] float _deltaAngleSigned;
    float _deltaAngle;
    [Tooltip("轮胎转角")] float _wheelAngle;
    #endregion
    #region 动画表现参数
    [Header("移动")]
    [Tooltip("转向速度")][SerializeField] float _turnSpeed = 450;
    [Tooltip("转向死区")][SerializeField] float _turnThreshold = 0.1f;
    [Tooltip("转向衰减边界(角度)")][SerializeField] float _turnDampingBorder = 10;
    [Header("底盘惯性")]

    [Header("天线惯性")]
    [Tooltip("惯性增量")][SerializeField] float _inertanceAccelerate;
    [Tooltip("惯性死区")][SerializeField] float _inertanceThreshold;
    [Tooltip("惯性限制")][SerializeField] float _inertanceLimit;
    [Tooltip("旋转系数")][SerializeField] float _rotateFactor;
    [Tooltip("旋转限制")][SerializeField] float _rotateLimit;
    [Header("螺旋桨")]
    [Tooltip("默认转速")][SerializeField] float _screwDefaultSpeed = 1000;
    [Tooltip("超载转速")][SerializeField] float _screwOverloadSpeed = 2000;
    [Header("推进器")]
    [Tooltip("引擎瞬时转速")][SerializeField] float _ventMaxSpeed = 1500;
    [Tooltip("引擎减速度")][SerializeField] float _ventDeaccelate = 500;
    #endregion
    #region 测试
    [Header("Test")]
    public float SlantAngle;
    #endregion

    private void Awake()
    {
        _controller = GetComponent<PlayerAbilityController>();
        _jump = GetComponent<JumpAbility>();
        _pick = GetComponent<PickupAbility>();
    }
    private void Start()
    {
        _modelTrans = _rig.transform;
        _modelTrans.position = transform.position;
        _modelTrans.rotation = transform.rotation;



        //_lastDirect = _modelTrans.forward;
    }
    /*private void FixedUpdate()
    {
        #region Temp
        if (_pick.IsCarrying)
        {
            _rig.GravityGun.SetActive(true);
        }
        else
        {
            _rig.GravityGun.SetActive(false);
        }
        #endregion
    }*/
    private void LateUpdate()
    {
        _rig.UpdateSpeed();
        Move();
        Fly();
        Dash();
        Shake();
    }

    private void OnEnable()
    {
        if (_controller != null)
        {
            _controller.OnInstantCast += OnCast;
            _pick.OnCarryChanged += OnPickup;
        }
    }
    private void OnDisable()
    {
        if (_controller != null)
        {
            _controller.OnInstantCast -= OnCast;
            _pick.OnCarryChanged -= OnPickup;
        }
    }

    /// <summary>
    /// 惯性摇摆
    /// </summary>
    void Shake()
    {
        _rig.Body.OnLateUpdate(_inertanceAccelerate, _inertanceThreshold, _rotateFactor, _rotateLimit, _inertanceLimit);
        _rig.Underpan.InertanceSlant(_controller.IsGrounded, SlantAngle);
    }
    /// <summary>
    /// 触发重力枪动画
    /// </summary>
    /// <param name="isPickup"></param>
    void OnPickup(bool isPickup)
    {
        _rig.GravityGun.SetActive(isPickup);
    }
    /// <summary>
    /// 触发能力时调用
    /// </summary>
    void OnCast(AbilityKind ability)
    {
        switch (ability)
        {
            case AbilityKind.Move:
                //Debug.Log("Move");
                break;
            case AbilityKind.Jump:
                //Debug.Log("Jump");
                break;
            /*case AbilityKind.Pickup:
                Debug.Log("Pickup");
                if (_pick.IsCarrying)
                {
                    _rig.GravityGun.SetActive(true);
                }
                else
                {
                    _rig.GravityGun.SetActive(false);
                }
                break;*/
            case AbilityKind.Pull:
                //Debug.Log("Pull");
                break;
            case AbilityKind.Dash:
                //Debug.Log("Dash");
                _ventSpeed = _ventMaxSpeed;
                break;
            case AbilityKind.Push:
                //Debug.Log("Push");
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// 推进
    /// </summary>
    /// <remarks>
    /// 触发推进时转动推进器喷口，并随时间停止
    /// </remarks>
    void Dash()
    {
        if (_rig.Engine == null || _ventSpeed == 0) return;
        if (_ventSpeed > 0)
        {
            _rig.Engine.Roll(_ventSpeed * Time.deltaTime);
            _ventSpeed -= _ventDeaccelate * Time.deltaTime;
        }
        else
        {
            _ventSpeed = 0;
        }
    }
    /// <summary>
    /// 螺旋桨转动
    /// </summary>
    /// <remarks>
    /// 根据<see langword="IsFlying"/>调整转速
    /// </remarks>
    void Fly()
    {
        if (_rig.Airscrew == null) return;

        if (_jump.IsFlying)
        {
            _rig.Airscrew.Roll(_screwOverloadSpeed * Time.deltaTime);
        }
        else
        {
            _rig.Airscrew.Roll(_screwDefaultSpeed * Time.deltaTime);
        }
    }
    /// <summary>
    /// 移动
    /// </summary>
    /// <remarks>
    /// [旧版]通过比较两帧间<see cref="Transform"/>.<see langword="forward"/>夹角大小来决定轮胎旋转状态
    /// </remarks>
    void Move()
    {
        if (_rig.Underpan == null) return;
        //[未使用]
        //车体旋转半径0.84，轮胎半径0.32
        //轮胎旋转角度 = 车体旋转半径 ÷ 轮胎半径 × 车体摆动角度

        //---------------转动车体
        _deltaAngleSigned = Vector3.SignedAngle(_modelTrans.forward, transform.forward, Vector3.up);
        _deltaAngle = Mathf.Abs(_deltaAngleSigned);
        //_wheelAngle = 0.84f / 0.32f * _deltaAngle;
        if (_deltaAngle > _turnThreshold)
        {
            _modelTrans.Rotate(Vector3.up, _turnSpeed * Time.deltaTime * (_deltaAngle > _turnDampingBorder ? (_deltaAngleSigned > 0 ? 1 : -1) : _deltaAngleSigned / _turnDampingBorder));
        }
        //else if (_deltaAngle < -_turnThreshold)
        //{
        //    _modelTrans.Rotate(Vector3.up, -_turnSpeed * Time.deltaTime * _deltaAngle > _turnDampingBorder ? 1 : _deltaAngle / _turnDampingBorder);
        //}

        //-------------------转动车轮
        _wheelAngle = -_controller.Tuning.MoveSpeed * Mathf.Rad2Deg * Time.deltaTime;
        //float angle = Vector3.SignedAngle(_modelTrans.forward, _lastDirect, Vector3.up);
        //Debug.Log(angle);
        if (_deltaAngleSigned > 1)
        {
            _rig.Underpan.RollLeft(_wheelAngle);
            _rig.Underpan.RollRight(-_wheelAngle);
        }
        else if (_deltaAngleSigned < -1)
        {
            _rig.Underpan.RollLeft(-_wheelAngle);
            _rig.Underpan.RollRight(_wheelAngle);
        }
        else
        {
            _wheelAngle *= _controller.MoveInput.magnitude;
            _rig.Underpan.RollLeft(_wheelAngle);
            _rig.Underpan.RollRight(_wheelAngle);
        }

        //_lastDirect = _modelTrans.forward;
    }
}
