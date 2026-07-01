using System.Collections;
using UnityEngine;

namespace DontPushTheButton.Core
{
    /// <summary>
    /// 机关柱升降（M4.10）：接 ButtonTrigger.OnPressed → 升起位移 _liftHeight 当踏板。
    /// 协程插值 y 位置，无 DOTween 依赖。默认不接 OnReleased（柱升起后保持，符合"推箱搭路"惯性）。
    /// </summary>
    public class PillarLift : MonoBehaviour
    {
        [Tooltip("升起位移（Y 轴正向，世界单位）")]
        [SerializeField] private float _liftHeight = 15f;
        [Tooltip("升降时长（秒）")]
        [SerializeField] private float _duration = 0.8f;
        [Tooltip("升起到位后，Button 松开时是否降回（接 OnReleased → Lower）")]
        [SerializeField] private bool _returnOnRelease = false;

        private float _originY;
        private bool _lifted = false;
        private Coroutine _coroutine;

        private void Awake() { _originY = transform.position.y; }

        /// <summary>升起到 _originY + _liftHeight（接 ButtonTrigger.OnPressed）。</summary>
        public void Lift()
        {
            if (_lifted) return;
            _lifted = true;
            StartMove(_originY + _liftHeight);
        }

        /// <summary>降回原位（仅 _returnOnRelease=true 时接 ButtonTrigger.OnReleased）。</summary>
        public void Lower()
        {
            if (!_lifted) return;
            _lifted = false;
            StartMove(_originY);
        }

        private void StartMove(float targetY)
        {
            if (_coroutine != null) StopCoroutine(_coroutine);
            _coroutine = StartCoroutine(MoveY(targetY));
        }

        private IEnumerator MoveY(float targetY)
        {
            Vector3 from = transform.position;
            Vector3 to = new Vector3(from.x, targetY, from.z);
            float t = 0f;
            while (t < _duration)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(from, to, t / _duration);
                yield return null;
            }
            transform.position = to;
            _coroutine = null;
        }
    }
}
