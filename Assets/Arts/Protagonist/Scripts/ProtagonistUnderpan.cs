using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 控制底盘动画
/// </summary>
public class ProtagonistUnderpan : MonoBehaviour
{
    //轮子，从前至后
    [SerializeField] Transform[] _leftWheels;
    [SerializeField] Transform[] _rightWheels;

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
