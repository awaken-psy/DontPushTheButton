#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Unity.Cinemachine;
using UnityEditor.SceneManagement;

namespace DontPushTheButton.EditorTools
{
    /// <summary>
    /// 一次性构建器：配置俯视 2.5D Cinemachine 相机（Cinemachine 3.x）。
    /// 相机跟随角色位置(Follow)、旋转固定(斜俯视 ~50°)、不随角色转向；
    /// Body = CinemachinePositionComposer(硬跟随/无死区/无阻尼)，Aim = 不挂(旋转固定)。
    /// 菜单：DPTB/Build Top-Down Camera。
    /// </summary>
    public static class CinemachineTopDownBuilder
    {
        private const string MenuPath = "DPTB/Build Top-Down Camera";
        private const string VcamName = "CM TopDown";
        private const float Pitch = 50f;    // 俯角(°)
        private const float Yaw = 0f;       // 偏航(°)
        private const float Distance = 14f; // 相机距角色距离

        [MenuItem(MenuPath)]
        public static void Build()
        {
            Transform follow = FindFollowTarget();
            if (follow == null)
            {
                Debug.LogError("[CinemachineTopDownBuilder] 场景中未找到 Player（带 PlayerMover 的角色）。先建 Player 再执行。");
                return;
            }

            Camera mainCam = Camera.main;
            if (mainCam == null)
            {
                Debug.LogError("[CinemachineTopDownBuilder] 未找到 Main Camera。");
                return;
            }

            // Main Camera 挂 Brain（驱动 CinemachineCamera）
            if (mainCam.GetComponent<CinemachineBrain>() == null)
                mainCam.gameObject.AddComponent<CinemachineBrain>();

            // 创建/复用 VirtualCamera（new GameObject 默认进 active scene）
            GameObject vcamGo = GameObject.Find(VcamName);
            if (vcamGo == null) vcamGo = new GameObject(VcamName);

            CinemachineCamera vc = vcamGo.GetComponent<CinemachineCamera>();
            if (vc == null) vc = vcamGo.AddComponent<CinemachineCamera>();

            // 跟随 + 固定俯角旋转（相机看向地面）
            vc.Follow = follow;
            vcamGo.transform.rotation = Quaternion.Euler(Pitch, Yaw, 0f);

            // Body = PositionComposer（Cinemachine 3.x 替代废弃的 FramingTransposer）：硬跟随、无死区、无阻尼
            CinemachinePositionComposer body =
                vcamGo.GetComponent<CinemachinePositionComposer>();
            if (body == null)
                body = vcamGo.AddComponent<CinemachinePositionComposer>();

            body.CameraDistance = Distance;
            body.Damping = Vector3.zero;              // 3.x 合并 XYZ damping 为 Vector3，默认(1,1,1)需清零
            body.DeadZoneDepth = 0f;
            body.CenterOnActivate = false;
            // 死区：3.x 重组进 Composition.DeadZone（Enabled+Size），置零=无死区
            var comp = body.Composition;
            var dz = comp.DeadZone;
            dz.Enabled = false;
            dz.Size = Vector2.zero;
            comp.DeadZone = dz;
            body.Composition = comp;

            // Aim：故意不挂 Aim 组件 → 旋转固定，保持上方设定的俯角

            EditorUtility.SetDirty(vcamGo);
            EditorUtility.SetDirty(mainCam.gameObject);
            EditorSceneManager.MarkSceneDirty(mainCam.gameObject.scene);
            Debug.Log($"[CinemachineTopDownBuilder] 俯视相机就绪: Follow={follow.name}, Pitch={Pitch}°, Distance={Distance}（Aim 固定 / 无死区 / 无阻尼）");
        }

        private static Transform FindFollowTarget()
        {
            var mover = Object.FindObjectOfType<DontPushTheButton.Player.PlayerAbilityController>();
            if (mover != null) return mover.transform;
            GameObject go = GameObject.Find("Player");
            return go != null ? go.transform : null;
        }
    }
}
#endif
