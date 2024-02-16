using UnityEngine;

namespace CameraController
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform _player;
        [SerializeField] private float _smoothTime = 0.3f;
        [SerializeField] private Vector3 _offset = new Vector3(0, 1);
        [SerializeField] private float _lookAheadDistance = 2;
        [SerializeField] private float _lookAheadSpeed = 1;
        [SerializeField] private int _zGoal = -10;

        private Vector3 _velOffset;
        private Vector3 _vel;
        private TarodevController.IPlayerController _playerController;
        private Vector3 _lookAheadVel;

        private void Awake() => _player.TryGetComponent(out _playerController);

        private void LateUpdate()
        {
            if (_playerController != null)
            {
                var projectedPos = _playerController.Velocity.normalized * _lookAheadDistance;
                _velOffset = Vector3.SmoothDamp(_velOffset, projectedPos, ref _lookAheadVel, _lookAheadSpeed);
            }

            Step(_smoothTime);
        }

        private void OnValidate() => Step(0);

        private void Step(float time)
        {
            var goal = _player.position + _offset + _velOffset;
            goal.z = _zGoal;
            transform.position = Vector3.SmoothDamp(transform.position, goal, ref _vel, time);
        }
    }
}