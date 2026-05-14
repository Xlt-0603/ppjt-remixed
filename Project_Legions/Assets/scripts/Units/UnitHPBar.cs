using UnityEngine;

namespace PPCorps
{
    public class UnitHPBar : MonoBehaviour
    {
        [SerializeField] private Vector2 _offset = new Vector2(0, 1.2f);
        [SerializeField] private int _barWidth = 60;
        [SerializeField] private int _barHeight = 8;

        private UnitBase _unit;
        private Camera _camera;

        private void Start()
        {
            _unit = GetComponent<UnitBase>();
            _camera = Camera.main;
        }

        private void OnGUI()
        {
            if (_unit == null || _unit.IsDead || _camera == null) return;

            Vector3 worldPos = transform.position + (Vector3)_offset;
            Vector3 screenPos = _camera.WorldToScreenPoint(worldPos);

            if (screenPos.z < 0) return;

            float hpRatio = (float)_unit.CurrentHP / _unit.MaxHP;
            if (hpRatio < 0) hpRatio = 0;

            float barX = screenPos.x - _barWidth / 2f;
            float barY = Screen.height - screenPos.y - _barHeight / 2f;

            GUI.color = Color.red;
            GUI.DrawTexture(new Rect(barX, barY, _barWidth, _barHeight), Texture2D.whiteTexture);

            GUI.color = Color.green;
            GUI.DrawTexture(new Rect(barX, barY, _barWidth * hpRatio, _barHeight), Texture2D.whiteTexture);

            GUI.color = Color.white;
        }
    }
}
