using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "动画配置/主角动画配置")]
public class SO_ProtagonistAnimator : ScriptableObject
{
    [Header("移动")]
    [Tooltip("转向速度")] public float _turnSpeed = 450;
    [Tooltip("转向死区")] public float _turnThreshold = 0.1f;
    [Tooltip("转向衰减边界(角度)")] public float _turnDampingBorder = 10;
    [Header("底盘惯性")]
    [Tooltip("惯性增量")] public float inertanceAccelerate;
    [Tooltip("惯性死区")] public float inertanceThreshold;
    [Tooltip("惯性限制")] public float inertanceLimit;
    [Tooltip("旋转系数")] public float rotateFactor;
    [Tooltip("旋转限制")] public float rotateLimit;
    [Header("天线惯性")]
    [Tooltip("惯性增量")] public float _inertanceAccelerate;
    [Tooltip("惯性死区")] public float _inertanceThreshold;
    [Tooltip("惯性限制")] public float _inertanceLimit;
    [Tooltip("旋转系数")] public float _rotateFactor;
    [Tooltip("旋转限制")] public float _rotateLimit;
    [Header("螺旋桨")]
    [Tooltip("默认转速")] public float _screwDefaultSpeed = 1000;
    [Tooltip("超载转速")] public float _screwOverloadSpeed = 2000;
    [Header("推进器")]
    [Tooltip("引擎瞬时转速")] public float _ventMaxSpeed = 1500;
    [Tooltip("引擎减速度")] public float _ventDeaccelate = 500;
}
