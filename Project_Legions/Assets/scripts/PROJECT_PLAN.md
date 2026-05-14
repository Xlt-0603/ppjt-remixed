# 砰砰军团 — 项目开发计划

> 基于 Unity 2022.3 LTS + C# 的 2D 自走棋/塔防策略游戏

---

## 一、项目现状

| 项目 | 状态 |
|------|------|
| 场景 (SampleScene) | 已搭建基本 Tilemap 及背景 |
| 素材资源 | 少量瓦片图 (tiles), 剑士1 精灵图, 战场背景预制体 |
| 脚本 | 仅有空占位脚本 `剑士1.cs` (类名 `NewBehaviourScript` 待修正) |
| 目标 | 从零构建完整游戏逻辑框架 |

---

## 二、整体架构设计

### 2.1 文件夹结构

```
Assets/
├── Scripts/                    # C# 脚本（核心逻辑）
│   ├── Core/                   # 游戏管理器、生命周期
│   ├── Units/                  # 单位行为（砰砰 & 敌人）
│   ├── Deploy/                 # 部署系统
│   ├── Combat/                 # 战斗逻辑（索敌、攻击、伤害）
│   ├── Pool/                   # 对象池
│   ├── UI/                     # UI 逻辑
│   ├── Waves/                  # 敌人波次管理
│   └── Data/                   # ScriptableObject 定义
├── Objects/                    # 精灵、预制体等资源
├── Scenes/                     # 场景文件
└── Prefabs/                    # 预制体（待创建）
```

### 2.2 核心数据流

```
[玩家点击部署] → DeploymentSystem → 消耗费用 → ObjectPool.Spawn(unitPrefab)
                                                  ↓
                                         Unit 实例化到 Grid 位置
                                                  ↓
         ┌────────────────── GameLoop (Update) ──────────────────┐
         ↓                                                       ↓
    [战斗阶段]                                              [胜负判定]
    Unit.Update():                                             Commander.HP <= 0 → Defeat
      → 索敌 (FindNearestEnemy)                                EnemyBase.HP <= 0 → Victory
      → 攻击 (Attack / FireProjectile)
      → 移动 (MoveTowardsEnemy)
```

---

## 三、模块划分与实现顺序

### 阶段 1：基础框架（优先级最高）

| # | 模块 | 说明 | 文件 |
|---|------|------|------|
| 1.1 | **GameManager** | 游戏总控制器，管理游戏状态（部署/战斗/胜利/失败） | `Core/GameManager.cs` |
| 1.2 | **UnitData (ScriptableObject)** | 所有单位的属性配置数据（血/攻/范围/速度/费用） | `Data/UnitData.cs` |
| 1.3 | **UnitBase** | 单位基类，包含 Stats、受伤、死亡逻辑 | `Units/UnitBase.cs` |
| 1.4 | **Commander** | 玩家指挥官（基地），血量归零判定 | `Units/Commander.cs` |

### 阶段 2：部署系统

| # | 模块 | 说明 |
|---|------|------|
| 2.1 | **DeploySystem** | 处理鼠标点击 → 网格坐标映射 → 单位实例化 |
| 2.2 | **EnergyManager** | 费用/能量管理，自动回复，UI 联动 |
| 2.3 | **GridHelper** | Tilemap 坐标与世界坐标转换工具 |

### 阶段 3：战斗系统

| # | 模块 | 说明 |
|---|------|------|
| 3.1 | **CombatSystem** | 战斗循环：索敌 → 攻击 → 冷却 |
| 3.2 | **MeleeAttack** | 近战攻击实现 |
| 3.3 | **RangedAttack** | 远程攻击实现（生成子弹） |
| 3.4 | **Projectile** | 子弹飞行与碰撞逻辑 |

### 阶段 4：敌人系统

| # | 模块 | 说明 |
|---|------|------|
| 4.1 | **EnemyBase** | 敌人基类（继承 UnitBase），自动向左移动 |
| 4.2 | **WaveManager** | 波次配置与生成，波间间隔 |
| 4.3 | **EnemySpawner** | 从屏幕右侧刷敌 |

### 阶段 5：对象池

| # | 模块 | 说明 |
|---|------|------|
| 5.1 | **ObjectPool** | 通用对象池，用于子弹、特效、单位回收 |
| 5.2 | **PooledObject** | 可回收对象接口/基类 |

### 阶段 6：UI

| # | 模块 | 说明 |
|---|------|------|
| 6.1 | **UI_EnergyBar** | 费用显示与更新 |
| 6.2 | **UI_CommanderHP** | 指挥官血量条 |
| 6.3 | **UI_DeployPanel** | 单位选择面板（点击部署） |
| 6.4 | **UI_GameOver** | 胜负结算界面 |

---

## 四、ScriptableObject 数据设计

```csharp
// UnitData.cs
[CreateAssetMenu(fileName = "NewUnitData", menuName = "砰砰军团/单位数据")]
public class UnitData : ScriptableObject
{
    public string unitName;
    public GameObject prefab;          // 模型预制体
    public Sprite icon;                // UI 图标
    public UnitType unitType;          // 近战/远程
    public int maxHP;
    public int attackPower;
    public float attackRange;
    public float attackInterval;       // 攻击间隔（秒）
    public float moveSpeed;
    public int deployCost;             // 部署所需费用
}
```

---

## 五、核心约定

### 命名规范
- 脚本文件名与类名**一致**，使用 `PascalCase`
- 私有字段：`_camelCase` 或 `[SerializeField] private` + `PascalCase`
- 方法：`PascalCase`
- 枚举：`PascalCase`

### 代码规范
- 所有序列化字段使用 `[SerializeField] private` 而非 `public`
- 公开属性使用属性（Property）包装
- 逻辑与数据分离：ScriptableObject 只存数据，MonoBehaviour 处理行为
- 使用事件（UnityEvent / Action）实现模块间解耦

### Git 协作
- 每个阶段完成后提交，commit message 格式：`[phase] 模块名: 简短说明`
- 避免大文件提交（Library、Logs 已在 .gitignore）

---

## 六、开发者注意事项

1. **对象池优先**：所有子弹、特效、频繁生成/销毁的对象必须走对象池
2. **网格映射**：部署时鼠标位置 → Tilemap cell position → 对齐到网格中心
3. **战斗索敌**：每帧检测攻击范围内最近的敌人（使用 `Vector2.Distance` 或 `Physics2D.OverlapCircle`）
4. **性能**：单位数量较多时考虑使用 `Physics2D.OverlapCircleNonAlloc` 减少 GC
5. **测试**：在阶段3完成后即可进行基本的部署+战斗闭环测试

---

## 七、待办清单

- [ ] 阶段 1：基础框架（GameManager + UnitData + UnitBase + Commander）
- [ ] 阶段 2：部署系统（DeploySystem + EnergyManager + GridHelper）
- [ ] 阶段 3：战斗系统（CombatSystem + 近战/远程攻击 + Projectile）
- [ ] 阶段 4：敌人系统（EnemyBase + WaveManager + EnemySpawner）
- [ ] 阶段 5：对象池（ObjectPool + PooledObject）
- [ ] 阶段 6：UI（费用/血量/面板/结算）
- [ ] 集成测试与调优

---

*计划版本：v0.1 — 2026-05-14*
