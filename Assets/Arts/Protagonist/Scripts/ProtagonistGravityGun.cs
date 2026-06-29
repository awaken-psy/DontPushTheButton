using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtagonistGravityGun : MonoBehaviour
{
    Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void SetActive(bool active)
    {
        _animator.SetBool("pick", active);
    }
}
