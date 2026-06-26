using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WheelColliderEvent : MonoBehaviour
{
    public Vector3 Normal { get; private set; }

    private void OnCollisionEnter(Collision collision)
    {
        Normal = collision.GetContact(0).normal;
    }
}
