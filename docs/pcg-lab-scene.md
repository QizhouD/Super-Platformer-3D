# PCG_Lab 详细技术文档

> 数据来源：Unity MCP 实时查询（`manage_scene` / `find_gameobjects` / `execute_code` SerializedObject / `manage_camera` / `read_console` / `manage_script`）。
> 采样时刻：Unity Editor **未进入 Play Mode**（`playing=False`，`paused=False`）。场景有未保存修改。
> 引擎：Unity **2022.3.2f1**，产品名 `Platformer`，公司 `DefaultCompany`。

---

## 1. 场景概览

| 项 | MCP 实测值 |
|---|---|
| 场景名称 | `PCG_Lab` |
| 完整路径 | `Assets/_Project/Scenes/PCG_Lab.unity` |
| 是否已加载 | 是（当前唯一已加载场景） |
| 是否活动场景 | 是 |
| 是否脏（未保存） | **是**（`isDirty=True`） |
| `Scene.buildIndex` | **4**（`get_active` 与 `SceneManager.GetActiveScene().buildIndex` 一致） |
| Build Settings 列表位置 | 第 6 条，工具返回 `buildIndex=5`，`enabled=true`，GUID `b71fc096aebac3b4384ce614dcccfdd4` |
| 加载模式 | 单场景加载（`get_loaded_scenes` 仅 1 个场景） |
| 根对象数 | **6** |
| Transform 总数（含未激活） | **75** |
| Component 总数 | **142** |
| MonoBehaviour 总数 | **32** |
| 场景校验 | `validate`：**clean**（missing scripts=0，broken prefabs=0） |

**Build Index 差异说明（实测，非猜测）：** Build Settings 中第一条 `Assets/_Demo/Scenes/Level1.unity` 为 `enabled=false`。`manage_scene.get_build_settings` 按列表下标给 `PCG_Lab` 标为 5；编辑器 `Scene.buildIndex` 返回 4。启用场景顺序为：

1. `Main Menu`（enabled）
2. `Level_Tutorial`
3. `Level_1`
4. `VictoryScene`
5. **`PCG_Lab`**

**目的（基于场景对象与脚本，而非推测）：** 这是 PCG 实验室场景。编辑器里只有起点平台、玩家、相机与 PCG 系统；`Generated Chunks` 子物体数为 **0**。`LevelGenerator.generateOnStart=True`，进入 Play Mode 后按 seed `82431` 生成关卡。`PCGDebugPanel.Awake()` 会调用 `PCGLabExperience.EnsureInstalled()`，运行时再安装表现层（HUD、灯光/雾、平台换肤、SFX）。

**整体规模：** 编辑器态很小（6 根、75 Transform）。玩家骨骼占 59 个 Transform。正式内容在运行时由 14 个 chunk 资源实例化（配置 `initialChunkCount=16`）。

---

## 2. 完整层级结构

格式：`Name` `[instanceID]` `tag` `layer` `activeSelf/activeInHierarchy`

```
PCG_Lab
├─ Player 2 [29262]  tag=Player  layer=Default(0)  True/True
│    prefab=Connected → Assets/_Project/Prefabs/Player 2.prefab
│    ├─ Body05 [-1676]  Untagged Default  True/True     SkinnedMeshRenderer
│    ├─ Body10 [-1682]  Untagged Default  False/False   SkinnedMeshRenderer
│    ├─ Cloak02 [-1688] Untagged Default  True/True     SkinnedMeshRenderer
│    ├─ Cloak03 [-1694] Untagged Default  False/False   SkinnedMeshRenderer
│    └─ root [-1700]    Untagged Default  True/True
│         └─ pelvis [-1704]  (骨骼继续向下；Player 2 子树共 59 个 Transform，未全部展开)
│
├─ Directional Light [29366]  Untagged Default  True/True
│
├─ Start Platform [29362]  Untagged Default  True/True
│    ├─ Platform [29352]       Untagged  Ground(7)  True/True
│    ├─ Start Anchor [29348]   Untagged  Default    True/True
│    └─ Player Spawn [29304]   Untagged  Default    True/True
│
├─ CameraSystem [29372]  Untagged Default  True/True
│    ├─ Main Camera [29332]  tag=MainCamera  Default  True/True
│    └─ FreeLook Camera [29274]  Untagged Default  True/True
│         ├─ TopRig [29342]     CinemachineVirtualCamera
│         │    └─ cm            CinemachinePipeline, OrbitalTransposer, Composer
│         ├─ MiddleRig [29280]  CinemachineVirtualCamera
│         │    └─ cm            CinemachinePipeline, OrbitalTransposer, Composer
│         └─ BottomRig [29242]  CinemachineVirtualCamera
│              └─ cm            CinemachinePipeline, OrbitalTransposer, Composer
│
├─ Generated Chunks [29308]  Untagged Default  True/True   (childCount=0)
│
└─ PCG System [29286]  Untagged Default  True/True
```

未激活对象：`Player 2/Body10`、`Player 2/Cloak03`。其余根对象及其可见子物体均为激活。

---

## 3. 关键 GameObject 与组件详情

### 3.1 Player 2

| 项 | 值 |
|---|---|
| Instance ID | `29262`（Transform `29260`） |
| Tag / Layer | `Player` / `Default (0)`（项目存在 `Player` layer=3，**本对象未使用**） |
| Prefab | `Connected`，源 `Assets/_Project/Prefabs/Player 2.prefab`，outermost=true |
| Prefab 修改 | 共 15 条；有意义覆盖 3 条：`PlayerController.feel`、`input`、`freeLookVCam` |
| Local / World Position | `(0, 1.5, -3.5)` |
| Rotation / Scale | `(0,0,0)` / `(1,1,1)` |

**挂载组件：**

| 组件 | 关键序列化值 |
|---|---|
| Animator | controller=`Assets/_Project/Animation/Player.controller`；avatar=`Idle_Battle_SwordAndShiledAvatar`（`Assets/RPG Tiny Hero Duo/Animation/SwordAndShield/Idle_Battle_SwordAndShiled.fbx`）；`applyRootMotion=True`；`culling=CullUpdateTransforms`；`updateMode=Normal` |
| CapsuleCollider | radius=0.5，height=1.6，center=`(0, 0.70, 0)`，Y-Axis，**非 Trigger** |
| Rigidbody | mass=1，drag=0，angularDrag=0.05，`useGravity=True`，`isKinematic=False`，`Interpolate=None`，`CollisionDetection=Discrete`，constraints=0（`PlayerController.Awake` 运行时会改 freezeRotation / 关重力 / Interpolate / Continuous） |
| GroundChecker | 见第 4 节 |
| Health | maxHealth=100 |
| PlayerController | 见第 4 节 |
| PlatformCollisionHandler | 无序列化字段 |
| ResetPlayer | fallThreshold=**-20** |
| PCGPlayerAbilityBridge | `playerController=null`（Awake 会 `GetComponent`） |
| BehaviorParameters | BehaviorName=`PCGNavigation`；BehaviorType=**1 (HeuristicOnly)**；Model=**null**；VectorObservationSize=**20**；NumStackedVectorObservations=1 |
| RenderTextureSensorComponent | SensorName=`PCGVisualObservation`；RenderTexture=**null**（运行时由 Agent 绑定）；Grayscale=False；ObservationStacks=1；Compression=1 |
| PCGNavigationAgent | 见第 4 节 |
| DecisionRequester | DecisionPeriod=**5**；TakeActionsBetweenDecisions=True |

外观：`Body05`、`Cloak02` 激活；`Body10`、`Cloak03` 关闭。

### 3.2 Start Platform

| 对象 | ID | World Pos | 组件 |
|---|---|---|---|
| Start Platform | 29362 | `(0, 0, -3.5)` | 仅 Transform |
| Platform | 29352 | `(0, 0, -3.5)` | MeshFilter=`Cube`；BoxCollider size=`(1,1,1)`；MeshRenderer；scale=`(8, 1, 7)`；**Layer=Ground(7)** |
| Start Anchor | 29348 | `(0, 0.50, 0.00)` | 仅 Transform（LevelGenerator 对齐起点） |
| Player Spawn | 29304 | `(0, 1.50, -3.50)` | 仅 Transform（与玩家当前位置重合） |

Platform 材质：`NoiseGround`，shader=`ShaderTest/NoiseGround`，路径 `Assets/_Project/_Shaders/NoiseGround.mat`。CastShadows=On，ReceiveShadows=True。

### 3.3 CameraSystem

见第 5.1 节。`CameraManager` 挂在根 `CameraSystem` 上，不在 Main Camera 上。

### 3.4 Generated Chunks

空容器。Transform ID `29310`，被 `LevelGenerator.generatedRoot` 引用。编辑器态 **0 children**。

### 3.5 PCG System

非 Prefab。世界原点。7 个自定义脚本全部 enabled，互相引用几乎都指向本对象或 Player 2 / Main Camera / Start Anchor。详见第 4 节。

### 3.6 Directional Light

见第 5.2 节。

---

## 4. 脚本使用与配置详解

场景中按 `by_component=MonoBehaviour` 搜到 11 个 GameObject。自定义游戏脚本如下（不含 Cinemachine / ML-Agents 包组件，那些列在第 5 节）。

### 4.1 挂载一览

| 脚本 | 命名空间 | 挂载位置 | 源码路径 |
|---|---|---|---|
| `LevelGenerator` | `Platformer.PCG` | PCG System | `Assets/_Project/Scripts/PCG/Runtime/LevelGenerator.cs` |
| `PCGRunController` | `Platformer.PCG` | PCG System | `Assets/_Project/Scripts/PCG/Runtime/PCGRunController.cs` |
| `PCGRunTelemetry` | `Platformer.PCG` | PCG System | `Assets/_Project/Scripts/PCG/Runtime/PCGRunTelemetry.cs` |
| `PCGAdaptiveDifficultyDirector` | `Platformer.PCG` | PCG System | `Assets/_Project/Scripts/PCG/Runtime/PCGAdaptiveDifficultyDirector.cs` |
| `PCGGameAIObservationSensor` | `Platformer.PCG` | PCG System | `Assets/_Project/Scripts/PCG/Runtime/PCGGameAIObservationSensor.cs` |
| `PCGMultimodalDatasetRecorder` | `Platformer.PCG` | PCG System | `Assets/_Project/Scripts/PCG/Runtime/PCGMultimodalDatasetRecorder.cs` |
| `PCGDebugPanel` | `Platformer.PCG` | PCG System | `Assets/_Project/Scripts/PCG/Runtime/PCGDebugPanel.cs` |
| `PCGNavigationAgent` | `Platformer.PCG` | Player 2 | `Assets/_Project/Scripts/PCGNavigationAgent.cs` |
| `PCGPlayerAbilityBridge` | **无命名空间** | Player 2 | `Assets/_Project/Scripts/PCGPlayerAbilityBridge.cs` |
| `PlayerController` | `Platformer` | Player 2 | `Assets/_Project/Scripts/PlayerController.cs` |
| `GroundChecker` | `Platformer` | Player 2 | `Assets/_Project/Scripts/GroundChecker.cs` |
| `Health` | `Platformer` | Player 2 | `Assets/_Project/Scripts/Health.cs` |
| `PlatformCollisionHandler` | `Platformer` | Player 2 | `Assets/_Project/Scripts/PlatformCollisionHandler.cs` |
| `ResetPlayer` | **无命名空间** | Player 2 | `Assets/_Project/Scripts/ResetPlayer.cs` |
| `CameraManager` | `Platformer` | CameraSystem | `Assets/_Project/Scripts/CameraManager.cs` |

**编辑器场景中不存在、但会在运行时挂上的脚本：** `PCGLabExperience`（`PCGDebugPanel.Awake` → `EnsureInstalled`，加到 PCG System 同一物体）。它再拉起 `PCGLabVisualStyler`、`PCGLabAudio`、`PCGLabHud`。

### 4.2 `LevelGenerator`（PCG System）

**功能：** Play Mode `Start()` 若 `generateOnStart` 则 `Generate()`。按 seed 确定性选 chunk、对齐 Entry/Exit、重叠检测、失败则安全回退、可选项目资源装饰、每个 Exit 生成 `ChunkCheckpoint`。完成后触发 `GenerationFinished`。

| 字段 | 当前值 |
|---|---|
| config | `LevelGenerationConfig` → `Assets/_Project/PCG/LevelGenerationConfig.asset` |
| startAnchor | Transform `Start Anchor`（sceneId 29350） |
| generatedRoot | Transform `Generated Chunks`（sceneId 29310） |
| seed | **82431** |
| chunkCountOverride | 0（使用 config.initialChunkCount=16） |
| generateOnStart | True |
| difficultyBias | 0 |
| maximumConsecutiveFlatChunks | 3 |
| maximumConsecutiveStraightChunks | 3 |
| minimumRelativeElevation | -4 |
| maximumRelativeElevation | 8 |
| hasDoubleJump | **False** |
| hasDash | **False** |
| traversalCapabilities.baseHorizontalReach | 6.5 |
| traversalCapabilities.baseVerticalReach | 1.8 |
| traversalCapabilities.dashHorizontalBonus | 5 |
| traversalCapabilities.doubleJumpVerticalBonus | 2.5 |

**Config 资源 `LevelGenerationConfig`：**

| 字段 | 值 |
|---|---|
| chunks | 14 个 `PlatformChunkData`（见附录） |
| initialChunkCount | 16 |
| chunksAhead | 4 |
| maximumGenerationAttempts | 20 |
| difficultyByProgress | AnimationCurve |
| checkpointInterval | 5 |
| recoveryChunkCooldown | 3 |
| overlapPadding | 0.08 |
| maximumConsecutiveCategory | 2 |
| reachSafetyFactor | 0.82 |
| syncReachFromPlayer | True |
| useRhythmPlan | True |
| spawnSafeFallback | True |
| decorateWithProjectAssets | True |

交互：被 `PCGDebugPanel`、`PCGAdaptiveDifficultyDirector`、`PCGPlayerAbilityBridge`（`FindObjectOfType<LevelGenerator>` + `SetTraversalCapabilities`）、`PCGNavigationAgent` 引用。事件：`GenerationFinished`。

### 4.3 `PCGRunController`（PCG System）

**功能：** 订阅静态事件 `ChunkCheckpoint.Reached`；记录最远检查点；`Update` 中 `player.y < fallThreshold` 则 Respawn；用 Rigidbody 传送并清速度。

| 字段 | 值 |
|---|---|
| player | Transform `Player 2`（29260） |
| initialSpawn | Transform `Player Spawn`（29306） |
| fallThreshold | **-12** |

Awake 后备：若 player 空则 `FindGameObjectWithTag("Player")`。事件：`CheckpointReached`、`PlayerRespawned`。

### 4.4 `PCGRunTelemetry`（PCG System）

**功能：** 记录生成完成、检查点、重生、定时平台状态；按间隔抓玩家位置/速度快照。

| 字段 | 值 |
|---|---|
| generator / runController / player | 均已绑定 PCG System / Player 2 |
| eventCapacity | 256 |
| snapshotInterval | 0.25 |

交互：C# 事件订阅 `LevelGenerator`、`PCGRunController`、静态 `PCGTimedPlatform.StateChanged`。

### 4.5 `PCGAdaptiveDifficultyDirector`（PCG System）

**功能：** 用检查点耗时与重生次数估计技能，平滑后写入 `difficultyBias`（范围由 `maximumDifficultyBias` 限制），并立刻改动动态平台参数。

| 字段 | 值 |
|---|---|
| generator / runController / telemetry | 均已绑定 |
| adaptiveDifficultyEnabled | True |
| smoothing | 0.25 |
| maximumDifficultyBias | 0.2 |
| model | Generic（默认构造 `skillEstimate=0.5`） |

Awake 后备：`FindObjectOfType` 补引用。

### 4.6 `PCGGameAIObservationSensor`（PCG System）

**功能：** 每 0.2s 采 20 维结构化观察；每 0.5s 从 Main Camera 抓 84×84 视觉帧。

| 字段 | 值 |
|---|---|
| generator / runController / player | 已绑定 |
| observationCamera | Camera `Main Camera`（29334） |
| difficultyDirector | 已绑定 |
| structuredObservationInterval | 0.2（5 Hz） |
| visualObservationInterval | 0.5（2 Hz） |
| captureVisualFrames | True |

常量：`VisualWidth=84`，`VisualHeight=84`。`Update` 中 `FindObjectsOfType<PCGTimedPlatform>()`（热路径分配，见第 7 节）。

### 4.7 `PCGMultimodalDatasetRecorder`（PCG System）

**功能：** 把观察/帧/奖励写成 `Application.persistentDataPath/PCGDatasets/<episode-id>/`。

| 字段 | 值 |
|---|---|
| 五个系统引用 | 均已绑定 |
| saveVisualFrames | True |
| saveEveryNthVisualFrame | 1 |

由 Debug Panel 启动/停止，编辑器态未在录制。

### 4.8 `PCGDebugPanel`（PCG System）

**功能：** IMGUI 实验室面板（seed、双跳/冲刺、自适应难度、训练模式、复制 seed/manifest/telemetry、数据集录制）。`Awake` 安装 `PCGLabExperience`。运行时 HUD 激活后可隐藏旧 IMGUI（`HideLegacyGui`）。

| 字段 | 值 |
|---|---|
| generator / player / runController / telemetry / difficultyDirector / observationSensor / datasetRecorder | 均已绑定 |
| trainingControllerBehaviour | `PCGNavigationAgent` on Player 2（sceneId 29264） |

运行时状态字段（seedText、doubleJump 等）不在 Inspector 序列化，Awake 从 generator.Seed 填 `"82431"`。

### 4.9 `PlayerController`（Player 2）

**功能：** 继承 `ValidatedMonoBehaviour`（KBCore.Refs）。`Awake` 建状态机（Locomotion / Jump / Dash / Attack），把 FreeLook Follow/LookAt 绑到自身，订阅 `InputReader`。`Start()` 调用 `input.EnablePlayerActions()`。

| 字段 | 当前 Inspector 值 |
|---|---|
| rb / groundChecker / animator | Self 引用，已填 |
| freeLookVCam | CinemachineFreeLook `FreeLook Camera`（29278） |
| input | `InputReader` → `Assets/_Project/ScriptableObjects/InputReader.asset` |
| feel | `PlayerFeelConfig` → `Assets/_Project/ScriptableObjects/PlayerFeelConfig.asset` |
| moveSpeed | **300** |
| rotationSpeed | **300** |
| smoothTime | 0.2 |
| jumpForce | 10 |
| jumpDuration | 0.5 |
| jumpCooldown | 0 |
| gravityMultiplier | 3（运行时下落重力实际走 `feel.fallGravity=2`，属性 `GravityMultiplier => FallGravity`） |
| allowDoubleJump | Inspector **False** |
| dashForce | 5 |
| dashDuration | 1 |
| dashCooldown | 2 |
| allowDash | **False** |
| attackCooldown | 0.5 |
| attackDistance | 1 |
| attackDamage | 10 |

**源码硬编码（与 Inspector 不一致）：** `Awake()` 内 `allowDoubleJump = true; hasDoubleJump = true;`。`PCGPlayerAbilityBridge.Awake()` 再调用 `SetTraversalAbilities(true, AllowDash)`。`PlayerFeelConfig.startWithDoubleJump=False` 注释写明不被跳跃次数使用。

C# 事件：`Jumped`、`Dashed`、`Landed`。

### 4.10 `PCGPlayerAbilityBridge`（Player 2）

**功能：** 把玩家能力同步到 `LevelGenerator`（当 config.`syncReachFromPlayer`）；把跳跃/冲刺转成 `PCGLabSignals`。

| 字段 | 值 |
|---|---|
| playerController | Inspector **null**（Awake/OnEnable `GetComponent<PlayerController>()`） |

`SyncTraversalToGenerator()` 使用 `Object.FindObjectOfType<LevelGenerator>()`。

### 4.11 `PCGNavigationAgent`（Player 2）

**功能：** ML-Agents `Agent` + `IPCGTrainingController`。观察 20 维向量；动作：2 连续轴 + 2 离散（jump/dash）。通过 `InputReader.SetExternalDirection/SendExternalJump/SendExternalDash` 驱动同一套玩家状态机。Heuristic 回读人类输入。`trainingMode=false` 时 BehaviorType=`HeuristicOnly`。

| 字段 | 值 |
|---|---|
| input | 同一 `InputReader` 资源 |
| generator / runController / observationSensor | PCG System 上对应组件 |
| navigationCamera | Main Camera |
| trainingMode | **False** |
| checkpointReward | 1 |
| completionReward | 5 |
| deathPenalty | -1 |
| decisionPenalty | -0.0005 |
| approachRewardScale | 0.01 |
| MaxStep（序列化） | 0（`Initialize()` 会 `Max(MaxStep, 5000)`） |

订阅 `PCGRunController` 检查点/重生。Console 曾出现 ML-Agents Academy 连 trainer `port 5004` 失败、改走 inference 的日志（见第 8 节）。当前 Behavior Model=null，且非 Play Mode。

### 4.12 玩家辅助脚本

**GroundChecker**

| 字段 | 值 |
|---|---|
| groundDistance | 0.08 |
| probeRadius | 0.22 |
| skinWidth | 0.08 |
| coyoteTime | 0.1 |
| extraRays | 4 |
| groundLayers | **128** = `1<<7` = **Ground** |
| drawGizmos | True |

SphereCast + 环向 Raycast + CheckSphere，只打 Ground。`PlayerController` 在 feel 存在时会 `SetCoyoteTime(feel.coyoteTime)`（同为 0.1）。

**Health：** `maxHealth=100`；`playerHealthChannel` = `FloatEventChannel` `PlayerHealthPercentage`（`Assets/_Project/ScriptableObjects/EventChannels/PlayerHealthPercentage.asset`）。本场景 **没有** EventListener / UI 血条。

**PlatformCollisionHandler：** 碰撞 tag=`MovingPlatform` 且法线向上则 `SetParent`。生成后的 `moving_01` 依赖此 tag。

**ResetPlayer：** `fallThreshold=-20`；`Start` 记录 `originalPos`；掉到阈值后传送回出生点。与 `PCGRunController`（-12，检查点）并行存在。

### 4.13 `CameraManager`（CameraSystem）

| 字段 | 值 |
|---|---|
| input | 同一 InputReader |
| freeLookVCam | FreeLook Camera |
| speedMultiplier | **2** |

订阅 `Look`、`EnableMouseControlCamera`、`DisableMouseControlCamera`。RMB 锁定光标；仅 RMB 按住时鼠标才驱动轴。写入 `m_XAxis/m_YAxis.m_InputAxisValue`。

### 4.14 共享 ScriptableObject

**InputReader**（无额外序列化字段）：实现 `IPlayerActions`；人类输入与 Agent 外部输入共用。`Direction` 在 `ExternalControlEnabled` 时走外部向量。`PCGNavigationAgent.SetTrainingMode` 会 `SetExternalControlEnabled(true)`。

**PlayerFeelConfig：** coyoteTime=0.1，jumpBuffer=0.12，minJumpHold=0.08，jumpCutMultiplier=0.45，riseGravity=1，fallGravity=2，airControl=0.7，airDrag=0.4，stepHeight=0.45，startWithDoubleJump=False。

---

## 5. 系统组件汇总

### 5.1 Camera / Cinemachine

`manage_camera.list_cameras` 返回：`cinemachineInstalled=false`，`cinemachineCameras=[]`，仅列出 Unity Camera `Main Camera`（FOV 80，depth 0）。**这与场景组件矛盾**：Main Camera 上有 `CinemachineBrain`，另有完整 FreeLook 三 rig。按 SerializedObject 为准。

**Main Camera** `[29332]`

| 项 | 值 |
|---|---|
| Tag | MainCamera |
| World Pos / Rot | `(0, 4, -13.5)` / `(9.24, 0, 0)` |
| ClearFlags | Skybox |
| FOV / Near / Far | 80 / 0.1 / 5000 |
| Orthographic | False |
| CullingMask | -1 |
| HDR / MSAA / OcclusionCulling | True / True / True |
| TargetTexture | null |
| AudioListener | 有（全场景唯一） |
| CinemachineBrain | Update=Smart Update；BlendUpdate=Late Update；DefaultBlend=**Ease In Out, 2s**；CustomBlends=null |

**CinemachineFreeLook** `[29274]`

| 项 | 值 |
|---|---|
| Priority | 10 |
| Follow / LookAt | Player 2 Transform |
| BindingMode | Simple Follow With World Up |
| CommonLens | True |
| Lens FOV / Near / Far / Dutch | 80 / 0.1 / 5000 / 0 |
| SplineCurvature | 0.2 |
| YAxis Value / MaxSpeed / Accel / Decel | 0.5 / 2 / 0.2 / 0.1 |
| XAxis Value / MaxSpeed / Accel / Decel | 0 / 300 / 0.1 / 0.1 |
| Orbit[0] Top | height=4.5，radius=3 |
| Orbit[1] Middle | height=2.5，radius=10 |
| Orbit[2] Bottom | height=0.4，radius=6 |

每个 Rig 的 `cm` 子物体：`CinemachinePipeline` + `CinemachineOrbitalTransposer` + `CinemachineComposer`。

### 5.2 Light / 环境

| 项 | 值 |
|---|---|
| 灯光数量 | 1（Directional Light 29366） |
| Type | Directional |
| Color | RGBA(1,1,1,1) |
| Intensity | 1.1 |
| Bounce | 1 |
| Rotation | `(45, 330, 0)` |
| Shadows | **None** |
| Lightmapping | Realtime |
| CullingMask | -1 |
| RenderSettings.sun | Directional Light |
| Ambient Mode | Skybox |
| Ambient Sky / Equator / Ground | (0.212, 0.227, 0.259) / (0.114, 0.125, 0.133) / (0.047, 0.043, 0.035) |
| Ambient Intensity | 1 |
| Fog | **False**（密度 0.01 已填但未启用） |
| Skybox | Default-Skybox（内置） |
| Lighting Settings 资产 | `Lightmapping.lightingSettings` 为 null（查询抛错） |

运行时 `PCGLabExperience` 会改黄昏光/雾，编辑器态未见。

### 5.3 UI

| 查询 | 数量 |
|---|---|
| Canvas | 0 |
| EventSystem | 0 |
| UIDocument | 0 |

无 ugui / UI Toolkit 场景对象。HUD 由运行时 `PCGLabHud` 提供；旧调试 UI 是 `PCGDebugPanel` IMGUI。

### 5.4 Physics

| 类型 | 数量 | 对象 |
|---|---|---|
| Rigidbody | 1 | Player 2 |
| Collider | 2 | Player CapsuleCollider；Start Platform/Platform BoxCollider |

无 PhysicMaterial。无 Scene 级 NavMeshAgent。Ground 仅 Platform（layer 7）。生成 chunk 后会增加碰撞体与 Trigger 检查点。

### 5.5 音频 / 粒子 / 动画 / 导航

| 系统 | 编辑器态 |
|---|---|
| AudioSource | **0** |
| AudioListener | 1（Main Camera） |
| ParticleSystem | **0** |
| Animator | 1（Player 2） |
| NavMeshAgent | **0** |
| Terrain | 0 |

### 5.6 Prefab

| 实例 | 状态 | 源 |
|---|---|---|
| Player 2 | Connected，outermost | `Assets/_Project/Prefabs/Player 2.prefab` |

其余根对象 `NotAPrefab`。Chunk Prefab 在 `Assets/_Project/PCG/Prefabs/`，仅运行时 Instantiate。

---

## 6. 依赖关系与交互分析

```
InputReader (SO)
  ├─ PlayerController  (OnEnable 订阅 Move/Jump/Dash/Attack；Start EnablePlayerActions)
  ├─ CameraManager     (Look + RMB 相机锁)
  └─ PCGNavigationAgent (SetExternal*；Heuristic 读 HumanDirection)

Player 2
  ├─ PlayerController ── Follow/LookAt ──► CinemachineFreeLook
  ├─ PCGPlayerAbilityBridge ── FindObjectOfType ──► LevelGenerator.SetTraversalCapabilities
  │                         ── Jumped/Dashed ──► PCGLabSignals
  ├─ Health ──► FloatEventChannel PlayerHealthPercentage  (本场景无监听者)
  ├─ PlatformCollisionHandler ── tag MovingPlatform 父子化
  ├─ ResetPlayer ── y<=-20 回到 originalPos
  └─ PCGNavigationAgent ── 观察/奖励 ──► PCGGameAIObservationSensor
                         ── 检查点/死亡 ──► PCGRunController 事件

PCG System
  LevelGenerator.Generate()
       Instantiates chunk prefabs under Generated Chunks
       Adds ChunkCheckpoint (static event Reached)
            └─► PCGRunController ──► Telemetry / AdaptiveDifficulty / NavigationAgent / LabExperience
  PCGDebugPanel.Awake ──► PCGLabExperience.EnsureInstalled
       └─► 运行时 HUD / 换肤 / 雾光 / SFX；订阅 GenerationFinished、Checkpoint、Respawn、TimedPlatform
```

**交互模式归类：**

| 模式 | 实例 |
|---|---|
| Inspector 直接引用 | PCG 系统内部 7 个组件互指；Agent ↔ Generator/Controller/Sensor/Camera；CameraManager ↔ FreeLook/InputReader |
| ScriptableObject 事件通道 | Health → `FloatEventChannel`（本场景无消费者） |
| C# event / 静态 event | `ChunkCheckpoint.Reached`、`PCGTimedPlatform.StateChanged`、`PCGLabSignals`、`GenerationFinished` |
| FindObjectOfType / FindGameObjectWithTag | AbilityBridge→LevelGenerator；多个 PCG 组件 Awake 后备查找；RunController 用 tag `Player` |
| GetComponent | AbilityBridge 补 PlayerController；Agent 取 BehaviorParameters / RenderTextureSensor |
| 单例 | 未见自定义单例；ML-Agents `Academy.Instance` 在 Play 时初始化 |
| 状态机 | `PlayerController` 内部 `StateMachine`（非场景组件） |

---

## 7. 潜在问题、风险与优化建议

以下均对应实测数据或已读源码。

1. **场景未保存**
   `isDirty=True`。交接或切场景会丢未保存改动。

2. **双重生系统冲突**
   `PCGRunController.fallThreshold=-12`（检查点重生）与 `ResetPlayer.fallThreshold=-20`（出生点）同时 `Update`。先触发的是 -12 的 PCG 路径；若 PCG 引用丢失，-20 会把玩家拉回 `Start` 时位置，绕过检查点。建议本场景禁用 `ResetPlayer`。

3. **Inspector 能力开关被代码覆盖**
   `PlayerController` Inspector：`allowDoubleJump=False`，`allowDash=False`；`LevelGenerator.hasDoubleJump=False`。但 `PlayerController.Awake` 强制 `allowDoubleJump=true`，`PCGPlayerAbilityBridge.Awake` 再 `SetTraversalAbilities(true, AllowDash)`。实验室默认实际是双跳开启、冲刺关闭。文档/HUD 若按 Inspector 理解会错。

4. **`PCGPlayerAbilityBridge.playerController` 未序列化**
   当前为 null，运行时 GetComponent 能工作，但丢失组件时不会在编辑器暴露。

5. **玩家 Layer 不是 Player**
   Tag=`Player`，Layer=`Default`。项目有 Layer 3=`Player`。若后续按 Layer 做检测/伤害会漏。GroundChecker 只认 Ground(7)，起点平台 layer 正确。

6. **Health 通道无消费者**
   `PlayerHealthPercentage` 已赋值，场景无 UI / EventListener。受伤无反馈（本关也几乎无战斗 UI）。

7. **观察传感器热路径分配**
   `PCGGameAIObservationSensor.CaptureStructuredObservation` 每次 `FindObjectsOfType<PCGTimedPlatform>()`，5 Hz。生成多个 timed 平台后有 GC 成本。

8. **ML-Agents 无模型 + 曾连不上 trainer**
   Behavior Model=null，BehaviorType=HeuristicOnly，`trainingMode=False`。Console 有 `Couldn't connect to trainer on port 5004 ... Will perform inference instead.` 进入 Play 且 Agent OnEnable 时会出现。无 ONNX 时训练模式不可用。

9. **RenderTextureSensor.RenderTexture 编辑器为空**
   Agent `Initialize` 才绑定 `observationSensor.GetOrCreateVisualFrame()`。未 Play 时传感器未就绪，属预期。

10. **Cinemachine MCP 探测失败**
    `list_cameras` 报未安装 Cinemachine，但组件齐全。自动化相机工具不可靠，应以层级为准。

11. **灯光无阴影 + 默认天空盒**
    `LIGHT_SHADOWS=None`，Fog 关。表现依赖 Play Mode 的 LabExperience。编辑器 Scene 观感会偏“原型”。

12. **`moveSpeed=300` 量级**
    若内部按 `rb.AddForce`/`velocity` 乘 `Time.fixedDeltaTime` 则合理；需与 `PCGPlayerReachModel.FromJumpProfile` 一致。config `syncReachFromPlayer=True`，生成可达性跟这个 300 走，改速度必须重测 chunk 筛选。

13. **`Generated Chunks` 编辑器为空**
    不要用编辑器层级判断关卡内容。验证必须进 Play Mode。

14. **Build 列表含禁用场景**
    `Assets/_Demo/Scenes/Level1.unity` enabled=false，导致工具 buildIndex 与 `Scene.buildIndex` 不一致。打包顺序以启用场景为准：Main Menu → Tutorial → Level_1 → Victory → PCG_Lab。

---

## 8. 附录

### 8.1 数据限制

- 本客户端无 `mcpforunity://scene/gameobject/{id}` resource 读取能力，组件详情用 `execute_code` + `SerializedObject` 补齐。
- Player 2 骨骼 59 个 Transform，只展开到 `root/pelvis`。
- `PlatformChunkData` 的 `requiresDoubleJump` / `compositeDifficulty` 等字段名与尝试路径不匹配，附录只列出成功读到的字段。
- 未进入 Play Mode，故未采集生成后的 16 chunk、检查点、HUD、雾效。

### 8.2 Chunk 库（`Assets/_Project/PCG/Data`）

| chunkId | category | prefab | requiredHorizontalReach | requiredVerticalReach |
|---|---|---|---|---|
| basic_01 | Basic | `PCG/Prefabs/basic_01.prefab` | 0 | 0 |
| rising_01 | Basic | rising_01 | 3.125 | 1.2 |
| turn_left_01 | Exploration | turn_left_01 | 0 | 0 |
| turn_right_01 | Exploration | turn_right_01 | 0 | 0 |
| offset_left_01 | Exploration | offset_left_01 | 3.125 | 0.4 |
| climb_01 | Basic | climb_01 | 2.5 | 1 |
| descend_01 | Recovery | descend_01 | 2.5 | 0 |
| climb_turn_left_01 | Exploration | climb_turn_left_01 | 3.125 | 1 |
| moving_01 | Moving | moving_01 | 3.125 | 0.4 |
| timed_01 | Timed | timed_01 | 2.8125 | 0 |
| double_jump_01 | Ability Gate | double_jump_01 | 6.25 | 2.2 |
| dash_gap_01 | Ability Gate | dash_gap_01 | 10 | 0 |
| combat_01 | Combat | combat_01 | 0 | 0 |
| recovery_01 | Recovery | recovery_01 | 0 | 0 |

默认 `hasDash=False` 时 `dash_gap_01` 应被选择器排除；`hasDoubleJump` Inspector 为 False，但运行时玩家双跳被代码打开，生成侧是否同步取决于 Debug Panel / Bridge 时序。

### 8.3 项目 Tags / Layers（编辑器完整列表）

**Layers：** 0 Default，1 TransparentFX，2 Ignore Raycast，3 Player，4 Water，5 UI，6 PostProcessing，7 Ground。

**Tags：** Untagged, Respawn, Finish, EditorOnly, MainCamera, Player, GameController, FxTemporaire, Bullet, MovingPlatform, Enemy, DoubleJump, Ground, Waste, TimeedPlatform, stealthPlayer, Time, Speed, HUD, Status, EventSystem, Results, Button, Pause, Dash。

本场景实际用到的 Tag：`Player`、`MainCamera`、`Untagged`。Layer 实际用到：Default、Ground。

### 8.4 完整组件类型清单（按对象）

| 路径 | 组件 |
|---|---|
| Player 2 | Transform, Animator, CapsuleCollider, Rigidbody, GroundChecker, Health, PlayerController, PlatformCollisionHandler, ResetPlayer, PCGPlayerAbilityBridge, BehaviorParameters, RenderTextureSensorComponent, PCGNavigationAgent, DecisionRequester |
| Player 2/Body05, Cloak02 | Transform, SkinnedMeshRenderer |
| Player 2/Body10, Cloak03 | 同上（未激活） |
| Player 2/root… | Transform（骨骼） |
| Directional Light | Transform, Light |
| Start Platform | Transform |
| Start Platform/Platform | Transform, MeshFilter, BoxCollider, MeshRenderer |
| Start Platform/Start Anchor | Transform |
| Start Platform/Player Spawn | Transform |
| CameraSystem | Transform, CameraManager |
| CameraSystem/Main Camera | Transform, Camera, AudioListener, CinemachineBrain |
| CameraSystem/FreeLook Camera | Transform, CinemachineFreeLook |
| …/TopRig, MiddleRig, BottomRig | Transform, CinemachineVirtualCamera |
| …/cm | Transform, CinemachinePipeline, CinemachineOrbitalTransposer, CinemachineComposer |
| Generated Chunks | Transform |
| PCG System | Transform, LevelGenerator, PCGRunController, PCGRunTelemetry, PCGAdaptiveDifficultyDirector, PCGGameAIObservationSensor, PCGMultimodalDatasetRecorder, PCGDebugPanel |

### 8.5 Console 日志摘要

**当前（read_console errors+warnings）：**

| 类型 | 内容 |
|---|---|
| Warning | MCP WebSocket Receive loop：对端未完成 close handshake 就关闭 |
| Warning | MCP WebSocket Connection closed（同上） |
| Warning | `manage_script.read is deprecated; prefer resources/read` |

**稍早一次 all 类型读取（4 条 Log，无 Error）：**

| 类型 | 内容 |
|---|---|
| Log | ML-Agents：`Couldn't connect to trainer on port 5004 using API version 1.5.0. Will perform inference instead.`（`Academy.cs:459`） |
| Log | MCP-FOR-UNITY：Starting local HTTP server |
| Log | MCP-FOR-UNITY：Server ready on `http://127.0.0.1:8080` |
| Log | MCP-FOR-UNITY：Session connected |

无场景脚本编译错误，无 missing script 报错。`validate` 确认场景干净。

### 8.6 建议的 Play Mode 验证（本文未执行）

编辑器未 Play，以下未在本次检查中验证：seed `82431` 的 16 chunk 布局、检查点触发、双重生竞争、Lab HUD/黄昏雾、Agent Heuristic 与人类输入是否抢方向、`FindObjectOfType` 在生成后的耗时。
