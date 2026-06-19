using UnityEngine;

namespace DontPushTheButton.Player
{
    /// <summary>
    /// 落点标记（俯视 2.5D 跳跃纵深补偿）。
    /// 把一个扁平圆盘钉在角色的 XZ 地面投影上、Y 固定在地面高度——
    /// 角色跳起时圆盘留在落点不动，俯视下清晰表达「会落在哪里」。
    /// GDD 锁定需求，M1 验证可见性。
    /// </summary>
    public class LandingMarker : MonoBehaviour
    {
        [Tooltip("跟随的目标（角色 Transform）")]
        [SerializeField] private Transform _target;
        [Tooltip("圆盘吸附的地面 Y 高度")]
        [SerializeField] private float _groundY = 0f;

        private void LateUpdate()
        {
            if (_target == null) return;
            Vector3 p = _target.position;
            transform.position = new Vector3(p.x, _groundY, p.z);
        }
    }
}
