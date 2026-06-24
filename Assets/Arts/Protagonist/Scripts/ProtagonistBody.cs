using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtagonistBody : MonoBehaviour
{
    [SerializeField] InertanceShake[] _antennaes;

    public void OnLateUpdate(float inertanceAccelerate, float inertanceThreshold, float rotateFactor)
    {
        foreach (var b in _antennaes)
        {
            b.CallOnLateUpdate(inertanceAccelerate, inertanceThreshold, rotateFactor);
        }
    }
}
