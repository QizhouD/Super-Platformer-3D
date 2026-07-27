---
name: unity-style-guide
description: 为 Unity 2022.3 LTS 3D 项目编写、修改或评审 C# 游戏代码时使用的开发风格指南。用于处理 MonoBehaviour 生命周期、物理与帧更新、ScriptableObject 配置和事件、Input System、有限状态机、对象生成、序列化引用、场景与预制体修改，以及 Unity 性能和验证工作；优先遵循当前仓库已经存在的架构与命名约定。
---

# Unity 开发风格指南

## 先确认项目事实

1. 阅读仓库根目录的 `AGENTS.md` 和目标目录附近的说明文件。
2. 检查相邻脚本、程序集定义、`Packages/manifest.json`、`ProjectSettings/ProjectVersion.txt` 和相关场景或预制体，再选择 API 与模式。
3. 搜索现有实现和调用点，确认组件实际挂载位置、标签、序列化字段、事件订阅方和场景依赖。
4. 不假设包、API、输入动作、标签、Layer、Animator 参数、资源路径或场景对象存在。无法从项目文件确认时，明确说明需要在 Unity Editor 中验证。

## 遵循本项目结构

- 将游戏代码放在 `Assets/_Project/Scripts/` 下。
- 对游戏逻辑使用 `Platformer` 命名空间；对计时器使用现有的 `Utilities` 命名空间。不要无理由创建新的顶层命名空间。
- 扩展已有系统，不创建功能重复的平行实现：行为优先接入 `StateMachine`，跨系统通知优先使用 `EventChannel<T>`，生成实体优先沿用 `EntityFactory<T>` 与生成策略。
- 玩家输入使用 `InputReader` 和生成的 `PlayerInputActions`，不要回退到旧版 `UnityEngine.Input`。
- 计时需求优先使用 `CountdownTimer` 或 `StopwatchTimer`，由拥有者显式调用 `Tick`。
- 新场景按项目约定放入 `Assets/_Project/Scenes/`；需要参与构建时，同时更新 Build Settings。不要直接编辑未要求改动的场景或预制体 YAML。
- 保持公开 API、序列化字段名和资源路径稳定。必须重命名序列化字段时，考虑用 `FormerlySerializedAs` 保留已有数据，并确认命名空间为 `UnityEngine.Serialization`。

## 编写组件

- 让每个 `MonoBehaviour` 只承担清晰的场景职责；把不依赖 Unity 生命周期的规则放入普通 C# 类，以便复用和测试。
- 在 `Awake` 建立组件自身依赖，在 `OnEnable` 订阅事件，在 `OnDisable` 对称退订，在 `Start` 处理依赖其他对象已完成 `Awake` 的初始化。
- 不依赖同一 GameObject 上不同组件的默认 `Awake` 或 `Update` 顺序；确有顺序要求时，显式建立调用关系或使用经过确认的 Script Execution Order。
- 每帧输入和非物理状态推进放在 `Update`；`Rigidbody` 移动和物理相关操作放在 `FixedUpdate`。帧率无关的变化使用传入的 `deltaTime`。
- 缓存反复使用的组件引用。避免在 `Update`、`FixedUpdate` 或高频回调中反复执行场景搜索、LINQ 查询、字符串拼接和不必要的分配。
- 需要组件约束时使用 `RequireComponent`；项目使用 KBCore.Refs 的组件继续沿用 `[Self]`、`[Child]`、`[Anywhere]` 和 `ValidateRefs()`，不要混入另一套自动注入方案。

## 管理序列化与配置

- 默认使用带 `[SerializeField]` 的私有字段暴露 Inspector 配置；只在其他类型确实需要访问时提供只读属性或方法。
- 使用 `ScriptableObject` 保存可复用配置或项目现有的事件通道，不把运行时实例状态意外写回共享资源。
- 在 `OnValidate` 中只做快速、确定且适合编辑器执行的校验或引用填充；不要放置依赖运行时场景状态的逻辑。
- 对 Inspector 必填引用尽早失败并给出可定位的错误。不要用静默的全局搜索掩盖错误配置。
- 修改 `.unity`、`.prefab`、`.asset` 或 Input Actions 资源前先检查现有引用；尽量通过 Unity Editor 或已安装且可用的编辑器集成修改，以保留序列化完整性。

## 使用现有行为模式

### 状态机

- 让状态实现 `IState`，并把进入、逐帧、物理帧和退出行为分别放入 `OnEnter`、`Update`、`FixedUpdate`、`OnExit`。
- 在拥有者中创建状态和转移，通过 `FuncPredicate` 表达简单条件；只把真正跨所有状态的规则注册为 Any Transition。
- 保证 `OnEnter` 与 `OnExit` 成对管理临时状态、动画标志、计时器或事件订阅。
- 注册状态后再调用 `SetState`；当前实现按状态的运行时类型存储节点，因此不要为同一状态类型创建多个需要独立注册的实例，除非先调整状态机设计。

### 事件通道

- 用强类型的 `EventChannel<T>` 解耦发布者和监听者，沿用现有 `EventListener<T>` 注册模型。
- 发布者只发布业务数据，不直接了解 UI 或其他消费者。
- 避免在观察者集合迭代期间注册或注销监听器；如果需求可能触发这种情况，先评估并安全调整事件通道实现。

### 输入

- 通过 `InputReader` 事件消费动作，在 `OnEnable` 与 `OnDisable` 对称订阅和退订。
- 根据 `InputAction.CallbackContext.phase` 区分按下、持续与释放语义；不要把一个动作的每个 phase 都当成一次触发。
- 修改 `.inputactions` 后使用 Unity Input System 的代码生成流程更新包装类，不手工维护生成代码。

### 生成与生命周期

- 通过 `EntityData` 提供预制体配置，通过现有工厂实例化，通过 `ISpawnPointStrategy` 选择位置。
- 在随机选择前验证配置数组非空，在返回实体前确认预制体包含期望的 `Entity` 派生组件。
- 只有分析表明频繁 `Instantiate`/`Destroy` 造成实际问题时才引入对象池；不要为了模式本身增加复杂度。

## 保持 Unity 性能可预测

- 先定位热点，再优化。区分编辑器开销、GC 分配、渲染、脚本和物理成本。
- 避免在热路径调用 `FindObjectOfType`、`FindGameObjectWithTag`、`GetComponent` 或创建临时集合；初始化阶段的一次性调用可以在确认成本可接受后保留。
- 比较标签时优先使用 `CompareTag`，并先确认标签已在项目设置中定义。
- 不把所有逻辑塞进全局单例；优先使用明确引用、状态所有权和项目已有事件通道。
- 不提前引入 DOTS、Addressables、第三方依赖或自定义框架。只有任务要求且项目已安装或用户批准时才采用。

## 修改与验证流程

1. 先阅读目标脚本及其调用方、场景引用和同类实现。
2. 做满足需求的最小改动，保留无关代码和用户已有修改。
3. 检查编译层面的命名空间、类型、生成代码和包版本兼容性。
4. 能运行 Unity 时，检查 Console 无新增错误，并在相关场景中进入 Play Mode 验证。此项目从 `Level_Tutorial` 开始，再按需要验证 `Level_1`。
5. 验证正常路径、边界条件、禁用/重新启用组件、场景重载和事件退订。
6. 无法启动 Unity Editor 时，明确列出未完成的编辑器验证，不声称已经通过 Play Mode 或构建。

## 输出要求

- 说明改动影响的组件、资源和场景。
- 区分已通过的静态检查、编译、Play Mode 和构建验证。
- 对需要 Inspector 配置、标签、Layer、Animator 参数或 Build Settings 操作的事项给出准确步骤，并标注哪些仍需人工完成。
- 不声称不存在的测试、CI、菜单项或编辑器工具已经运行。
