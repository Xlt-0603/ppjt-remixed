using UnityEngine;

namespace PPCorps
{
    public class VillageCameraController : MonoBehaviour
    {
        [SerializeField] private float _minX = -49.5f;
        [SerializeField] private float _maxX = 26.6f;
        [SerializeField] private float _scrollAmount = 40f;
        [SerializeField] private float _smoothTime = 0.15f;
        [SerializeField] private float _elasticStrength = 5f;
        [SerializeField] private float _dragResistance = 0.3f;
        [SerializeField] private float _maxOvershoot = 15f;

        private Camera _camera;
        private Vector3 _targetPosition;
        private Vector3 _dragWorldOrigin;
        private bool _isDragging;
        private Vector3 _velocity;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _targetPosition = transform.position;
        }

        private void Update()
        {
            HandleDrag();
            ApplyElasticSpring();
            transform.position = Vector3.SmoothDamp(
                transform.position, _targetPosition, ref _velocity, _smoothTime);
        }

        private void ApplyElasticSpring()
        {
            float target = _targetPosition.x;
            bool outOfBounds = false;

            if (target < _minX)
            {
                target = _minX;
                outOfBounds = true;
            }
            else if (target > _maxX)
            {
                target = _maxX;
                outOfBounds = true;
            }

            if (outOfBounds)
            {
                float strength = _isDragging ? _elasticStrength * 0.2f : _elasticStrength;
                _targetPosition.x = Mathf.Lerp(_targetPosition.x, target, Time.deltaTime * strength);
            }
        }

        private void HandleDrag()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _dragWorldOrigin = _camera.ScreenToWorldPoint(Input.mousePosition);
                _isDragging = true;
            }
            else if (Input.GetMouseButton(0) && _isDragging)
            {
                Vector3 currentWorld = _camera.ScreenToWorldPoint(Input.mousePosition);
                float rawDelta = _dragWorldOrigin.x - currentWorld.x;
                float deltaX = rawDelta;

                float overshoot = 0f;
                if (_targetPosition.x < _minX)
                    overshoot = _minX - _targetPosition.x;
                else if (_targetPosition.x > _maxX)
                    overshoot = _targetPosition.x - _maxX;

                if (overshoot > 0.01f)
                {
                    float t = Mathf.Clamp01(overshoot / _maxOvershoot);
                    deltaX *= Mathf.Lerp(1f, _dragResistance, t);
                }

                if (Mathf.Abs(deltaX) > 0.01f)
                {
                    _targetPosition.x += deltaX;
                    _dragWorldOrigin = currentWorld;
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                _isDragging = false;
            }
        }

        public void ScrollLeft()
        {
            _targetPosition.x -= _scrollAmount;
        }

        public void ScrollRight()
        {
            _targetPosition.x += _scrollAmount;
        }

        public void SetBounds(float minX, float maxX)
        {
            _minX = minX;
            _maxX = maxX;
        }
    }
}
