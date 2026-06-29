using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 控制螺旋桨动画
/// </summary>
public class ProtagonistAirscrew : MonoBehaviour
{
    [SerializeField] Transform _airscrew;

    public void Roll(float angle)
    {
        _airscrew.Rotate(Vector3.forward, -angle);
    }
}
