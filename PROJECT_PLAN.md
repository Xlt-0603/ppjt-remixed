# 方案：拖拽放置系统（Drag & Drop Deployment）

## 1. 核心流程

```
部署阶段 (GameState.Deploy)
  │
  ├─ 卡组区：底部显示可部署的单位卡牌
  │     └─ 鼠标在卡牌上按下 → 开始拖拽
  │
  ├─ 拖拽中：
  │     ├─ 幽灵单位跟随鼠标（吸附到最近格子）
  │     ├─ 幽灵绿色 = 可放置，红色 = 不可放置
  │     └─ 松开鼠标 → 如果有效则放置，否则取消
  │
  └─ 放置单位：
        ├─ 扣除费用
        ├─ 实例化预制体
        └─ 进入冷却
```

## 2. 模块设计

### 2.1 DeploySystem（部署管理器）
**职责：** 放置单位的最终执行者，费用管理，冷却管理。

| 字段/方法 | 说明 |
|-----------|------|
| `_maxEnergy` | 最大费用（默认 10） |
| `_energyPerBeat` | 每拍回复费用（默认 1） |
| `_energy` | 当前费用 |
| `_playerSpawnY` | 玩家单位出生 Y 坐标 |
| `_placeableColMin` | 可放置列范围（左边界，默认 0） |
| `_placeableColMax` | 可放置列范围（右边界，默认 11） |
| `CanPlace(UnitData, GridPosition)` | 验证：费用够/格子有效/格子未占用 |
| `PlaceUnit(UnitData, GridPosition)` | 执行放置：扣费 → 实例化 → 设数据 → 注册 |

### 2.2 DragHandler（拖拽处理器）
**职责：** 处理鼠标交互，管理幽灵对象，判定放置位置。

| 字段/方法 | 说明 |
|-----------|------|
| `_ghost` | 幽灵单位（SpriteRenderer 临时对象） |
| `_currentData` | 当前拖拽的单位数据 |
| `_isDragging` | 是否正在拖拽 |
| `_validTarget` | 当前鼠标位置的格子是否可放置 |
| `StartDrag(UnitData)` | 开始拖拽：创建幽灵，设定 sprite |
| `UpdateDrag()` | 每帧执行：鼠标→世界→吸附到格子→更新幽灵位置/颜色 |
| `EndDrag(bool cancelled)` | 结束拖拽：放置或取消，销毁幽灵 |

**幽灵单位逻辑：**
- 创建：`new GameObject("DeployGhost")` + `SpriteRenderer`
- sprite 来自 `UnitData.prefab` 上 `SpriteRenderer` 的 sprite，若无则用 `UnitData.icon`
- 每帧：`Camera.main.ScreenToWorldPoint(Input.mousePosition)` → 取 X → `GridManager.WorldToGrid()` → `GridToWorldX()` → 修正 Y = `_playerSpawnY`
- 颜色：有效=绿色 (0,1,0,0.6)，无效=红色 (1,0,0,0.6)
- 结束：`Destroy(_ghost)`

**拖拽操作：**
- 有效放置：左键松开 → `DeploySystem.PlaceUnit()`
- 取消：右键 / Escape → `EndDrag(cancelled=true)`

### 2.3 DeployPanel（卡牌面板）
**职责：** OnGUI 绘制底部卡组区，响应点击开始拖拽。

**布局：**
- 屏幕底部区域（如 `y = Screen.height - 120` 到 `Screen.height`）
- 横向排列的单位卡牌
- 每张卡牌显示：图标 + 名称 + 费用数字
- 灰色覆盖：费用不够或冷却中

**每张卡牌：**
```
┌──────────┐
│  图标     │
│  名称     │
│  费用 3   │
└──────────┘
```

**交互：**
- `EventType.MouseDown` 在卡牌区域 → 检查费用/冷却 → `DragHandler.StartDrag()`
- 卡牌区域 `GUI.DrawTexture` + `GUI.Label`

## 3. 数据准备

需要为每种玩家单位创建 `UnitData` ScriptableObject 资产，并配置好：
- `prefab` → 实例化用的预制体
- `icon` → 卡牌图标 + 幽灵 sprite（若无 prefab sprite 则 fallback）
- `deployCost` → 部署费用
- 其他战斗属性

## 4. 添加到场景

需要一个 `DeployRoot` GameObject，挂载：
- `DeploySystem`（单例，管理费用和放置）
- `DragHandler`（处理拖拽幽灵）
- `DeployPanel`（绘制卡牌 UI）
- 在 `DeployPanel` 中配置一个 `UnitData[]` 数组作为玩家卡组

## 5. 状态集成

`GameManager` 已经定义好了 `GameState.Deploy`：
- 开局：`GameState.Deploy`，卡牌面板可见
- 点击「开战」：`GameState.Battle`，卡牌面板隐藏，部署锁定
- 战斗中暂停：`GameState` 不变（保持 Battle），`IsPaused=true`，不触发部署

## 6. 可选的增强功能（第一期不实现）

| 功能 | 说明 |
|------|------|
| 攻击范围指示器 | 拖拽时显示该单位的攻击范围 |
| 单位信息弹出 | 鼠标悬停卡牌时显示详细属性 |
| 部署动画 | 放置时播放一个简单的生成特效 |
| 冷却指示 | 卡牌上的冷却倒计时指示 |
| 拖拽预览线 | 从卡牌到鼠标位置画一条连接线 |

## 6.1 第一期简化：费用数字显示

在第一期实现中，**不涉及能量条/电池等可视化 UI**，费用以纯数字形式显示在 `MetronomeUI` 的节拍显示区域的右方：

```
小节 3 | BPM: 120    费用 7/10
```

- `DeploySystem` 在 `Awake` 时将 `Energy` 初始化为 `_maxEnergy`（默认 10）
- 每拍回复 `_energyPerBeat` 点费用（仅在 `GameState.Battle` 时）
- 当前费用 / 最大费用显示在 `MetronomeUI` 的 `DrawBeatVisualizer` 和 `DrawCollapsedBar` 中

## 7. 文件清单（新增/修改）

| 文件 | 类型 | 说明 |
|------|------|------|
| `Assets/scripts/Deploy/DeploySystem.cs` | 新增 | 部署管理器 |
| `Assets/scripts/Deploy/DragHandler.cs` | 新增 | 拖拽处理器 |
| `Assets/scripts/Deploy/DeployPanel.cs` | 新增 | 卡牌面板（OnGUI） |
| 玩家单位 `UnitData` 资产 | 新增 | 每种可部署单位一个资产文件 |
