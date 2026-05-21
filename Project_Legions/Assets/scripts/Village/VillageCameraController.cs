using UnityEngine;

namespace PPCorps
{
    public class VillageCameraController : MonoBehaviour
    {
        [SerializeField] private float _dragSpeed = 1f;
        [SerializeField] private float _minX = -30f;
        [SerializeField] private float _maxX = 30f;
        [SerializeField] private float _scrollAmount = 12f;
        [SerializeField] private float _smoothTime = 0.15f;

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
            transform.position = Vector3.SmoothDamp(
                transform.position, _targetPosition, ref _velocity, _smoothTime);
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
                float deltaX = _dragWorldOrigin.x - currentWorld.x;
                if (Mathf.Abs(deltaX) > 0.1f)
                {
                    _targetPosition.x += deltaX;
                    _targetPosition.x = Mathf.Clamp(_targetPosition.x, _minX, _maxX);
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
            _targetPosition.x = Mathf.Clamp(_targetPosition.x, _minX, _maxX);
        }

        public void ScrollRight()
        {
            _targetPosition.x += _scrollAmount;
            _targetPosition.x = Mathf.Clamp(_targetPosition.x, _minX, _maxX);
        }

        public void SetBounds(float minX, float maxX)
        {
            _minX = minX;
            _maxX = maxX;
        }
    }
}
