using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 惯性摇摆
/// </summary>
/// <remarks>
/// 让角色上的部件根据惯性摇摆，挂载在对应骨骼上，由<see cref="ProtagonistRig"/>调度<see langword="CallOnFixedUpdate"/>
/// </remarks>
public class InertanceShake : MonoBehaviour
{
    Vector3 lastPos;
    float speed;
    float inertance;
    //[Tooltip("惯性增量")][SerializeField] float inertanceAccelerate;
    //[Tooltip("惯性死区")][SerializeField] float inertanceThreshold;
    //[Tooltip("旋转系数")][SerializeField] float rotateFactor;
    float inertanceDelta;
    [Tooltip("已旋转角度")] float angleRotated;
    float angleBeforeRotated;
    float rotateAngle;

    private void Awake()
    {
        lastPos = transform.position;
    }

    /// <summary>
    /// 在<see langword="LateUpdate"/>中调用
    /// </summary>
    public void CallOnLateUpdate(float inertanceAccelerate, float inertanceThreshold, float rotateFactor)
    {
        //speed ∈ [0, 0.05)
        speed = -transform.InverseTransformVector((transform.position - lastPos) / Time.deltaTime).y;
        //Debug.Log(speed);

        inertanceDelta = inertance - speed;
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
            angleBeforeRotated = angleRotated;
            angleRotated += rotateAngle;
            angleRotated = Mathf.Clamp(angleRotated, -10, 10);
            transform.Rotate(Vector3.right, angleRotated - angleBeforeRotated);
            Debug.Log($"惯性:{angleRotated}");
        }

        if (Mathf.Abs(speed) < inertanceThreshold)
        {
            if (Mathf.Abs(angleRotated) > 0.05f)
            {
                rotateAngle = -inertanceAccelerate * Time.deltaTime * rotateFactor * angleRotated / 10;
                angleBeforeRotated = angleRotated;
                angleRotated += rotateAngle;
                angleRotated = Mathf.Clamp(angleRotated, -10, 10);
                transform.Rotate(Vector3.right, angleRotated - angleBeforeRotated);
                Debug.Log($"回正:{angleRotated}");
            }
        }

        lastPos = transform.position;
    }
}
