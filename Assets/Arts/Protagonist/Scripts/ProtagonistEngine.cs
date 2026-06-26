using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 推进器动画
/// </summary>
public class ProtagonistEngine : MonoBehaviour
{
    [SerializeField] Transform _vent;

    public void Roll(float angle)
    {
        _vent.Rotate(Vector3.forward, -angle);
    }
}
