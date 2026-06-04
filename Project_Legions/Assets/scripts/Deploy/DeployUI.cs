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

        [Header("卡组轮换（可选，不配则展示全部 _cards）")]
        [SerializeField] private DeckManager _deckManager;

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
        [SerializeField] private Color _cardBgDisabled = new Color(0.1f, 0.1f, 0.1f, 0.6f);
        [SerializeField] private Color _ghostValid = new Color(0f, 1f, 0f, 0.6f);
        [SerializeField] private Color _ghostInvalid = new Color(1f, 0f, 0f, 0.6f);

        private bool _isDragging;
        private UnitData _dragData;
        private Sprite _dragCardSprite;
        private GameObject _ghost;
        private SpriteRenderer _ghostRenderer;
        private GridPosition _dragGridPos;
        private bool _isInFieldZone;
        private int _dragCardIndex = -1;

        private void Start()
        {
            if (_deckManager == null)
                _deckManager = FindObjectOfType<DeckManager>();
        }

        private void Update()
        {
            if (!_isDragging) return;

            Vector3 mousePos = Input.mousePosition;
            float guiMouseX = mousePos.x;
            float guiMouseY = Screen.height - mousePos.y;
            bool inPanel = guiMouseX >= _panelX && guiMouseX <= _panelX + _panelWidth
                        && guiMouseY >= _panelY && guiMouseY <= _panelY + _panelHeight;
            bool inField = !inPanel;
            if (inField != _isInFieldZone)
            {
                _isInFieldZone = inField;
                UpdateGhostSprite();
            }

            if (_isInFieldZone)
                UpdateFieldDrag();
            else
                UpdateCardDrag();

            if (Input.GetMouseButtonUp(0))
                EndDrag(false);
            else if (Input.GetMouseButtonDown(1))
                EndDrag(true);
        }

        private void UpdateGhostSprite()
        {
            if (_ghostRenderer == null) return;
            SpriteRenderer src = _dragData.prefab.GetComponentInChildren<SpriteRenderer>();
            _ghostRenderer.sprite = src != null ? src.sprite : _dragData.icon;
        }

        private void UpdateFieldDrag()
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = -3;

            int mouseCol = GridManager.Instance.WorldToGrid(mouseWorld).col;
            _dragGridPos = FindNearestValidCol(mouseCol);

            if (_dragGridPos.col >= 0)
            {
                float x = GridManager.Instance.GridToWorldX(_dragGridPos);
                _ghost.transform.position = new Vector3(x, DeploySystem.Instance.PlayerSpawnY, -3);
                _ghost.SetActive(true);

                bool valid = DeploySystem.Instance.CanPlaceUnit(_dragData, _dragGridPos);
                _ghostRenderer.color = valid ? _ghostValid : _ghostInvalid;
            }
            else
            {
                _ghost.SetActive(false);
            }
        }

        private GridPosition FindNearestValidCol(int centerCol)
        {
            int minCol = DeploySystem.Instance.GetPlaceableColMin();
            int maxCol = DeploySystem.Instance.GetMaxDeployCol();

            for (int offset = 0; offset <= 12; offset++)
            {
                int left = centerCol - offset;
                int right = centerCol + offset;

                if (left >= minCol)
                {
                    GridPosition pos = new GridPosition(left);
                    if (DeploySystem.Instance.CanPlaceUnit(_dragData, pos))
                        return pos;
                }
                if (right != left && right <= maxCol)
                {
                    GridPosition pos = new GridPosition(right);
                    if (DeploySystem.Instance.CanPlaceUnit(_dragData, pos))
                        return pos;
                }
            }

            return new GridPosition(-1);
        }

        private void UpdateCardDrag()
        {
            if (_ghost != null) _ghost.SetActive(false);
        }

        private void StartDrag(UnitData data)
        {
            if (_isDragging) return;
            if (GameManager.Instance.State != GameState.Deploy && GameManager.Instance.State != GameState.Battle) return;
            if (DeploySystem.Instance == null || DeploySystem.Instance.Energy < data.deployCost) return;

            _isDragging = true;
            _dragData = data;

            if (_deckManager != null)
                _dragCardIndex = GetHandIndex(data);
            else
                _dragCardIndex = System.Array.FindIndex(_cards, e => e != null && e.unitData == data);

            _dragCardSprite = GetCardSprite(data);

            _ghost = new GameObject("DeployGhost");
            _ghost.transform.SetParent(transform);
            _ghostRenderer = _ghost.AddComponent<SpriteRenderer>();
            _ghostRenderer.sortingOrder = 32767;

            Vector3 startMouse = Input.mousePosition;
            float sguiX = startMouse.x;
            float sguiY = Screen.height - startMouse.y;
            _isInFieldZone = !(sguiX >= _panelX && sguiX <= _panelX + _panelWidth
                            && sguiY >= _panelY && sguiY <= _panelY + _panelHeight);
            UpdateGhostSprite();

            if (_isInFieldZone)
                UpdateFieldDrag();
            else
                UpdateCardDrag();
        }

        private Sprite GetCardSprite(UnitData data)
        {
            if (_cards != null)
            {
                foreach (var e in _cards)
                {
                    if (e != null && e.unitData == data && e.cardImage != null)
                        return e.cardImage;
                }
            }
            if (data.icon != null) return data.icon;
            SpriteRenderer sr = data.prefab?.GetComponentInChildren<SpriteRenderer>();
            return sr != null ? sr.sprite : null;
        }

        private int GetHandIndex(UnitData data)
        {
            var hand = _deckManager?.Hand;
            if (hand == null) return -1;
            for (int i = 0; i < hand.Count; i++)
            {
                if (hand[i] == data) return i;
            }
            return -1;
        }

        private void EndDrag(bool cancelled)
        {
            if (!_isDragging) return;

            if (!cancelled && _isInFieldZone && _dragGridPos.col >= 0 && DeploySystem.Instance.CanPlaceUnit(_dragData, _dragGridPos))
            {
                DeploySystem.Instance.QueueDeploy(_dragData, _dragGridPos, _ghost);
                _ghost = null;
                _ghostRenderer = null;

                if (_deckManager != null && _dragCardIndex >= 0)
                    _deckManager.UseCard(_dragCardIndex);
            }
            else
            {
                if (_ghost != null)
                    Destroy(_ghost);
            }

            _isDragging = false;
            _dragData = null;
            _dragCardIndex = -1;
            _ghost = null;
            _ghostRenderer = null;
        }

        private void OnGUI()
        {
            if (GameManager.Instance == null || DeploySystem.Instance == null) return;
            if (GameManager.Instance.State != GameState.Deploy && GameManager.Instance.State != GameState.Battle) return;

            var hand = _deckManager != null ? _deckManager.Hand : null;
            bool useRotation = hand != null && hand.Count > 0;

            int displayCount = useRotation ? hand.Count : (_cards != null ? _cards.Length : 0);
            if (displayCount == 0) return;

            int totalW = displayCount * _cardWidth + (displayCount - 1) * _cardGap;
            if (totalW > _panelWidth)
            {
                _cardGap = Mathf.Max(2, (_panelWidth - displayCount * _cardWidth) / (displayCount - 1));
                totalW = displayCount * _cardWidth + (displayCount - 1) * _cardGap;
            }
            int offsetX = (_panelWidth - totalW) / 2;

            GUI.Box(new Rect(_panelX, _panelY, _panelWidth, _panelHeight), "");

            float cardY = _panelY + (_panelHeight - _cardHeight) / 2;

            for (int i = 0; i < displayCount; i++)
            {
                UnitData data = useRotation ? hand[i] : (_cards[i] != null ? _cards[i].unitData : null);
                if (data == null) continue;

                if (_isDragging && i == _dragCardIndex && useRotation)
                {
                    Rect slotRect = new Rect(_panelX + offsetX + i * (_cardWidth + _cardGap), cardY, _cardWidth, _cardHeight);
                    GUI.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
                    GUI.DrawTexture(slotRect, Texture2D.whiteTexture);
                    GUI.color = Color.white;
                    continue;
                }

                Rect cardRect = new Rect(_panelX + offsetX + i * (_cardWidth + _cardGap), cardY, _cardWidth, _cardHeight);
                bool canAfford = DeploySystem.Instance.Energy >= data.deployCost;
                bool disabled = false;

                Color borderColor = disabled ? _cardBorderDisabled : (canAfford ? _cardBorderAfford : _cardBorderNormal);
                GUI.color = borderColor;
                GUI.DrawTexture(cardRect, Texture2D.whiteTexture);
                GUI.color = Color.white;

                Rect bgRect = new Rect(cardRect.x + 2, cardRect.y + 2, cardRect.width - 4, cardRect.height - 4);
                GUI.color = disabled ? _cardBgDisabled : (canAfford ? _cardBg : Color.Lerp(_cardBg, Color.black, 0.4f));
                GUI.DrawTexture(bgRect, Texture2D.whiteTexture);
                GUI.color = Color.white;

                Sprite cardSprite = GetCardSprite(data);
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
                labelStyle.normal.textColor = disabled ? Color.gray : (canAfford ? Color.white : Color.gray);
                GUI.Label(new Rect(cardRect.x, cardRect.y + _cardIconSize + 12, cardRect.width, 20), displayName, labelStyle);

                GUIStyle costStyle = new GUIStyle(GUI.skin.label);
                costStyle.fontSize = 12;
                costStyle.alignment = TextAnchor.LowerCenter;
                costStyle.normal.textColor = disabled ? Color.gray : (canAfford ? Color.yellow : Color.gray);
                GUI.Label(new Rect(cardRect.x, cardRect.y + _cardHeight - 22, cardRect.width, 18), $"\u8d39{data.deployCost}", costStyle);

                if (!_isDragging && !disabled && Event.current.type == EventType.MouseDown && cardRect.Contains(Event.current.mousePosition))
                {
                    StartDrag(data);
                    Event.current.Use();
                }
            }

            if (_isDragging && !_isInFieldZone && _dragData != null)
            {
                Vector2 mp = Event.current.mousePosition;
                Rect dcRect = new Rect(mp.x - _cardWidth / 2, mp.y - _cardHeight / 2, _cardWidth, _cardHeight);

                GUI.color = _cardBorderAfford;
                GUI.DrawTexture(dcRect, Texture2D.whiteTexture);
                GUI.color = Color.white;

                Rect dbg = new Rect(dcRect.x + 2, dcRect.y + 2, dcRect.width - 4, dcRect.height - 4);
                GUI.color = _cardBg;
                GUI.DrawTexture(dbg, Texture2D.whiteTexture);
                GUI.color = Color.white;

                if (_dragCardSprite != null)
                {
                    float ix = dcRect.x + (dcRect.width - _cardIconSize) * 0.5f;
                    float iy = dcRect.y + 8;
                    GUI.DrawTexture(new Rect(ix, iy, _cardIconSize, _cardIconSize), _dragCardSprite.texture);
                }

                string dn = !string.IsNullOrEmpty(_dragData.unitName) ? _dragData.unitName : _dragData.name;
                GUIStyle ls = new GUIStyle(GUI.skin.label);
                ls.fontSize = 14;
                ls.alignment = TextAnchor.UpperCenter;
                ls.normal.textColor = Color.white;
                GUI.Label(new Rect(dcRect.x, dcRect.y + _cardIconSize + 12, dcRect.width, 20), dn, ls);

                GUIStyle cs = new GUIStyle(GUI.skin.label);
                cs.fontSize = 12;
                cs.alignment = TextAnchor.LowerCenter;
                cs.normal.textColor = Color.yellow;
                GUI.Label(new Rect(dcRect.x, dcRect.y + _cardHeight - 22, dcRect.width, 18), $"\u8d39{_dragData.deployCost}", cs);
            }
        }
    }
}
