using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 主角动画控制器
/// </summary>
/// <remarks>
/// 快速获取指定部位动画控制器
/// </remarks>
public class ProtagonistRig : MonoBehaviour
{
    [SerializeField] ProtagonistBody _body;
    [SerializeField] ProtagonistUnderpan _underpan;
    [SerializeField] ProtagonistAirscrew _airscrew;
    [SerializeField] ProtagonistEngine _engine;
    [SerializeField] ProtagonistGravityGun _gravityGun;

    public ProtagonistBody Body => _body;
    public ProtagonistUnderpan Underpan => _underpan;
    public ProtagonistAirscrew Airscrew => _airscrew;
    public ProtagonistEngine Engine => _engine;
    public ProtagonistGravityGun GravityGun => _gravityGun;
}
