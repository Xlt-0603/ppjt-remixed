# ppjt-remixed — 砰砰军团

**Unity 2022.3.62f3c1** + **URP 2D** 的节拍驱动自走棋/塔防策略游戏。

## 核心机制：节拍驱动

游戏以**节拍系统**而非传统帧循环驱动：
```
小节 (Bar) = 1 秒 (BPM=60)
  ├─ 拍 1 ~ 拍 8
```

- 每拍广播 `OnBeat`，所有单位同步执行：索敌 → 攻击/移动
- 单位每小节的 **Beat 1** 移动一次，其余拍攻击或待机
- 攻击间隔以"拍"为单位

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
│   │   ├── GridManager.cs        # 网格管理器 (24列×1行, cellSize=1.3333)
│   │   └── MetronomeUI.cs        # 节拍器 HUD（调速、步进、暂停、单位列表）
│   ├── Data/
│   │   ├── UnitData.cs           # ScriptableObject 单位数据配置
│   │   ├── GridPosition.cs       # 格子坐标结构 (col)
│   │   └── GameEnums.cs          # 枚举 (GameState, UnitType, UnitAction)
│   ├── Units/
│   │   ├── UnitBase.cs           # 单位基类（格子化移动+索敌+攻击+冷却+死亡）
│   │   ├── Commander.cs          # 玩家指挥官（血量归零 → Lose）
│   │   ├── EnemyBase.cs          # 敌人基类（继承 UnitBase，方向左）
│   │   ├── Tower.cs              # 防御塔（不移动，只攻击）
│   │   ├── UnitVisual.cs         # 视觉层（一小节内 Lerp 平滑移动 + flipX）
│   │   └── UnitHPBar.cs          # 单位头顶血条
│   ├── FRAMEWORK_DESIGN.md       # 框架设计文档
│   └── PROJECT_PLAN.md           # 开发计划
├── Objects/                      # 精灵、ScriptableObject 数据资产
├── Prefabs/                      # 预制体（剑士、敌人、防御塔、格子、sky）
└── Scenes/                       # 场景文件、Tilemap 瓦片资源（含 bckgrd2）
```

## 网格系统（新增）

```
24 列 × 1 行，水平排列

列 0  1  2  3  4  5 ... 18 19 20 21 22 23
    |  | 塔 | 塔 |  |  |     |  | 塔 | 塔 |  |

特殊格 (col 2,3 / 20,21): 可同时容纳 1 塔 + 1 非塔单位
普通格: 严格 1 个单位
```

## 移动系统（重构）

| 方面 | 说明 |
|------|------|
| **时机** | 每小节 Beat 1 触发一次 |
| **距离** | `moveSpeed` 格（整数），如 `moveSpeed=2` 一次走 2 格 |
| **耗时** | 移动占用整小节（8 拍），视觉上平滑滑动 |
| **阻挡** | 目标格被占用 → 停在原地，若在攻击范围内则攻击阻挡者 |
| **移动态** | `_isMoving` 期间不可被索敌，防止移动中被拦截攻击 |
| **视觉** | `UnitVisual` 在 8 拍内 `Lerp(MoveFrom, MoveTo)` |

### 单拍执行流程

```
OnBeat(bar, beat):
  ├─ if _isMoving → 检查移动是否完成（Time.time >= 整小节），完成则 snap 到位
  ├─ FindNearestEnemy() → 跳过 _isMoving 的单位
  ├─ 有目标 & 在攻击范围 → TryAttack()
  ├─ beat==1 → TryMove()
  │   ├─ 计算目标格子 = _gridPos + dir × moveSpeed
  │   ├─ 可进入 → 更新 _gridPos，标记 _isMoving
  │   └─ 不可进入 → 若目标格有阻挡者则攻击，否则 Idle
  └─ 其他 → Idle
```

## 架构要点

- **全格子化**：所有位置用 `GridPosition(col)`，`InAttackRange` / `FindNearestEnemy` 按列距离计算
- **逻辑/视觉分离**：`UnitBase` 管理 `_gridPos`，`UnitVisual` 管理平滑移动
- **节拍引擎**：基于 `Time.deltaTime` 累加器，不受帧率影响

## 单位类型

| 类型 | 脚本 | 移动 | 特殊 |
|------|------|------|------|
| 剑士（玩家） | `UnitBase` | Beat 1 右移 | 基础单位 |
| 剑士（敌人） | `EnemyBase` | Beat 1 左移 | 继承 UnitBase |
| 防御塔 | `Tower` | 不移动 | 驻守特殊格 |

## 开发进度

| 阶段 | 状态 |
|------|------|
| 阶段 1：基础框架 | ✅ 完成 |
| 阶段 2：部署系统 | ⏸ 搁置 |
| 阶段 3：格子系统 | ✅ 完成 |
| 阶段 4：战斗系统 | 🔄 进行中 |
| 阶段 5：敌人系统 | ⬜ 未开始 |

你好