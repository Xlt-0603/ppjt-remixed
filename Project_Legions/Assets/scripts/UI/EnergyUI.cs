using UnityEngine;
using UnityEngine.UI;

namespace PPCorps
{
    public class EnergyUI : MonoBehaviour
    {
        [Header("UI 组件")]
        [SerializeField] private Image _progressFill;

        [Header("参数")]
        [SerializeField] private float _fillDuration = 15f;

        private float _timer;

        private void Start()
        {
            _timer = 0f;
        }

        private void Update()
        {
            _timer += Time.deltaTime;

            if (_timer >= _fillDuration)
                _timer -= _fillDuration;

            if (_progressFill != null)
                _progressFill.fillAmount = Mathf.Clamp01(_timer / _fillDuration);
        }
    }
}
