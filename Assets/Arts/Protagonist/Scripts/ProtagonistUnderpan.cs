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
    [Tooltip("初始夹角β_0（弧度）")][SerializeField] float beta_0;
    [Tooltip("初始夹角β_1（弧度）")][SerializeField] float beta_1;
    [Tooltip("轮子半径")][SerializeField] float r_w;
    [Tooltip("倾斜中心高度")][SerializeField] float h;
    [Tooltip("倾斜中心到悬挂起点距离")][SerializeField] float[] l;
    [Tooltip("悬挂长度")][SerializeField] float[] l_s;
    [Tooltip("悬挂旋转阈值")][SerializeField] float _suspendThreshold = 0.01f;
    //[Tooltip("悬挂初始姿态")] Quaternion[] _suspendDefault;
    float deltaY;
    [Tooltip("上帧悬挂夹角")] float[] _lastSuspendAngles = new float[2];
    [Tooltip("当前悬挂夹角")] float[] _suspendAngles = new float[2];
    [Tooltip("该帧悬挂需旋转的角度°")] float _suspendAngle;
    //[Tooltip("车体倾斜角度")] float _underpanAngle;
    //float _lastUnderpanAngle;
    [Header("惯性")]
    //[Tooltip("惯性增量")][SerializeField] float inertanceAccelerate;
    //[Tooltip("惯性死区")][SerializeField] float inertanceThreshold;
    //[Tooltip("惯性限制")][SerializeField] float inertanceLimit;
    //[Tooltip("旋转系数")][SerializeField] float rotateFactor;
    //[Tooltip("旋转限制")][SerializeField] float rotateLimit;
    float inertance;
    float inertanceDelta;
    [Tooltip("车体倾斜角度")] float angleRotated;
    [Tooltip("车体倾斜角度（弧度）")] float angleRotatedRad;
    float angleBeforeRotated;
    float rotateAngle;
    ProtagonistRig rig;
    float lastSpeed;
    //悬挂应旋转的角度β与车体倾斜角度α的关系为：
    //β = arccos((Δy sinα - cosα √(l_s^2 - Δy^2))/l_s) - β_0
    //其中Δy = r_w -h + l sinα
    [Header("底盘适应地形")]
    //[Tooltip("前后高低差阈值")][SerializeField] float columnDropThreshold;
    //[Tooltip("轮胎碰撞")][SerializeField] WheelColliderEvent[] wheelColliders;
    //[Tooltip("法线差死区")][SerializeField] float normalDeltaThreshold;
    //[Tooltip("横向")] Vector3 rowDirect;
    //[Tooltip("纵向")] Vector3 columnDirect;
    //[Tooltip("前后高低差")] float columnDrop;
    [Tooltip("旋转速度")]
    [SerializeField] float adaptSpeed;
    [Tooltip("目标朝向")] Quaternion targetDirect;

    private void Awake()
    {
        rig = GetComponentInParent<ProtagonistRig>();

        //_suspendDefault = new Quaternion[4];
        //_suspendDefault[0] = _suspendsFront[0].localRotation;
        //_suspendDefault[1] = _suspendsFront[1].localRotation;
        //_suspendDefault[2] = _suspendsBack[0].localRotation;
        //_suspendDefault[3] = _suspendsBack[1].localRotation;

        _lastSuspendAngles[0] = beta_0 * Mathf.Rad2Deg;
        _lastSuspendAngles[1] = beta_1 * Mathf.Rad2Deg;
    }

    /// <summary>
    /// 根据地形倾斜底盘
    /// </summary>
    public void UnderpanSlant(Vector3 normal)
    {
        //Debug.DrawRay(transform.position, normal, Color.red);
        //transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(Quaternion.AngleAxis(90, transform.right) * normal, transform.parent.right), normal);
        //Debug.DrawRay(transform.position, Quaternion.AngleAxis(90, transform.right) * normal, Color.yellow);

        targetDirect = Quaternion.LookRotation(Vector3.ProjectOnPlane(Quaternion.AngleAxis(90, transform.right) * normal, transform.parent.right), normal);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetDirect, adaptSpeed);

        //return;
        /*//高低差
        columnDrop = Mathf.Abs(_leftWheels[0].localPosition.z + _rightWheels[0].localPosition.z - _leftWheels[2].localPosition.z - _rightWheels[2].localPosition.z);
        //Debug.Log(columnDrop);
        if (columnDrop < columnDropThreshold) return;*/
        //法线对比
        //if (!((Vector3.Dot(wheelColliders[0].Normal, wheelColliders[2].Normal) > normalDeltaThreshold && Vector3.Dot(wheelColliders[1].Normal, wheelColliders[3].Normal) > normalDeltaThreshold) || (Vector3.Dot(wheelColliders[0].Normal, wheelColliders[1].Normal) > normalDeltaThreshold && Vector3.Dot(wheelColliders[2].Normal, wheelColliders[3].Normal) > normalDeltaThreshold))) return;
        //平面法线
        //rowDirect = _rightWheels[2].position - _leftWheels[2].position;
        //columnDirect = _leftWheels[0].position - _leftWheels[2].position;
        //transform.rotation = Quaternion.LookRotation(columnDirect, Vector3.Cross(columnDirect, rowDirect));
        //Debug.DrawRay(transform.position, Vector3.Cross(Vector3.ProjectOnPlane(columnDirect, transform.parent.right), rowDirect), Color.red);
    }
    /// <summary>
    /// 惯性倾斜
    /// </summary>
    /// <param name="alpha">倾斜角度α，默认前倾</param>
    public void InertanceSlant(bool isGrounded, float inertanceAccelerate, float inertanceThreshold, float inertanceLimit, float rotateFactor, float rotateLimit)
    {
        ///-----------底盘惯性
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
                _underpan.Rotate(Vector3.right, angleRotated - angleBeforeRotated);
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
                    _underpan.Rotate(Vector3.right, angleRotated - angleBeforeRotated);
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

        ///------------悬挂自适应
        angleRotatedRad = Mathf.Abs(angleRotated) * Mathf.Deg2Rad;
        ///---------前轮
        deltaY = r_w - h + l[0] * Mathf.Sin(angleRotatedRad);
        _suspendAngles[0] = (Mathf.Acos((deltaY * Mathf.Sin(angleRotatedRad) - Mathf.Cos(angleRotatedRad) * Mathf.Sqrt(l_s[0] * l_s[0] - deltaY * deltaY)) / l_s[0])) * Mathf.Rad2Deg;
        _suspendAngle = _suspendAngles[0] - _lastSuspendAngles[0];
        if (angleRotated < 0 && Mathf.Abs(_suspendAngle) > _suspendThreshold)
        {
            foreach (var suspend in _suspendsFront)
            {
                suspend.Rotate(Vector3.right, -_suspendAngle);
            }
        }
        //Debug.Log(_suspendAngles[0]);
        ///---------后轮
        deltaY = r_w - h + l[1] * Mathf.Sin(angleRotatedRad);
        _suspendAngles[1] = (Mathf.Acos((deltaY * Mathf.Sin(angleRotatedRad) - Mathf.Cos(angleRotatedRad) * Mathf.Sqrt(l_s[1] * l_s[1] - deltaY * deltaY)) / l_s[1])) * Mathf.Rad2Deg;
        _suspendAngle = _suspendAngles[1] - _lastSuspendAngles[1];
        if (angleRotated > 0 && Mathf.Abs(_suspendAngle) > _suspendThreshold)
        {
            foreach (var suspend in _suspendsBack)
            {
                suspend.Rotate(Vector3.right, _suspendAngle);
            }
        }

        _suspendAngles.CopyTo(_lastSuspendAngles, 0);
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
