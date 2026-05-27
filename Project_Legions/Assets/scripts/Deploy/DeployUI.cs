using UnityEngine;

namespace PPCorps
{
    [System.Serializable]
    public class DeployCardEntry
    {
        public UnitData unitData;
        public Sprite cardImage;
    }

    public class DeployUI : MonoBehaviour
    {
        [Header("卡牌配置")]
        [SerializeField] private DeployCardEntry[] _cards;

        [Header("面板位置")]
        [SerializeField] private int _panelX;
        [SerializeField] private int _panelY;
        [SerializeField] private int _panelWidth = 800;
        [SerializeField] private int _panelHeight = 130;

        [Header("卡牌外观")]
        [SerializeField] private int _cardWidth = 80;
        [SerializeField] private int _cardHeight = 100;
        [SerializeField] private int _cardGap = 10;
        [SerializeField] private int _cardIconSize = 50;
        [SerializeField] private Color _cardBg = new Color(0.15f, 0.15f, 0.15f, 0.9f);
        [SerializeField] private Color _cardBorderNormal = new Color(0.5f, 0.5f, 0.5f);
        [SerializeField] private Color _cardBorderAfford = new Color(0f, 1f, 0f);
        [SerializeField] private Color _cardBorderDisabled = new Color(0.3f, 0.3f, 0.3f);
        [SerializeField] private Color _ghostValid = new Color(0f, 1f, 0f, 0.6f);
        [SerializeField] private Color _ghostInvalid = new Color(1f, 0f, 0f, 0.6f);

        private bool _isDragging;
        private UnitData _dragData;
        private GameObject _ghost;
        private SpriteRenderer _ghostRenderer;
        private GridPosition _dragGridPos;

        private void Update()
        {
            if (!_isDragging) return;

            UpdateDrag();

            if (Input.GetMouseButtonUp(0))
                EndDrag(false);
            else if (Input.GetMouseButtonDown(1))
                EndDrag(true);
        }

        private void StartDrag(UnitData data)
        {
            if (_isDragging) return;
            if (GameManager.Instance.State != GameState.Deploy && GameManager.Instance.State != GameState.Battle) return;
            if (DeploySystem.Instance == null || DeploySystem.Instance.Energy < data.deployCost) return;

            _isDragging = true;
            _dragData = data;

            _ghost = new GameObject("DeployGhost");
            _ghost.transform.SetParent(transform);
            _ghostRenderer = _ghost.AddComponent<SpriteRenderer>();

            SpriteRenderer src = data.prefab.GetComponentInChildren<SpriteRenderer>();
            _ghostRenderer.sprite = src != null ? src.sprite : data.icon;
            _ghostRenderer.sortingOrder = 32767;

            UpdateDrag();
        }

        private void UpdateDrag()
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;

            int col = GridManager.Instance.WorldToGrid(mouseWorld).col;
            _dragGridPos = new GridPosition(col);

            float x = GridManager.Instance.GridToWorldX(_dragGridPos);
            _ghost.transform.position = new Vector3(x, DeploySystem.Instance.PlayerSpawnY, 0);

            bool valid = DeploySystem.Instance.CanPlaceUnit(_dragData, _dragGridPos);
            _ghostRenderer.color = valid ? _ghostValid : _ghostInvalid;
        }

        private void EndDrag(bool cancelled)
        {
            if (!_isDragging) return;

            if (!cancelled && DeploySystem.Instance.CanPlaceUnit(_dragData, _dragGridPos))
                DeploySystem.Instance.PlaceUnit(_dragData, _dragGridPos);

            if (_ghost != null)
                Destroy(_ghost);

            _isDragging = false;
            _dragData = null;
            _ghost = null;
            _ghostRenderer = null;
        }

        private void OnGUI()
        {
            if (GameManager.Instance == null || DeploySystem.Instance == null) return;
            if (GameManager.Instance.State != GameState.Deploy && GameManager.Instance.State != GameState.Battle) return;
            if (_cards == null || _cards.Length == 0) return;

            int totalW = _cards.Length * _cardWidth + (_cards.Length - 1) * _cardGap;
            if (totalW > _panelWidth)
            {
                _cardGap = Mathf.Max(2, (_panelWidth - _cards.Length * _cardWidth) / (_cards.Length - 1));
                totalW = _cards.Length * _cardWidth + (_cards.Length - 1) * _cardGap;
            }
            int offsetX = (_panelWidth - totalW) / 2;

            GUI.Box(new Rect(_panelX, _panelY, _panelWidth, _panelHeight), "");

            float cardY = _panelY + (_panelHeight - _cardHeight) / 2;

            for (int i = 0; i < _cards.Length; i++)
            {
                DeployCardEntry entry = _cards[i];
                if (entry == null || entry.unitData == null) continue;

                UnitData data = entry.unitData;
                Rect cardRect = new Rect(_panelX + offsetX + i * (_cardWidth + _cardGap), cardY, _cardWidth, _cardHeight);
                bool canAfford = DeploySystem.Instance.Energy >= data.deployCost;

                Color borderColor = _isDragging ? _cardBorderDisabled : (canAfford ? _cardBorderAfford : _cardBorderNormal);
                GUI.color = borderColor;
                GUI.DrawTexture(cardRect, Texture2D.whiteTexture);
                GUI.color = Color.white;

                Rect bgRect = new Rect(cardRect.x + 2, cardRect.y + 2, cardRect.width - 4, cardRect.height - 4);
                GUI.color = canAfford ? _cardBg : Color.Lerp(_cardBg, Color.black, 0.4f);
                GUI.DrawTexture(bgRect, Texture2D.whiteTexture);
                GUI.color = Color.white;

                Sprite cardSprite = entry.cardImage;
                if (cardSprite == null)
                {
                    if (data.icon != null)
                        cardSprite = data.icon;
                    else
                    {
                        SpriteRenderer sr = data.prefab?.GetComponentInChildren<SpriteRenderer>();
                        if (sr != null) cardSprite = sr.sprite;
                    }
                }
                if (cardSprite != null)
                {
                    float iconX = cardRect.x + (cardRect.width - _cardIconSize) * 0.5f;
                    float iconY = cardRect.y + 8;
                    GUI.DrawTexture(new Rect(iconX, iconY, _cardIconSize, _cardIconSize), cardSprite.texture);
                }

                string displayName = !string.IsNullOrEmpty(data.unitName) ? data.unitName : data.name;
                GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.fontSize = 14;
                labelStyle.alignment = TextAnchor.UpperCenter;
                labelStyle.normal.textColor = canAfford ? Color.white : Color.gray;
                GUI.Label(new Rect(cardRect.x, cardRect.y + _cardIconSize + 12, cardRect.width, 20), displayName, labelStyle);

                GUIStyle costStyle = new GUIStyle(GUI.skin.label);
                costStyle.fontSize = 12;
                costStyle.alignment = TextAnchor.LowerCenter;
                costStyle.normal.textColor = canAfford ? Color.yellow : Color.gray;
                GUI.Label(new Rect(cardRect.x, cardRect.y + _cardHeight - 22, cardRect.width, 18), $"费{data.deployCost}", costStyle);

                if (!_isDragging && Event.current.type == EventType.MouseDown && cardRect.Contains(Event.current.mousePosition))
                {
                    StartDrag(data);
                    Event.current.Use();
                }
            }
        }
    }
}
