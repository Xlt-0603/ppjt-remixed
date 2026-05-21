# 砰砰军团 — 村庄主界面架构设计

> 本文档定义村庄主界面（砰砰村）的软件架构，包括场景管理、模块划分、数据流和编码规范。
> 基于现有项目（Unity 2022.3 + URP 2D，命名空间 `PPCorps`）扩展。

---

## 一、整体架构

### 1.1 场景策略：双场景模式

```
启动 → VillageScene（村庄主界面）
         │
         ├─ 点击"战斗" → LoadSceneAsync("BattleScene")
         │     战斗结束后 → LoadSceneAsync("VillageScene")
         │
         ├─ 点击"食堂" → 打开食堂 UI Panel（叠加在村庄上）
         ├─ 点击"渔场" → 打开渔场 UI Panel（叠加在村庄上）
         └─ 其余功能 → 各自 Panel / SubScene
```

| 场景 | 文件 | 说明 |
|------|------|------|
| `VillageScene` | `Scenes/VillageScene.unity` | 村庄主界面，常驻场景 |
| `BattleScene` | `Scenes/SampleScene.unity` | 战斗场景（现有，后续改造） |

> **注意**：不拆分多场景加载（ additive load），采用简单场景切换，各场景独立 Manager。
> 跨场景数据通过 `VillageDataManager`（持久化单例）传递。

### 1.2 分层架构

```
┌──────────────────────────────────────────────────────────────────┐
│                        UI 表现层 (UGUI)                           │
│  ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐ ┌──────────┐      │
│  │底部工具栏│ │建筑面板 │ │装饰面板 │ │弹窗系统 │ │对话气泡  │      │
│  └────────┘ └────────┘ └────────┘ └────────┘ └──────────┘      │
├──────────────────────────────────────────────────────────────────┤
│                        Manager 逻辑层                            │
│  ┌───────────┐ ┌────────────┐ ┌──────────┐ ┌──────────────┐    │
│  │VillageMgr │ │BuildingMgr │ │DecoMgr   │ │豆丁AnimMgr   │    │
│  └───────────┘ └────────────┘ └──────────┘ └──────────────┘    │
├──────────────────────────────────────────────────────────────────┤
│                        数据层                                     │
│  ┌────────────────┐ ┌────────────────┐ ┌──────────────────┐    │
│  │VillageDataSO   │ │BuildingDataSO  │ │VillageSaveData   │    │
│  └────────────────┘ └────────────────┘ └──────────────────┘    │
│                        ↑ 持久化 (PlayerPrefs / JSON)            │
└──────────────────────────────────────────────────────────────────┘
```

### 1.3 与现有系统的集成

```
GameManager (战斗用单例)
  └─ 仅在 BattleScene 中激活
  └─ 不参与村庄逻辑

新增 VillageManager (村庄用单例)
  └─ 仅在 VillageScene 中激活
  └─ DontDestroyOnLoad 可选，用于跨场景持有村庄数据

共享资源：
  ├─ PPCorps 命名空间
  ├─ GridManager (仅在战斗中)
  ├─ UnitData / UnitBase (战斗单位，村庄预览可引用)
  └─ UGUI (com.unity.ugui)
```

---

## 二、模块划分

### 2.1 模块清单

| 模块 | 类名 | 职责 |
|------|------|------|
| **村庄主控** | `VillageManager` | 村庄整体状态管理，子模块协调 |
| **数据持久化** | `VillageDataManager` | 数据加载/保存，跨场景传递 |
| **建筑系统** | `VillageBuilding` | 建筑实体（可点击、升级、展示） |
| **装饰系统** | `VillageDecoration` | 装饰物（静态 + 交互） |
| **豆丁漫步** | `VillageWalker` | 豆丁在村庄中随机走动/互动 |
| **底部工具栏** | `VillageToolbarUI` | 底部功能按钮栏 |
| **建筑面板** | `BuildingPanelUI` | 点击建筑后弹出的功能面板 |
| **资源栏** | `ResourceBarUI` | 顶部金币/宝石/科技点显示 |
| **对话气泡** | `ChatBubbleUI` | 豆丁随机对话气泡 |

---

## 三、VillageManager — 村庄主控

### 3.1 职责

- 村庄场景初始化
- 管理当前打开的 UI 面板（互斥）
- 协调 BuildingMgr / DecoMgr / WalkerMgr 子模块
- 暴露村庄状态（等级、金币、建筑列表等）

### 3.2 核心定义

```csharp
namespace PPCorps
{
    public class VillageManager : MonoBehaviour
    {
        public static VillageManager Instance { get; private set; }

        [Header("村庄数据")]
        [SerializeField] private VillageDataSO _villageData;
        [SerializeField] private int _villageLevel = 1;
        [SerializeField] private CurrencyData _currency;

        [Header("场景引用")]
        [SerializeField] private Transform _buildingRoot;      // 建筑父物体
        [SerializeField] private Transform _decorationRoot;     // 装饰父物体
        [SerializeField] private Transform _walkerRoot;         // 豆丁父物体

        [Header("UI 引用")]
        [SerializeField] private ResourceBarUI _resourceBar;
        [SerializeField] private VillageToolbarUI _toolbar;

        // 子模块引用
        private BuildingManager _buildingMgr;
        private DecorationManager _decoMgr;
        private WalkerManager _walkerMgr;

        // 状态
        public int VillageLevel => _villageLevel;
        public CurrencyData Currency => _currency;
        public VillageState State { get; private set; }

        public event Action<int> OnLevelChanged;
        public event Action<CurrencyData> OnCurrencyChanged;

        private void Awake()
        {
            Instance = this;
            State = VillageState.Idle;
        }

        private void Start()
        {
            InitSubManagers();
            RefreshUI();
        }

        public void OpenPanel(GameObject panel) { /* 互斥打开 UI */ }
        public void CloseAllPanels() { /* 关闭所有面板 */ }
        public void EnterBattle() { /* 场景切换到 BattleScene */ }
    }

    public enum VillageState
    {
        Idle,
        Building,
        Decorating,
        InPanel
    }
}
```

### 3.3 场景 Hierarchy 结构

```
VillageScene (场景根)
│
├─ Environment (环境)
│   ├── Main Camera (正交 / 透视)
│   ├── Lighting / Sky / Background
│   └── Ground
│
├─── VillageManagers (村庄管理器)
│   ├── VillageManager.cs
│   ├── BuildingManager.cs
│   ├── DecorationManager.cs
│   └── WalkerManager.cs
│
├─── Buildings (建筑实体)
│   ├── 战斗中心 (Building_Combat)
│   │   └── Sprite / Collider / Building组件
│   ├── 食堂 (Building_Canteen)
│   ├── 渔场 (Building_Fishing)
│   ├── 研究所 (Building_Research)
│   ├── 商店 (Building_Shop)
│   └── 村中心 (Building_Center)
│
├─── Decorations (装饰物)
│   ├── 前层装饰组
│   ├── 中层装饰组
│   └── 背景层装饰组
│
├─── Walkers (漫步豆丁)
│   ├── 豆丁_001
│   └── 豆丁_002 ...
│
├─── UI (Canvas, Screen Space - Camera)
│   ├── ResourceBar (顶部资源栏)
│   ├── Toolbar (底部工具栏)
│   ├── BuildingPanels (各建筑面板)
│   ├── ChatBubbles (对话气泡)
│   └── Popups (弹窗)
│
└─── DataPersistence (数据持久化可选)
    └── VillageSaveLoader.cs
```

---

## 四、建筑系统

### 4.1 BuildingDataSO — 建筑数据配置

```csharp
[CreateAssetMenu(fileName = "NewBuildingData", menuName = "砰砰军团/建筑数据")]
public class BuildingDataSO : ScriptableObject
{
    public string buildingName;              // 建筑名称
    public BuildingType buildingType;        // 建筑类型
    public Sprite icon;                      // UI 图标
    public Sprite[] levelSprites;            // 各级别外观 (index 0 = Lv1)
    public string[] unlockDescriptions;      // 各级解锁描述
    public int[] upgradeCosts;               // 各级升级所需金币
    public int[] upgradeLevelRequirements;   // 各级所需的村庄等级
    public Vector2 worldPosition;            // 在村庄中的位置
}

public enum BuildingType
{
    CombatCenter,       // 战斗中心
    Canteen,            // 食堂
    Fishing,            // 渔场
    ResearchLab,        // 研究所
    Shop,               // 商店
    VillageCenter,      // 村中心
    DecorationShop      // 装饰商店
}
```

### 4.2 VillageBuilding — 建筑实体组件

```csharp
public class VillageBuilding : MonoBehaviour
{
    [SerializeField] private BuildingDataSO _data;
    [SerializeField] private int _currentLevel = 1;

    public BuildingType Type => _data.buildingType;
    public int CurrentLevel => _currentLevel;
    public int MaxLevel => _data.levelSprites.Length;

    public event Action<VillageBuilding> OnClicked;
    public event Action<VillageBuilding, int> OnLevelChanged;

    private void OnMouseDown()
    {
        OnClicked?.Invoke(this);
    }

    public bool CanUpgrade(int villageLevel, int gold)
    {
        int idx = _currentLevel - 1;
        if (idx >= _data.upgradeCosts.Length - 1) return false;
        return villageLevel >= _data.upgradeLevelRequirements[idx + 1]
            && gold >= _data.upgradeCosts[idx + 1];
    }

    public void Upgrade()
    {
        if (_currentLevel >= MaxLevel) return;
        _currentLevel++;
        UpdateVisual();
        OnLevelChanged?.Invoke(this, _currentLevel);
    }

    private void UpdateVisual()
    {
        // 切换 sprite 到对应等级
    }
}
```

### 4.3 BuildingManager

```csharp
public class BuildingManager : MonoBehaviour
{
    private List<VillageBuilding> _buildings = new List<VillageBuilding>();

    private void Start()
    {
        // 收集场景中所有 VillageBuilding，建立索引
    }

    public VillageBuilding GetBuilding(BuildingType type)
    {
        return _buildings.Find(b => b.Type == type);
    }

    public bool TryUpgrade(BuildingType type)
    {
        // 检查条件 → 扣钱 → 升级
    }
}
```

---

## 五、装饰系统

### 5.1 装饰层级

```
村庄画面分层（从里到外）：
  ┌──────────────────────────────────────────────┐
  │  背景层 (Background)  ← 远山、天空、云朵      │
  ├──────────────────────────────────────────────┤
  │  中层 (Midground)     ← 建筑主体、树木、道路   │
  ├──────────────────────────────────────────────┤
  │  前层 (Foreground)    ← 花草、栅栏、装饰小品   │
  └──────────────────────────────────────────────┘
  ↑ 玩家视角（摄像机）
```

### 5.2 DecorationDataSO

```csharp
[CreateAssetMenu(fileName = "NewDecorationData", menuName = "砰砰军团/装饰数据")]
public class DecorationDataSO : ScriptableObject
{
    public string itemName;
    public DecorationLayer layer;       // Foreground / Midground / Background
    public Sprite sprite;
    public bool isInteractive;          // 是否可交互（蹦床/椅子/大炮）
    public int price;                   // 购买价格
    public string requiredBuilding;     // 需要哪个建筑解锁
}
```

### 5.3 DecorationManager

```csharp
public class DecorationManager : MonoBehaviour
{
    [SerializeField] private Transform _foregroundRoot;
    [SerializeField] private Transform _midgroundRoot;
    [SerializeField] private Transform _backgroundRoot;

    private List<VillageDecoration> _placedItems = new List<VillageDecoration>();

    public void EnterEditMode() { /* 显示编辑 UI */ }
    public void ExitEditMode() { /* 隐藏编辑 UI */ }

    public bool TryPlaceItem(DecorationDataSO data, Vector2 position)
    {
        // 检查金币 → 实例化 → 加入列表
    }

    public void RemoveItem(VillageDecoration item)
    {
        // 移除实例
    }
}
```

---

## 六、豆丁漫步系统

### 6.1 VillageWalker

```csharp
public class VillageWalker : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private float _walkSpeed = 0.5f;
    [SerializeField] private float _idleDuration = 2f;

    private Vector2 _targetPosition;
    private bool _isMoving;

    private void Start()
    {
        PickNewTarget();
    }

    private void Update()
    {
        if (_isMoving)
        {
            // 向目标位置移动
            // 到达后播放 Idle 动画，等待后选新目标
        }
    }

    public void InteractWith(DecorationDataSO decoration)
    {
        // 根据装饰类型播放对应动画
        // 蹦床 → 弹跳动画；椅子 → 坐下；大炮 → 发射
    }

    private void PickNewTarget()
    {
        // 在村庄范围内随机选点
    }

    private void OnMouseDown()
    {
        // 点击豆丁时展示对话气泡
    }
}
```

### 6.2 WalkerManager

```csharp
public class WalkerManager : MonoBehaviour
{
    [SerializeField] private GameObject _walkerPrefab;  // 豆丁预制体
    [SerializeField] private int _maxWalkers = 5;

    private List<VillageWalker> _activeWalkers = new List<VillageWalker>();

    private void Start()
    {
        SpawnInitialWalkers();
    }

    public void SpawnWalker(Sprite beanSprite)
    {
        if (_activeWalkers.Count >= _maxWalkers) return;
        // 实例化并初始化
    }

    public void RemoveWalker(VillageWalker walker)
    {
        // 移出村庄（拖回战斗中心）
    }
}
```

---

## 七、UI 层设计

### 7.1 Canvas 层级

```
Canvas (Screen Space - Camera)
│
├── ResourceBar (top layer)          ← 资源栏：金币、宝石、食玉、科技点
│   └── 文本 + 图标 Image
│
├── MainView (middle layer)          ← 主内容层
│   ├── BuildingPanel_Combat         ← 战斗中心面板（匹配/冒险/组卡）
│   ├── BuildingPanel_Canteen        ← 食堂面板
│   ├── BuildingPanel_Fishing        ← 渔场面板
│   ├── BuildingPanel_Research       ← 研究所面板（科技树）
│   ├── BuildingPanel_Shop           ← 商店面板
│   └── BuildPanel_Upgrade           ← 建筑升级确认弹窗
│
├── Toolbar (bottom layer)           ← 底部固定工具栏
│   ├── Btn_Combat    [战斗]
│   ├── Btn_Deck      [卡牌]
│   ├── Btn_Decorate  [装饰]
│   ├── Btn_Shop      [商店]
│   └── Btn_Mail      [邮件]
│
├── ChatBubbleLayer (overlay)        ← 豆丁对话气泡（叠加在最上层）
│
└── PopupLayer (topmost)             ← 全局弹窗（确认框/提示/奖励）
```

### 7.2 UI 组件约定

| 组件 | 类名 | 继承/实现 | 说明 |
|------|------|-----------|------|
| 资源栏 | `ResourceBarUI` | `MonoBehaviour` | 顶部固定，实时刷新货币 |
| 工具栏 | `VillageToolbarUI` | `MonoBehaviour` | 底部固定，按钮 + 事件 |
| 建筑面板基类 | `BuildingPanelUI` | `MonoBehaviour` | 抽象基类，提供打开/关闭动画 |
| 战斗中心面板 | `CombatPanelUI` | `BuildingPanelUI` | 匹配/冒险/组卡入口 |
| 食堂面板 | `CanteenPanelUI` | `BuildingPanelUI` | 食堂玩法入口（预留） |
| 渔场面板 | `FishingPanelUI` | `BuildingPanelUI` | 渔场玩法入口（预留） |
| 研究所面板 | `ResearchPanelUI` | `BuildingPanelUI` | 科技树显示 |
| 商店面板 | `ShopPanelUI` | `BuildingPanelUI` | 装饰/道具购买 |
| 升级弹窗 | `UpgradePopupUI` | `MonoBehaviour` | 建筑升级确认 |
| 对话气泡 | `ChatBubbleUI` | `MonoBehaviour` | 豆丁头顶随机文本 |

### 7.3 交互流程示例

```
用户点击"战斗中心"建筑：
  1. VillageBuilding.OnMouseDown()
  2. → 触发 OnClicked 事件
  3. → VillageManager 收到事件
  4. → CloseAllPanels()
  5. → Instantiate(CombatPanelUI) / SetActive
  6. → 面板打开，显示匹配/冒险/组卡选项

用户点击"进入战斗"：
  1. CombatPanelUI 触发 EnterBattle 事件
  2. → VillageManager.EnterBattle()
  3. → SceneManager.LoadScene("SampleScene")
```

---

## 八、数据持久化

### 8.1 VillageData — 存档数据结构

```csharp
[System.Serializable]
public class VillageSaveData
{
    public int villageLevel;
    public int gold;
    public int gems;
    public int foodJade;
    public int techPoints;

    public List<BuildingSaveData> buildings;
    public List<DecorationSaveData> decorations;
    public List<string> ownedBeanIds;  // 拥有的豆丁
}

[System.Serializable]
public class BuildingSaveData
{
    public BuildingType type;
    public int level;
}

[System.Serializable]
public class DecorationSaveData
{
    public string itemId;
    public DecorationLayer layer;
    public float posX, posY;
}
```

### 8.2 VillageDataManager

```csharp
public class VillageDataManager : MonoBehaviour
{
    private const string SAVE_KEY = "VillageSaveData";

    public VillageSaveData CurrentData { get; private set; }

    private void Awake()
    {
        LoadData();
    }

    public void SaveData()
    {
        string json = JsonUtility.ToJson(CurrentData);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    public void LoadData()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            CurrentData = JsonUtility.FromJson<VillageSaveData>(json);
        }
        else
        {
            CurrentData = CreateDefaultData();
        }
    }

    private VillageSaveData CreateDefaultData() { /* 初始数据 */ }
}
```

---

## 九、文件结构

新增文件清单：

```
Assets/Scripts/
├── Village/                              # 村庄模块（新增）
│   ├── VillageManager.cs                 ← 村庄主控
│   ├── VillageDataManager.cs             ← 数据持久化
│   ├── VillageSaveData.cs                ← 存档数据结构
│   │
│   ├── Buildings/
│   │   ├── BuildingDataSO.cs             ← 建筑数据配置
│   │   ├── VillageBuilding.cs            ← 建筑实体组件
│   │   └── BuildingManager.cs            ← 建筑管理器
│   │
│   ├── Decorations/
│   │   ├── DecorationDataSO.cs           ← 装饰数据配置
│   │   ├── VillageDecoration.cs          ← 装饰实体组件
│   │   └── DecorationManager.cs          ← 装饰管理器
│   │
│   ├── Walkers/
│   │   ├── VillageWalker.cs              ← 豆丁漫步组件
│   │   └── WalkerManager.cs              ← 豆丁管理器
│   │
│   └── UI/
│       ├── ResourceBarUI.cs              ← 顶部资源栏
│       ├── VillageToolbarUI.cs           ← 底部工具栏
│       ├── BuildingPanelUI.cs            ← 建筑面板基类
│       ├── CombatPanelUI.cs              ← 战斗中心面板
│       ├── CanteenPanelUI.cs             ← 食堂面板（预留）
│       ├── FishingPanelUI.cs             ← 渔场面板（预留）
│       ├── ResearchPanelUI.cs            ← 研究所面板
│       ├── ShopPanelUI.cs                ← 商店面板
│       ├── UpgradePopupUI.cs             ← 升级弹窗
│       └── ChatBubbleUI.cs               ← 豆丁对话气泡
│
├── Core/                                 # 已有，不修改
│   ├── GameManager.cs
│   ├── GridManager.cs
│   └── MetronomeUI.cs
├── Data/                                 # 已有
│   ├── UnitData.cs
│   ├── GridPosition.cs
│   └── GameEnums.cs
└── Units/                                # 已有
    ├── UnitBase.cs
    ├── Commander.cs
    ├── EnemyBase.cs
    ├── Tower.cs
    ├── UnitVisual.cs
    └── UnitHPBar.cs
```

新增资源文件：

```
Assets/
├── Prefabs/
│   └── Village/                          ← 村庄预制体
│       ├── Building_Combat.prefab
│       ├── Building_Canteen.prefab
│       ├── Building_Fishing.prefab
│       ├── Building_Research.prefab
│       ├── Building_Shop.prefab
│       ├── Building_Center.prefab
│       ├── VillageWalker.prefab          ← 豆丁漫步预制体
│       └── UI/
│           ├── ResourceBar.prefab
│           ├── Toolbar.prefab
│           ├── Panel_Combat.prefab
│           ├── Panel_Upgrade.prefab
│           └── ChatBubble.prefab
│
├── Objects/
│   └── Village/                          ← 村庄数据资产
│       ├── BuildingData_Combat.asset
│       ├── BuildingData_Canteen.asset
│       ├── BuildingData_Fishing.asset
│       ├── BuildingData_Research.asset
│       ├── BuildingData_Shop.asset
│       └── BuildingData_Center.asset
│
└── Scenes/
    └── VillageScene.unity                ← 村庄场景（新增）
```

---

## 十、开发路线（实现顺序）

| 阶段 | 内容 | 估计 |
|------|------|------|
| **Phase 1: 框架搭建** | VillageManager + VillageDataManager + 存档系统 + 场景创建 | 1-2 天 |
| **Phase 2: 建筑系统** | BuildingDataSO + VillageBuilding + BuildingManager + UI 面板框架 | 1-2 天 |
| **Phase 3: UI 层** | ResourceBarUI + ToolbarUI + 建筑面板基类 + 场景切换 | 1-2 天 |
| **Phase 4: 装饰系统** | DecorationDataSO + 编辑模式 + 三层渲染 + 交互装饰 | 2-3 天 |
| **Phase 5: 豆丁漫步** | VillageWalker + WalkerManager + 对话气泡 | 1-2 天 |
| **Phase 6: 功能面板** | CombatPanelUI + ResearchPanelUI + ShopPanelUI + UpgradePopup | 2-3 天 |

> **当前阶段**：Phase 1 — 框架搭建。
> 后续渔场和食堂的玩法逻辑不在本文档范围内，仅在 UI 中预留面板入口。

---

## 十一、编码规范（补充）

沿用现有项目规范，补充村庄模块专用约定：

| 规则 | 说明 |
|------|------|
| 命名空间 | 保持 `PPCorps`，不拆分子命名空间 |
| 村庄类前缀 | 见上面文件清单，不加额外前缀 |
| UI 类后缀 | 统一 `...UI` 后缀（如 `ResourceBarUI`） |
| 数据 SO 后缀 | 统一 `...DataSO` 后缀 |
| 事件命名 | `On` + 事件名（如 `OnBuildingClicked`） |
| UI 交互 | 全部使用 UGUI（`Button` / `Text` / `Image`），不新增 IMGUI |
| 场景切换 | 使用 `SceneManager.LoadScene`，场景名定义为常量 |
| 数据持久化 | 使用 `JsonUtility` + `PlayerPrefs`，不引入第三方库 |
| 资源路径 | UI 预制体放在 `Resources/` 或使用直接引用 |

---

*设计版本：v1.0 — 2026-05-21*
