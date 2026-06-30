using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

/// <summary>
/// 2.5D 相机右键手动旋转辅助（M3.11）。
/// 默认：CM TopDown 固定俯视（PositionComposer 控制看主角）。
/// 右键按住：禁用 PositionComposer，Yaw 跟鼠标 X 临时旋转视角；松开 Lerp 回默认。
/// 放 Assembly-CSharp（默认引用 Cinemachine + InputSystem）。
/// </summary>
public class CameraYawOrbit : MonoBehaviour
{
    [SerializeField] CinemachineCamera _cm;
    [Tooltip("PositionComposer（右键时禁用，让手动 rotation 生效）")]
    [SerializeField] MonoBehaviour _composer;
    [Tooltip("Yaw 旋转速度")]
    [SerializeField] float _yawSpeed = 0.2f;
    [Tooltip("固定俯视 Pitch（度，与 CM 一致）")]
    [SerializeField] float _pitch = 50f;

    float _yaw = 0f;

    void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;
        if (mouse.rightButton.isPressed)
        {
            _yaw += mouse.delta.x.ReadValue() * _yawSpeed;
            if (_composer != null) _composer.enabled = false;
            transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }
        else if (_yaw != 0f)
        {
            _yaw = Mathf.Lerp(_yaw, 0f, 0.15f);
            transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            if (Mathf.Abs(_yaw) < 0.5f)
            {
                _yaw = 0f;
                if (_composer != null) _composer.enabled = true;
            }
        }
    }
}
