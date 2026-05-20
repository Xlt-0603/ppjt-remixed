# ppjt-remixed — 砰砰军团

**Unity 2022.3.62f3c1** + **URP 2D** 的节拍驱动自走棋/塔防策略游戏。

## 核心机制：节拍驱动

游戏以**节拍系统**而非传统帧循环驱动：
```
小节 (Bar) = 1 秒 (BPM=60)
  ├─ 拍 1 ~ 拍 8
```

- 每拍广播 `OnBeat`，所有单位同步执行：索敌 → 攻击/移动
- 单位**每小节的拍 1 移动一次**，其余拍攻击或待机
- 攻击间隔以"拍"为单位（如每 4 拍攻击一次）

## 项目结构

```
Assets/
├── anim/                         # 动画资源
│   ├── 剑士2.controller          # Animator Controller
│   ├── 剑士捅.anim               # 攻击动画 Clip
│   └── 剑士攻击（捅）精灵图.png   # 攻击序列帧精灵图
├── scripts/
│   ├── Core/
│   │   ├── GameManager.cs        # 全局节拍引擎 + 游戏状态机
│   │   └── MetronomeUI.cs        # 节拍器 HUD（调速、步进、暂停、单位列表）
│   ├── Data/
│   │   ├── UnitData.cs           # ScriptableObject 单位数据配置
│   │   └── GameEnums.cs          # 枚举 (GameState, UnitType, UnitAction)
│   ├── Units/
│   │   ├── UnitBase.cs           # 单位基类（索敌、攻击、移动、受伤、死亡）
│   │   ├── Commander.cs          # 玩家指挥官（血量归零 → Lose）
│   │   ├── EnemyBase.cs          # 敌人基类（自动向左移动，朝向朝左）
│   │   ├── Tower.cs              # 防御塔（静态单位，只攻击不移动）
│   │   ├── UnitVisual.cs         # 视觉平滑组件（Lerp + sprite flipX）
│   │   └── UnitHPBar.cs          # 单位头顶血条
│   ├── FRAMEWORK_DESIGN.md       # 框架设计文档
│   └── PROJECT_PLAN.md           # 开发计划
├── Objects/                      # 精灵、ScriptableObject 数据资产
├── Prefabs/                      # 预制体（剑士、敌人、防御塔、格子、sky）
└── Scenes/                       # 场景文件、Tilemap 瓦片资源（含 bckgrd2）
```

## 单位类型

| 类型 | 脚本 | 移动 | 攻击 |
|------|------|------|------|
| 剑士（玩家） | `UnitBase` | 拍 1 向右移 | 近战，攻击间隔看数据配置 |
| 剑士（敌人） | `EnemyBase` | 拍 1 向左移（× tileSize） | 近战，朝向自动朝左 |
| 防御塔 | `Tower` | 不动 | 近战 |

## 架构要点

- **逻辑/视觉分离**：`LogicalPosition` 节拍驱动，`UnitVisual` 每帧 Lerp 平滑插值，Sprite 根据 `FacingDirection` 自动 flipX
- **动画框架**：`Animator` 通过 `Action(Int)` 参数控制（0=Idle, 1=Moving, 2=Attacking, 3=Dead），由 `SetAction()` + `SyncAnimator()` 驱动
- **节拍引擎**：基于 `Time.deltaTime` 累加器，不受帧率影响，掉帧时 `while` 补拍

## 开发进度

| 阶段 | 状态 |
|------|------|
| 阶段 1：基础框架（GameManager + UnitBase + Commander + MetronomeUI + UnitVisual） | ✅ 完成 |
| 阶段 2：部署系统（尝试后回滚） | ⏸ 搁置 |
| 阶段 3：战斗系统 | ⬜ 未开始 |
| 阶段 4：敌人系统（波次管理） | ⬜ 未开始 |
| 阶段 5：对象池 | ⬜ 未开始 |
| 阶段 6：UI | ⬜ 未开始 |
