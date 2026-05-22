# 渔场 MVP 素材清单

> 已有素材打 ✓，需新建打 ■

---

## 一、已有素材

| 文件 | 类型 | 说明 |
|------|------|------|
| `Assets/Objects/Village/渔场.png` | 建筑 Sprite | 1000×600px, PPU=60 |
| `Assets/Objects/Village/BuildingData_Fishing.asset` | SO 配置 | 已由 VillageSceneSetup 生成 |

---

## 二、需新建素材

### 1. 鱼 Sprite（8 张）

路径：`Assets/Objects/Village/Fishing/`

| 文件名 | 鱼名 | 稀有度 | 建议尺寸 |
|--------|------|--------|---------|
| `fish_carp.png` | 溪流鲫鱼 | 普通（蓝） | 64×32px |
| `fish_rockbass.png` | 石斑鱼 | 普通（蓝） | 64×32px |
| `fish_redbrow.png` | 赤眉鱼 | 稀有（紫） | 64×48px |
| `fish_jellyfish.png` | 荧光水母 | 稀有（紫） | 64×48px |
| `fish_fireloach.png` | 火焰泥鳅 | 稀有（紫） | 64×48px |
| `fish_goldcarp.png` | 金鳞鲤 | 传说（金） | 64×64px |
| `fish_shadowcatfish.png` | 暗影鲶 | 传说（金） | 64×64px |
| `fish_leviathan.png` | 利维坦幼体 | 传说（金） | 96×64px |

### 2. UI 图标（9 张）

路径：`Assets/UI/Village/Fishing/`

| 文件名 | 建议尺寸 | 说明 |
|--------|---------|------|
| `icon_fishcoin.png` | 32×32 | 鱼币图标 |
| `icon_rod.png` | 32×32 | 鱼竿图标 |
| `icon_bait_normal.png` | 32×32 | 普通饵 |
| `icon_bait_advanced.png` | 32×32 | 高级饵 |
| `icon_fishbox.png` | 32×32 | 鱼箱图标 |
| `icon_rarity_blue.png` | 16×16 | 普通稀有度标记 |
| `icon_rarity_purple.png` | 16×16 | 稀有稀有度标记 |
| `icon_rarity_gold.png` | 16×16 | 传说稀有度标记 |
| `icon_fish_unknown.png` | 64×64 | 图鉴未解锁占位图 |

### 3. UI 预制体（3 个）

路径：`Assets/Prefabs/Village/UI/`

| 文件 | 内容 |
|------|------|
| `FishingPanel.prefab` | 渔场主面板（装备栏 + 开始钓鱼按钮 + 鱼箱状态 + 图鉴入口） |
| `FishingCollectionPanel.prefab` | 图鉴面板（8 鱼网格，已解锁/未解锁） |
| `FishResultPopup.prefab` | 钓到鱼时的弹出通知（鱼名 + 稀有度 + NEW 标识） |

### 4. 脚本（9 个）

路径：`Assets/Scripts/Village/Fishing/`

| 文件 | 职责 |
|------|------|
| `FishEnums.cs` | `FishRarity` / `FishSize` 枚举 |
| `FishingDataSO.cs` | 鱼数据配置（ScriptableObject） |
| `FishingRodConfigSO.cs` | 鱼竿等级表（等级 → 费用 → 效果） |
| `FishBoxConfigSO.cs` | 鱼箱等级表（等级 → 费用 → 容量） |
| `FishingSaveData.cs` | 存档数据结构（`[System.Serializable]`） |
| `FishingManager.cs` | 渔场主控单例（开始钓鱼/概率抽取/收竿结算） |
| `FishingUI.cs` | 渔场面板逻辑（刷新显示、按钮事件） |
| `FishingCollectionUI.cs` | 图鉴面板逻辑（网格填充、未解锁显示） |
| `FishResultPopup.cs` | 鱼获弹窗逻辑（展示 + 自动关闭） |

### 5. 数据资产（10 个）

路径：`Assets/Objects/Village/Fishing/`

| 文件 | 类型 |
|------|------|
| `FishData_Carp.asset` | 溪流鲫鱼配置 |
| `FishData_RockBass.asset` | 石斑鱼配置 |
| `FishData_RedBrow.asset` | 赤眉鱼配置 |
| `FishData_Jellyfish.asset` | 荧光水母配置 |
| `FishData_FireLoach.asset` | 火焰泥鳅配置 |
| `FishData_GoldCarp.asset` | 金鳞鲤配置 |
| `FishData_ShadowCatfish.asset` | 暗影鲶配置 |
| `FishData_Leviathan.asset` | 利维坦幼体配置 |
| `FishRodConfig.asset` | 鱼竿等级表 |
| `FishBoxConfig.asset` | 鱼箱等级表 |

---

## 三、文件夹结构总览

```
Assets/
├── Objects/Village/
│   ├── Fishing/                     ← 新建
│   │   ├── fish_*.png              (×8, Sprite)
│   │   ├── FishData_*.asset        (×8, SO)
│   │   ├── FishRodConfig.asset     (SO)
│   │   └── FishBoxConfig.asset     (SO)
│   ├── 渔场.png                     ✓已有
│   └── BuildingData_Fishing.asset   ✓已有
│
├── UI/Village/Fishing/              ← 新建
│   ├── icon_fishcoin.png
│   ├── icon_rod.png
│   ├── icon_bait_normal.png
│   ├── icon_bait_advanced.png
│   ├── icon_fishbox.png
│   ├── icon_rarity_blue.png
│   ├── icon_rarity_purple.png
│   ├── icon_rarity_gold.png
│   └── icon_fish_unknown.png
│
├── Prefabs/Village/UI/
│   ├── FishingPanel.prefab          ← 新建
│   ├── FishingCollectionPanel.prefab ← 新建
│   └── FishResultPopup.prefab       ← 新建
│
└── Scripts/Village/Fishing/         ← 新建
    ├── FishEnums.cs
    ├── FishingDataSO.cs
    ├── FishingRodConfigSO.cs
    ├── FishBoxConfigSO.cs
    ├── FishingSaveData.cs
    ├── FishingManager.cs
    ├── FishingUI.cs
    ├── FishingCollectionUI.cs
    └── FishResultPopup.cs
```

---

## 四、汇总

| 类别 | 已有 | 需新建 |
|------|:----:|:------:|
| Sprite（建筑 + 鱼 + UI） | 1 | 17 |
| UI 预制体 | 0 | 3 |
| 脚本 | 0 | 9 |
| 数据 asset | 1 | 10 |
| **合计** | **2** | **39 个文件** |
