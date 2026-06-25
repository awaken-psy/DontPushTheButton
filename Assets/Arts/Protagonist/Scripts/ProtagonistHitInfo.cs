using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 用于获取主角接触点法向
/// </summary>
public class ProtagonistHitInfo : MonoBehaviour
{
    public Vector3 Normal { get; private set; }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Normal = hit.normal;
    }
}
