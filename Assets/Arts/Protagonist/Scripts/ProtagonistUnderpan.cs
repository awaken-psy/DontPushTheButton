using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 控制底盘动画
/// </summary>
public class ProtagonistUnderpan : MonoBehaviour
{
    [Header("底盘")]
    [SerializeField] Transform _underpan;
    [Header("轮子，从前至后")]
    [SerializeField] Transform[] _leftWheels;
    [SerializeField] Transform[] _rightWheels;
    [Header("悬挂")]
    [Tooltip("前轮")][SerializeField] Transform[] _suspendsFront;
    [Tooltip("后轮")][SerializeField] Transform[] _suspendsBack;
    [Tooltip("初始夹角β_0")][SerializeField] float beta_0;
    [Tooltip("初始夹角β_1")][SerializeField] float beta_1;
    [Tooltip("轮子半径")][SerializeField] float r_w;
    [Tooltip("倾斜中心高度")][SerializeField] float h;
    [Tooltip("倾斜中心到悬挂起点距离")][SerializeField] float[] l;
    [Tooltip("悬挂长度")][SerializeField] float[] l_s;
    [Tooltip("悬挂旋转阈值")][SerializeField] float _suspendThreshold = 0.01f;
    [Tooltip("悬挂初始姿态")] Quaternion[] _suspendDefault;
    float deltaY;
    float _suspendAngle;
    [Tooltip("车体倾斜角度")] float _underpanAngle;
    float _lastUnderpanAngle;
    [Header("惯性")]
    [Tooltip("惯性增量")][SerializeField] float inertanceAccelerate;
    [Tooltip("惯性死区")][SerializeField] float inertanceThreshold;
    [Tooltip("惯性限制")][SerializeField] float inertanceLimit;
    [Tooltip("旋转系数")][SerializeField] float rotateFactor;
    [Tooltip("旋转限制")][SerializeField] float rotateLimit;
    float inertance;
    float inertanceDelta;
    [Tooltip("已旋转角度")] float angleRotated;
    float angleBeforeRotated;
    float rotateAngle;
    ProtagonistRig rig;
    float lastSpeed;
    //悬挂应旋转的角度β与车体倾斜角度α的关系为：
    //β = arccos((Δy sinα - cosα √(l_s^2 - Δy^2))/l_s) - β_0
    //其中Δy = r_w -h + l sinα

    private void Awake()
    {
        rig = GetComponentInParent<ProtagonistRig>();

        _suspendDefault = new Quaternion[4];
        _suspendDefault[0] = _suspendsFront[0].localRotation;
        _suspendDefault[1] = _suspendsFront[1].localRotation;
        _suspendDefault[2] = _suspendsBack[0].localRotation;
        _suspendDefault[3] = _suspendsBack[1].localRotation;
    }

    /// <summary>
    /// 惯性倾斜
    /// </summary>
    /// <param name="alpha">倾斜角度α，默认前倾</param>
    public void InertanceSlant(bool isGrounded, float alpha)
    {
        //-----------底盘惯性
        if (isGrounded)
        {
            inertanceDelta = inertance - Mathf.Clamp(rig.Speed, -inertanceLimit, inertanceLimit);
            if (Mathf.Abs(inertanceDelta) > inertanceThreshold)
            {
                if (inertanceDelta > 0)
                {
                    inertance -= inertanceAccelerate * Time.deltaTime;
                    rotateAngle = -inertanceAccelerate * Time.deltaTime * rotateFactor;
                }
                else
                {
                    inertance += inertanceAccelerate * Time.deltaTime;
                    rotateAngle = inertanceAccelerate * Time.deltaTime * rotateFactor;
                }
                inertance = Mathf.Clamp(inertance, -inertanceLimit, inertanceLimit);
                angleBeforeRotated = angleRotated;
                angleRotated += rotateAngle;
                angleRotated = Mathf.Clamp(angleRotated, -rotateLimit, rotateLimit);
                transform.Rotate(Vector3.right, -(angleRotated - angleBeforeRotated));
                //Debug.Log($"惯性:{angleRotated}");
            }
            if (Mathf.Abs(rig.Speed - lastSpeed) < inertanceThreshold)
            {
                if (Mathf.Abs(angleRotated) > 0.05f)
                {
                    rotateAngle = -inertanceAccelerate * Time.deltaTime * rotateFactor * angleRotated / rotateLimit;
                    angleBeforeRotated = angleRotated;
                    angleRotated += rotateAngle;
                    angleRotated = Mathf.Clamp(angleRotated, -rotateLimit, rotateLimit);
                    transform.Rotate(Vector3.right, -(angleRotated - angleBeforeRotated));
                    //Debug.Log($"回正:{angleRotated}");
                }
            }
        }
        else
        {
            inertanceDelta = inertance - rig.Speed;
            if (Mathf.Abs(inertanceDelta) > inertanceThreshold)
            {
                if (inertanceDelta > 0)
                {
                    inertance -= inertanceAccelerate * Time.deltaTime;
                    rotateAngle = -inertanceAccelerate * Time.deltaTime * rotateFactor;
                }
                else
                {
                    inertance += inertanceAccelerate * Time.deltaTime;
                    rotateAngle = inertanceAccelerate * Time.deltaTime * rotateFactor;
                }
                inertance = Mathf.Clamp(inertance, -inertanceLimit, inertanceLimit);
                angleBeforeRotated = angleRotated;
                angleRotated += rotateAngle;
                angleRotated = Mathf.Clamp(angleRotated, -rotateLimit, rotateLimit);
                transform.Rotate(Vector3.right, -(angleRotated - angleBeforeRotated));
                //Debug.Log($"惯性:{angleRotated}");
            }
            if (Mathf.Abs(rig.Speed) < inertanceThreshold)
            {
                if (Mathf.Abs(angleRotated) > 0.05f)
                {
                    rotateAngle = -inertanceAccelerate * Time.deltaTime * rotateFactor * angleRotated / rotateLimit;
                    angleBeforeRotated = angleRotated;
                    angleRotated += rotateAngle;
                    angleRotated = Mathf.Clamp(angleRotated, -rotateLimit, rotateLimit);
                    transform.Rotate(Vector3.right, -(angleRotated - angleBeforeRotated));
                    //Debug.Log($"回正:{angleRotated}");
                }
            }
        }

        lastSpeed = rig.Speed;

        //------------悬挂自适应

        //_underpan.Rotate(Vector3.right, -alpha);
        //_underpanAngle += alpha;
        //alpha *= -Mathf.Deg2Rad;
        ////---------前轮
        //if (_underpanAngle > 0)
        //{
        //    deltaY = r_w - h + l[0] * Mathf.Sin(alpha);
        //    _suspendAngle = (Mathf.Acos((deltaY * Mathf.Sin(alpha) - Mathf.Cos(alpha) * Mathf.Sqrt(l_s[0] * l_s[0] - deltaY * deltaY)) / l_s[0]) - beta_0) * Mathf.Rad2Deg;
        //    if (_suspendAngle > _suspendThreshold)
        //    {
        //        _suspendAngle *= (_underpanAngle > _lastUnderpanAngle ? 1 : -1);
        //        foreach (var suspend in _suspendsFront)
        //        {
        //            suspend.Rotate(Vector3.right, _suspendAngle);
        //        }
        //    }
        //    else
        //    {
        //        _suspendsFront[0].localRotation = _suspendDefault[0];
        //        _suspendsFront[1].localRotation = _suspendDefault[1];

        //        //_suspendsFront[0].localRotation = Quaternion.RotateTowards(_suspendsFront[0].localRotation, _suspendDefault[0], _suspendAngle);
        //        //_suspendsFront[1].localRotation = Quaternion.RotateTowards(_suspendsFront[1].localRotation, _suspendDefault[1], _suspendAngle);
        //    }
        //}
        ////---------后轮
        //if (_underpanAngle < 0)
        //{

        //}
        //alpha *= -1;
        //deltaY = r_w - h + l[1] * Mathf.Sin(alpha);
        //_suspendAngle = (Mathf.Acos((deltaY * Mathf.Sin(alpha) - Mathf.Cos(alpha) * Mathf.Sqrt(l_s[1] * l_s[1] - deltaY * deltaY)) / l_s[1]) - beta_1) * Mathf.Rad2Deg;
        //if (_suspendAngle > _suspendThreshold)
        //{
        //    foreach (var suspend in _suspendsBack)
        //    {
        //        suspend.Rotate(Vector3.right, -_suspendAngle);
        //    }
        //}
        //else
        //{
        //    //_suspendsBack[0].localRotation = _suspendDefault[2];
        //    //_suspendsBack[1].localRotation = _suspendDefault[3];

        //    _suspendsBack[0].localRotation = Quaternion.RotateTowards(_suspendsBack[0].localRotation, _suspendDefault[2], _suspendAngle);
        //    _suspendsBack[1].localRotation = Quaternion.RotateTowards(_suspendsBack[1].localRotation, _suspendDefault[3], _suspendAngle);

        //}
        ////------------
        //_lastUnderpanAngle = _underpanAngle;
    }
    /// <summary>
    /// 旋转左侧轮胎
    /// </summary>
    /// <param name="angle">角度</param>
    public void RollLeft(float angle)
    {
        foreach (Transform t in _leftWheels)
        {
            t.Rotate(Vector3.up, angle);
        }
    }
    /// <summary>
    /// 旋转右侧轮胎
    /// </summary>
    /// <param name="angle">角度</param>
    public void RollRight(float angle)
    {
        foreach (Transform t in _rightWheels)
        {
            t.Rotate(Vector3.up, -angle);
        }
    }
}
