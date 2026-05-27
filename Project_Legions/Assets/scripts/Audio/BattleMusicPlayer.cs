using UnityEngine;

namespace PPCorps
{
    [RequireComponent(typeof(AudioSource))]
    public class BattleMusicPlayer : MonoBehaviour
    {
        [SerializeField] private AudioClip _musicClip;

        private AudioSource _audioSource;
        private bool _wasPaused;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioSource.loop = true;
            _audioSource.clip = _musicClip;
        }

        private void Start()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged += OnStateChanged;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged -= OnStateChanged;
        }

        private void OnStateChanged(GameState state)
        {
            if (state == GameState.Battle)
            {
                _audioSource.Play();
            }
            else
            {
                _audioSource.Stop();
            }
        }

        private void Update()
        {
            bool paused = GameManager.Instance.IsPaused;
            if (paused != _wasPaused)
            {
                _wasPaused = paused;
                if (paused)
                    _audioSource.Pause();
                else if (GameManager.Instance.State == GameState.Battle)
                    _audioSource.UnPause();
            }
        }
    }
}
