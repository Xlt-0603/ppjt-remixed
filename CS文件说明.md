# 砰砰军团 — CS 文件说明

> 所有脚本位于 `Project_Legions/Assets/scripts/` 下，命名空间 `PPCorps`。

---

## 目录结构

```
Assets/Scripts/
├── Core/
│   ├── GameManager.cs      ← 全局控制器，节拍引擎，状态机
│   └── MetronomeUI.cs      ← 节拍器 HUD 面板（调试用）
├── Data/
│   ├── UnitData.cs          ← 单位数据配置（ScriptableObject）
│   └── GameEnums.cs         ← 枚举定义
└── Units/
    ├── UnitBase.cs          ← 单位基类（核心逻辑）
    ├── UnitVisual.cs        ← 单位视觉层（平滑移动 + 朝向）
    ├── UnitHPBar.cs         ← 单位血条显示
    ├── EnemyBase.cs         ← 敌人单位（自动向左移动）
    ├── Tower.cs             ← 防御塔（不移动，远程攻击）
    └── Commander.cs         ← 指挥官（基地血条）
```

---

## Core — 核心

### GameManager.cs
**全局游戏管理器，单例，挂载在场景 Manager 物体上**

| 公开成员 | 类型 | 说明 |
|---------|------|------|
| `Instance` | `static` | 单例访问 |
| `Bar` | `int` | 当前小节数（从 1 开始） |
| `Beat` | `int` | 当前拍数（1~8） |
| `BPM` | `float` | 每分钟小节数，默认 60（1 秒 1 小节） |
| `State` | `GameState` | 当前游戏状态：Deploy / Battle / Win / Lose |
| `IsPaused` | `bool` | 节拍器是否暂停 |
| `OnBeat` | `event` | 每拍广播 `(bar, beat)` |
| `OnStateChanged` | `event` | 状态切换时广播 |

**核心流程**：
```
Update() → 计时 → AdvanceOneBeat()
  → Beat++ → OnBeat 事件 → 遍历所有单位 OnBeat()
  → CheckGameOver()
```

**公开方法**：

| 方法 | 功能 |
|------|------|
| `RegisterUnit(UnitBase)` | 注册单位到全局列表 |
| `UnregisterUnit(UnitBase)` | 从列表移除 |
| `GetAllUnits()` | 获取所有存活单位 |
| `GetCommander()` | 获取指挥官引用 |
| `StartBattle()` | Deploy → Battle |
| `StepBeat()` | 暂停状态下步进一拍 |
| `TogglePause()` | 暂停/继续 |
| `SetBPM(float)` | 调整速度 |

> 单位在 `Start()` 中自动注册，在 `OnDestroy()` 中自动注销，不需要手动管理。

---

### MetronomeUI.cs
**节拍器调试 UI，挂载在 Manager 物体上**

- **节拍可视化**：屏幕顶部 8 个方块，当前拍高亮蓝色，已过拍绿色，未到拍灰色
- **调试面板**（可开关 `_showDebugPanel`）：
  - BPM 滑块 + 加减按钮（-10 / -1 / +1 / +10）
  - 游戏状态显示
  - 战斗控制按钮：开战 / 暂停 / 步进
  - 单位列表：显示每个单位的 HP 条、当前行为、目标

> 使用 `OnGUI` / `GUILayout` 实现，纯调试用途，不影响正式 UI。

---

## Data — 数据层

### UnitData.cs
**单位属性配置，ScriptableObject**

在 Project 中通过 `右键 → Create → 砰砰军团 → 单位数据` 创建。

| 字段 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `unitName` | `string` | — | 单位名称 |
| `prefab` | `GameObject` | — | 预制体引用 |
| `icon` | `Sprite` | — | UI 图标 |
| `unitType` | `UnitType` | — | Melee / Ranged |
| `maxHP` | `int` | 10 | 最大生命值 |
| `attackPower` | `int` | 2 | 攻击力 |
| `attackRange` | `float` | 1 | 攻击距离（Unity 单位） |
| `attackIntervalInBeats` | `int` | 4 | 攻击间隔（拍数） |
| `moveSpeed` | `float` | 0.15 | 每拍移动距离 |
| `deployCost` | `int` | 3 | 部署所需费用 |

> 这是纯数据资产，不挂载在游戏物体上，通过 Inspector 拖拽赋值给 UnitBase。

---

### GameEnums.cs
**枚举定义，不挂载，纯代码引用**

| 枚举 | 值 | 说明 |
|------|----|------|
| `GameState` | `Deploy`, `Battle`, `Win`, `Lose` | 游戏状态 |
| `UnitType` | `Melee`, `Ranged` | 单位类型 |
| `UnitAction` | `Idle` (0), `Moving` (1), `Attacking` (2), `Dead` (3) | 单位行为状态，值对应 Animator 的 Action 参数 |

---

## Units — 单位层

### UnitBase.cs
**所有单位的基类**

**Inspector 可配置字段**：

| 字段 | 说明 |
|------|------|
| `data` | `UnitData` 引用（必填） |
| `isEnemy` | 是否为敌方单位 |
| `defaultMoveDirection` | 默认移动方向 |
| `_animator` | `Animator` 组件引用（可拖拽，为空则自动获取） |

**公开属性**：

| 属性 | 类型 | 说明 |
|------|------|------|
| `IsEnemy` | `bool` | 是否是敌人 |
| `IsDead` | `bool` | 是否死亡 |
| `CurrentHP` | `int` | 当前血量 |
| `MaxHP` | `int` | 最大血量 |
| `CurrentAction` | `UnitAction` | 当前行为 |
| `CurrentTarget` | `UnitBase` | 当前攻击目标 |
| `LogicalPosition` | `Vector3` | 逻辑位置（每拍跳变） |
| `FacingDirection` | `float` | 面朝方向（-1 左 / 1 右） |

**核心方法**：

| 方法 | 可见性 | 说明 |
|------|--------|------|
| `Start()` | `protected virtual` | 初始化血量、位置、方向；自动注册 + 挂血条 |
| `OnBeat(bar, beat)` | `public virtual` | **每拍调用一次**：索敌 → 攻击/移动/待机 |
| `TryAttack(target)` | `protected virtual` | 攻击目标（含冷却逻辑） |
| `TakeDamage(damage)` | `public virtual` | 扣血 → 死亡判定 |
| `Die()` | `protected virtual` | 死亡处理：标记死亡 + 反注册 + 销毁 |
| `SetAction(action)` | `protected` | **修改行为 + 广播事件 + 同步 Animator** |
| `FindNearestEnemy()` | `protected` | 找最近的敌方单位 |
| `InAttackRange(target)` | `protected` | 判断目标是否在攻击范围内 |
| `MoveOneStepTowards(pos)` | `protected` | 向目标位置移动一格 |

> **新单位写法**：继承 `UnitBase`，重写 `OnBeat`，用 `SetAction()` 改变行为。

> **关键约定**：必须用 `SetAction()` 而非直接赋值 `_currentAction`，否则动画不会同步。

**动画同步机制**：
```
SetAction(action)
  → _currentAction = action
  → OnActionChanged 事件（供 UnitVisual 等订阅）
  → SyncAnimator()  →  _animator.SetInteger("Action", (int)action)
```

---

### UnitVisual.cs
**单位视觉表现层，挂载在同一预制体上**

| 字段 | 说明 |
|------|------|
| `_spriteRenderer` | SpriteRenderer 引用（必填） |
| `_smoothSpeed` | 平滑移动速度（默认 12，越大越跟手） |

**Update() 每帧做两件事**：
1. **平滑移动**：`transform.position = Lerp(当前位置, LogicalPosition, speed * delta)`
2. **人物朝向**：根据 `FacingDirection` 设置 `SpriteRenderer.flipX`

> 不负责动画，动画由 UnitBase 的 `SyncAnimator()` 直接驱动。

---

### UnitHPBar.cs
**单位头顶血条**

用 `OnGUI` 在单位头顶绘制红底绿条的血量条，`_offset` 控制血条高度偏移。

> 自动跟随 `transform.position`（视觉位置），与平滑移动配合。

---

### EnemyBase.cs
**敌人单位，继承 UnitBase**

- `_moveDirection`：移动方向，默认 `Vector2.left`（向左）
- **与 UnitBase.OnBeat 的区别**：使用 `_moveDirection` 而非 `defaultMoveDirection`
- 其他行为与 UnitBase 一致

---

### Tower.cs
**防御塔，继承 UnitBase**

- **不移动**：`OnBeat` 中只有索敌→攻击逻辑，没有移动代码
- 无目标时直接 `SetAction(UnitAction.Idle)`
- 通过 `UnitData.attackRange` 控制射程（设 > 0.5 即为远程）

---

### Commander.cs
**指挥官（基地），独立类，不继承 UnitBase**

| 成员 | 说明 |
|------|------|
| `maxHP` | 最大生命值（Inspector 配置，默认 20） |
| `currentHP` | 当前生命值 |
| `OnHPChanged` | 事件：血量变化时广播 `(current, max)` |
| `OnDeath` | 事件：死亡时广播 |
| `TakeDamage(int)` | 扣血（不会低于 0） |

> GameManager 通过 `_commander.currentHP <= 0` 判定胜负，`_commander` 引用在 Inspector 中拖拽赋值。
