using System;
using UnityEngine;
using UnityEngine.Video;

namespace PPCorps
{
    [System.Serializable]
    public class RarityVideoMapping
    {
        public Rarity rarity;
        public VideoClip videoClip;
    }

    public class GachaResultPage : MonoBehaviour
    {
        [Header("结果页面根物体（拖入空物体占位，以后替换为视频播放器）")]
        public GameObject pageRoot;

        [Header("品级→视频（留空待填）")]
        public RarityVideoMapping[] rarityVideos;

        [Header("跳过设置")]
        public string skipButtonText = "跳过";
        public float autoSkipTime = 0f;

        private Action _onComplete;
        private Rarity _currentRarity;
        private VideoPlayer _videoPlayer;
        private bool _isPlaying;
        private float _playTimer;

        private void Awake()
        {
            if (pageRoot != null)
            {
                _videoPlayer = pageRoot.GetComponentInChildren<VideoPlayer>(true);
                pageRoot.SetActive(false);
            }
        }

        public void Play(Rarity highestRarity, Action onComplete)
        {
            _currentRarity = highestRarity;
            _onComplete = onComplete;
            _isPlaying = true;
            _playTimer = 0f;

            if (pageRoot != null)
                pageRoot.SetActive(true);

            if (_videoPlayer != null)
            {
                var clip = GetVideoClip(highestRarity);
                if (clip != null)
                {
                    _videoPlayer.clip = clip;
                    _videoPlayer.Play();
                }
            }
        }

        private void OnGUI()
        {
            if (!_isPlaying) return;

            GUI.Label(new Rect(Screen.width / 2f - 100, Screen.height - 100, 200, 30), "抽卡结果页面（占位）", new GUIStyle
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter,
                normal = new GUIStyleState { textColor = Color.gray }
            });

            if (!string.IsNullOrEmpty(skipButtonText))
            {
                float btnW = 100f;
                float btnH = 30f;
                float btnX = Screen.width / 2f - btnW / 2f;
                float btnY = Screen.height - 60f;
                if (GUI.Button(new Rect(btnX, btnY, btnW, btnH), skipButtonText))
                    Complete();
            }

            if (autoSkipTime > 0f)
            {
                _playTimer += Time.unscaledDeltaTime;
                if (_playTimer >= autoSkipTime)
                    Complete();
            }
        }

        public void Complete()
        {
            _isPlaying = false;

            if (_videoPlayer != null)
                _videoPlayer.Stop();

            if (pageRoot != null)
                pageRoot.SetActive(false);

            _onComplete?.Invoke();
        }

        private VideoClip GetVideoClip(Rarity rarity)
        {
            if (rarityVideos == null) return null;
            foreach (var m in rarityVideos)
                if (m.rarity == rarity) return m.videoClip;
            return null;
        }
    }
}
