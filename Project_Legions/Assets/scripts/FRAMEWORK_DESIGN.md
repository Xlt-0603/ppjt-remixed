# 砰砰军团 — 基础框架设计（节拍驱动 · 可视化）

## 一、核心思想：节拍驱动

游戏使用**音乐化的节拍系统**而非传统帧循环：

```
小节 (Bar) = 1 秒
  ├─ 拍 1 (Beat 1)
  ├─ 拍 2
  ├─ 拍 3
  ├─ 拍 4
  ├─ 拍 5
  ├─ 拍 6
  ├─ 拍 7
  └─ 拍 8


|--- Bar 1 ---|--- Bar 2 ---|--- Bar 3 ---|...
   1 2 3 4 5 6 7 8   1 2 3 4 5 6 7 8   1 2 3 4 5 6 7 8
```

### 为什么用节拍？
- **可视化**：像乐谱一样观察战局，每拍都是可暂停的瞬间
- **调试友好**：可逐拍步进，所有单位状态在每拍结束时定格
- **节拍器**：随时调速（慢放检视 / 快进跳过）

### 单位节奏
- 攻击间隔以"拍"为单位（如"每 4 拍攻击一次"）
- 移动速度以"格/拍"为单位（如"每拍移动 1 格"）

---

## 二、GameManager — 全局节拍引擎

```
┌──────────────────────────────────────────────────┐
│                   GameManager                     │
│                                                    │
│  ┌──────────┐    ┌──────────────────────────────┐ │
│  │ GameFSM  │    │         BeatEngine            │ │
│  │          │    │  Bar (int)                    │ │
│  │ - Deploy │    │  Beat (1-8)                   │ │
│  │ - Battle │    │  BPM (float, 默认 60)         │ │
│  │ - Win    │    │  → 每拍广播 OnBeat()          │ │
│  │ - Lose   │    └──────────────────────────────┘ │
│  └──────────┘                                      │
│                      ┌──────────────────────────┐  │
│                      │  节拍器 (Metronome)       │  │
│                      │  ┌──── [▶/⏸] ────┐       │  │
│                      │  │ BPM: [60]  ±  │       │  │
│                      │  │ 小节: 3  拍: 5 │       │  │
│                      │  │ [⏮] [⏭步进] [⏹] │       │  │
│                      │  └────────────────┘       │  │
│                      └──────────────────────────┘  │
└──────────────────────────────────────────────────────┘
```

### BPM = 60 → 1 秒一小节，8 拍
- BPM = 60 → 每分钟 60 小节 = 每秒 1 小节 = 每拍 0.125s
- BPM = 120 → 每分钟 120 小节 = 每秒 2 小节 = 每拍 0.0625s
- BPM = 30 → 每分钟 30 小节 = 每秒 0.5 小节 = 每拍 0.25s

### GameFSM 状态流转

```
    ┌───────────────────────────────┐
    │  Deploy（部署阶段）             │  ← 放置单位，能量自动回复
    └───────────┬───────────────────┘
                │ 点击"开战" / 自动结束
                ↓
    ┌───────────────────────────────┐
    │  Battle（战斗阶段）             │  ← 每拍自动执行：伤害·移动·死亡
    └───────────┬───────────────────┘
                │
    ┌───────────┴───────────┐
    ↓                       ↓
┌───────┐              ┌───────┐
│  Win  │              │ Lose  │
└───────┘              └───────┘
```

### 关键 API

```csharp
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int Bar { get; private set; }           // 当前小节
    public int Beat { get; private set; }           // 当前拍 (1-8)
    public float BPM { get; set; }                  // 可随时调整
    public GameState State { get; private set; }

    public event Action<int, int> OnBeat;           // 每拍广播 (bar, beat)
    public event Action<GameState> OnStateChanged;

    // 节拍器控制
    [ContextMenu("步进一拍")] public void StepBeat();
    [ContextMenu("暂停/继续")] public void TogglePause();
    public void SetBPM(float bpm);
}
```

---

## 三、核心循环：每拍做什么

```
OnBeat(bar, beat)
  │
  ├─ 1. 所有存活单位执行
  │      ├─ 索敌 (FindNearestEnemy)
  │      ├─ 有目标 & 在攻击范围 → 造成伤害
  │      ├─ 有目标 & 不在范围   → 向目标移动 1 格
  │      └─ 无目标              → 向敌方基地移动 1 格
  │
  ├─ 2. 伤害结算
  │      ├─ 累积的伤害统一扣除 HP
  │      └─ HP <= 0 → 标记死亡
  │
  ├─ 3. 死亡处理
  │      ├─ 移除死亡单位
  │      └─ 触发死亡事件（回收/特效）
  │
  └─ 4. 胜负判定
         ├─ 指挥官 HP <= 0 → Lose
         ├─ 敌人基地 HP <= 0 → Win
         └─ 否则 → 等待下一拍
```

**每拍只做一件事**：要么攻击、要么移动。没有复杂的帧间插值，逻辑纯粹而确定。

---

## 四、UnitBase — 单位基类

```csharp
public class UnitBase : MonoBehaviour
{
    [SerializeField] protected UnitData data;

    protected int _currentHP;
    protected int _attackCooldown;       // 剩余冷却（拍数）
    protected UnitBase _currentTarget;
    protected bool _isDead;

    // 每拍调用一次
    public virtual void OnBeat(int bar, int beat)
    {
        if (_isDead) return;

        _currentTarget = FindNearestEnemy();

        if (_currentTarget != null && InAttackRange(_currentTarget))
            TryAttack(_currentTarget);
        else if (_currentTarget != null)
            MoveOneStepTowards(_currentTarget.transform.position);
        else
            MoveOneStepTowards(EnemyBasePosition);
    }

    protected void TryAttack(UnitBase target)
    {
        if (_attackCooldown > 0) { _attackCooldown--; return; }
        target.TakeDamage(data.attackPower);
        _attackCooldown = data.attackIntervalInBeats;
    }

    public virtual void TakeDamage(int damage)
    {
        _currentHP -= damage;
        if (_currentHP <= 0) Die();
    }

    protected virtual void Die()
    {
        _isDead = true;
        // 回收至对象池 / 播放死亡特效
    }

    // 向目标方向移动一格
    protected void MoveOneStepTowards(Vector3 target);
}
```

---

## 五、Commander — 指挥官

```csharp
public class Commander : MonoBehaviour
{
    public int maxHP = 20;
    public int currentHP { get; private set; }

    public event Action<int, int> OnHPChanged;
    public event Action OnDeath;

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        OnHPChanged?.Invoke(currentHP, maxHP);
        if (currentHP <= 0) OnDeath?.Invoke();
    }
}
```

---

## 六、节拍器调试面板

Editor / Dev Build 下显示的 HUD：

```
┌─────────────────────────────────────┐
│  ♩ 节拍器                           │
│  ┌───────────────────────┐          │
│  │  BPM: [60]  [－] [+ ] │          │
│  │  小节 3 │ 拍 5        │          │
│  │  [▶] [⏸] [⏭步进] [⏹] │          │
│  └───────────────────────┘          │
│  ───────────────────────────────── │
│  单位                              │
│  ├ 剑士1  HP:8/12  攻击→史莱姆    │
│  ├ 弓手2  HP:7/7   移动→          │
│  └ 史莱姆 HP:3/10  攻击→剑士1     │
└─────────────────────────────────────┘
```

- **BPM 滑块**：实时调整节奏，慢放检视或快进
- **步进按钮**：第一次按暂停，后续每按一次走一拍
- **状态列表**：每个单位当前 HP 和行为

---

## 七、初始化流程

```
GameManager.Awake()
  ├─ 缓存 Commander、Grid 引用
  ├─ 设置 BPM = 60（每秒 1 小节，每拍 0.125s）
  ├─ 切换至 Deploy 状态
  │
  Deploy 阶段
  ├─ 玩家放置单位（节拍器继续，可步进）
  ├─ 点击"开战" → 切换至 Battle
  │
  Battle 阶段
  ├─ BeatEngine 自动循环 OnBeat
  ├─ 每拍 → 伤害 → 移动 → 死亡 → 胜负判定
  └─ 胜负触发 → Win / Lose
```

---

## 八、场景层级与挂载方式

### 推荐 Hierarchy 结构

```
场景 (SampleScene)
│
├─ Managers（空 GameObject）
│   └─ GameManager.cs  ← 挂载 GameManager
│   └─ MetronomeUI.cs  ← 挂载 MetronomeUI（挂在同一个或子物体上）
│
├─ Commander（空 GameObject）
│   └─ Commander.cs    ← 挂载 Commander
│   └─ SpriteRenderer   ← 指挥官模型
│
├─ Grid（已有 Tilemap 结构）
│   └─ Tilemap
│
├─ Units（空 GameObject，运行时单位放这里）
│
└─ UI（Canvas）
    └─ 节拍器面板文本 / 按钮
```

### 各脚本归属说明

| 脚本 | 挂载位置 | 说明 |
|------|----------|------|
| `GameManager` | `Managers` 物体 | 单例，`DontDestroyOnLoad` 可选 |
| `MetronomeUI` | `Managers` 物体 | 控制节拍器面板显示，引用 GameManager |
| `UnitData` (SO) | **不挂载** | 在 Project 窗口中 `右键 → Create → 砰砰军团 → 单位数据` 创建 |
| `GameEnums` | **不挂载** | 纯枚举定义，无 MonoBehaviour |
| `UnitBase` | 运行时生成的单位预制体 | 每个单位的预制体上挂载这个脚本 |
| `Commander` | `Commander` 物体 | 固定在场景中 |
| `EnemyBase` | 运行时生成的敌人预制体 | 继承 UnitBase |

### 关键原则

- **Manager 物体**：放全局控制器，永不移动/销毁
- **预制体**：UnitBase 和 EnemyBase 不放在场景里，而是做成 Prefab，运行时实例化
- **ScriptableObject**：纯数据资产，放在 Project 面板的 Assets 中，通过 Inspector 拖拽赋值

---

## 九、文件清单（阶段 1）

| 文件 | 类 | 职责 |
|------|-----|------|
| `Core/GameManager.cs` | `GameManager` | 全局控制器 + BeatEngine + GameFSM |
| `Core/MetronomeUI.cs` | `MetronomeUI` | 节拍器 HUD，调速与步进 |
| `Data/UnitData.cs` | `UnitData` (SO) | 单位属性配置 |
| `Data/GameEnums.cs` | `GameState`, `UnitType` | 枚举定义 |
| `Units/UnitBase.cs` | `UnitBase` | 单位基类（伤害·移动·死亡） |
| `Units/Commander.cs` | `Commander` | 指挥官基地 |
| `Units/EnemyBase.cs` | `EnemyBase` | 敌人基类（自动向左走） |

---

## 九、阶段 1 完成后能看到什么

1. 部署几个砰砰和一个敌人，点击开战
2. 左上的节拍器跳动：`♩ 小节 3 | 拍 5`
3. 单位每拍移动一格或攻击一次
4. 节拍器调 BPM 到 10 → 慢动作；调 BPM 到 200 → 快进
5. 点击暂停 + 步进，逐拍观察伤害和移动
6. 指挥官 HP 归零 → Game Over

---

*设计版本：v0.2 — 2026-05-14*
