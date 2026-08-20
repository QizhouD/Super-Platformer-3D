# Grok + Unity CLI 项目交付文档

## 交付状态

- 交付日期：2026-08-19
- 仓库：`https://github.com/QizhouD/Super-Platformer-3D.git`
- 当前分支：`main`，上游为 `origin/main`
- 本文档生成前的本地基线提交：`4fa1195b chore: track Unity ML-Agents folder metadata`
- 上一个功能提交：`6ac11e42 feat(ml-agents): add trainable PCG navigation agent`
- 未执行远端推送。接手后先运行 `git log -3 --oneline` 获取包含本文档的最终 HEAD。
- 本文档生成前，所有项目修改均已提交，工作区为空。

`4fa1195b` 只补齐 `Assets/ML-Agents.meta`、`Assets/ML-Agents/Timers.meta`，并将本机专用的 `.claude/settings.local.json` 加入 `.gitignore`。没有尚未提交的玩法代码改动。

## 接手时先读

按以下顺序建立上下文：

1. `docs/agents.md`：仓库架构、目录、场景和约定。
2. `docs/pcg.md`：当前 PCG Lab 的玩法、生成规则、遥测、数据集和 ML-Agents 契约。
3. `docs/training.md`：Python 训练环境与启动方式。
4. `docs/changelog.md`：最近 PCG、Game AI 和 ML-Agents 功能的完整验证记录。
5. `.claude/skills/unity-style-guide/SKILL.md`：Unity 修改与验证规范。Grok 不一定自动加载 Claude Skill，应将其作为普通 Markdown 手动阅读并遵循。

## 项目基线

- Unity：`2022.3.2f1`
- 渲染：URP `14.0.8`
- Input System：`1.6.1`
- Cinemachine：`2.9.7`
- AI Navigation：`1.1.7`
- Unity ML-Agents：`2.0.1`
- Unity Test Framework：解析后的锁定版本为 `1.1.33`
- Python Trainer：`mlagents==0.30.0`，要求 Python 3.9；依赖版本记录在 `Training/requirements-mlagents.txt`
- 核心玩法命名空间：`Platformer`
- PCG 命名空间与程序集：`Platformer.PCG`
- PCG EditMode 测试程序集：`Platformer.PCG.Tests`

当前机器的 PATH 和常见 Unity Hub 安装目录中没有检测到 `Unity.exe`，也没有正在运行的 Unity Editor。接手环境需要先定位或安装精确版本的 Editor；不要默认使用更高版本升级项目。

## 当前可运行成果

### 原有游戏

项目是第三人称 3D 平台跳跃游戏，使用：

- 玩家与敌人有限状态机；
- ScriptableObject 事件通道；
- Input System 驱动的 `InputReader`；
- 工厂与生成策略；
- Cinemachine FreeLook；
- NavMesh 敌人行为。

主流程场景从 `Assets/_Project/Scenes/Level_Tutorial.unity` 开始，然后是 `Level_1.unity`。实际构建列表还包含 Main Menu、VictoryScene 和 PCG_Lab。

### PCG Lab

入口场景：`Assets/_Project/Scenes/PCG_Lab.unity`

当前默认契约：

- 默认种子：`82431`
- 默认生成：16 个 chunk、16 个 checkpoint
- chunk 库：14 个原型，包含直行、转向、偏移、爬升、下降、移动、计时、能力门、战斗和恢复类型
- 空间限制：最多连续 3 个平坦 chunk、3 个直行 chunk
- 相对高度范围：`-4m..+8m`
- 生成保持确定性、能力过滤、可达性过滤、类别连续限制和碰撞检查
- `Platformer > PCG > Create First Batch` 可重建生成资源和实验场景

不要在生成的 prefab、data asset 或 `PCG_Lab.unity` 有需要保留的手工修改时运行重建命令；`PCGProjectBootstrap` 会覆盖这些资源。

### Game AI 与训练

`PCGNavigationAgent` 通过现有 Player 2 状态机和 `InputReader` 的外部控制通道驱动角色，不维护第二套移动逻辑。

策略契约：

- 20 个归一化结构化观测；
- 一个 84x84 RGB `RenderTextureSensorComponent`；
- 2 个连续移动轴；
- 2 个二值离散分支：Jump、Dash；
- checkpoint、通关、死亡、时间和靠近目标奖励；
- `PCGNavigation` Behavior Name；
- Decision Period 为 5；
- Debug Panel 可切换 `HeuristicOnly` 与训练用 `Default`。

附加能力：

- `PCGAdaptiveDifficultyDirector` 根据 checkpoint 时间与重生估计技能，动态调整平台并影响下一次生成难度；
- `PCGRunTelemetry` 记录生成、checkpoint、重生和动态平台事件；
- `PCGMultimodalDatasetRecorder` 输出结构化 JSONL、84x84 PNG 与 episode summary；
- PPO 配置位于 `Training/pcg_navigation_ppo.yaml`。

## 关键代码索引

| 位置 | 责任 |
|---|---|
| `Assets/_Project/Scripts/PCG/Runtime/LevelGenerator.cs` | 确定性关卡生成与 manifest |
| `Assets/_Project/Scripts/PCG/Runtime/ChunkSelector.cs` | 难度、能力、可达性和空间语法筛选 |
| `Assets/_Project/Scripts/PCG/Runtime/PlatformChunkData.cs` | chunk 元数据契约 |
| `Assets/_Project/Scripts/PCG/Runtime/PCGRunController.cs` | checkpoint、重生、run reset |
| `Assets/_Project/Scripts/PCG/Runtime/PCGGameAIObservationSensor.cs` | 20 值观测和 84x84 视觉帧 |
| `Assets/_Project/Scripts/PCG/Runtime/PCGAdaptiveDifficultyDirector.cs` | 在线技能估计和难度偏置 |
| `Assets/_Project/Scripts/PCG/Runtime/PCGRunTelemetry.cs` | 有界事件遥测 |
| `Assets/_Project/Scripts/PCG/Runtime/PCGMultimodalDatasetRecorder.cs` | 多模态 episode 数据集 |
| `Assets/_Project/Scripts/PCGNavigationAgent.cs` | ML-Agents Agent、动作和奖励 |
| `Assets/_Project/Scripts/Input/InputReader.cs` | 人类输入与外部策略输入复用 |
| `Assets/_Project/Scripts/Editor/PCGProjectBootstrap.cs` | 生成资源和 PCG_Lab 的 Editor 入口 |
| `Assets/_Project/Tests/PCG/Editor/PCGCoreTests.cs` | 26 个 PCG EditMode 测试 |

## Unity CLI 工作流

以下命令使用 PowerShell。先将 `$UnityExe` 改为实际的 `2022.3.2f1` 路径。Unity 同一项目不能同时被普通 Editor 和 batchmode 实例打开，因此执行前关闭该项目的 Editor。

```powershell
$ProjectPath = (Resolve-Path 'D:\unity-game\3D-Platformer').Path
$UnityExe = 'C:\Program Files\Unity\Hub\Editor\2022.3.2f1\Editor\Unity.exe'
$LogPath = Join-Path $ProjectPath 'Logs'
New-Item -ItemType Directory -Force $LogPath | Out-Null

if (-not (Test-Path $UnityExe)) {
    throw "Unity 2022.3.2f1 not found: $UnityExe"
}
```

### 1. 导入并检查编译

```powershell
& $UnityExe `
  -batchmode `
  -quit `
  -accept-apiupdate `
  -projectPath $ProjectPath `
  -logFile (Join-Path $LogPath 'grok-import.log')

if ($LASTEXITCODE -ne 0) {
    throw "Unity import/compile failed with exit code $LASTEXITCODE"
}
```

随后检查 `Logs/grok-import.log` 中是否出现编译错误。不要使用 `-ignorecompilererrors`。

### 2. 运行 PCG EditMode 测试

```powershell
$TestResults = Join-Path $LogPath 'pcg-editmode-results.xml'

& $UnityExe `
  -runTests `
  -batchmode `
  -projectPath $ProjectPath `
  -testPlatform EditMode `
  -assemblyNames 'Platformer.PCG.Tests' `
  -testResults $TestResults `
  -logFile (Join-Path $LogPath 'pcg-editmode.log')

if (-not (Test-Path $TestResults)) {
    throw 'Unity did not write the NUnit test result XML.'
}
```

必须读取 XML 和日志确认总数、失败数与错误堆栈，不要只依赖进程退出码。最后一次记录在 `docs/changelog.md` 的结果是 26 passed、0 failed，但本次交付环境因为找不到 Unity executable，没有重新运行测试。

### 3. 有意重建 PCG 资源

仅在确认允许覆盖生成资源后执行：

```powershell
& $UnityExe `
  -batchmode `
  -accept-apiupdate `
  -projectPath $ProjectPath `
  -executeMethod PCGProjectBootstrap.CreateFirstBatchBatchMode `
  -logFile (Join-Path $LogPath 'pcg-bootstrap.log')

if ($LASTEXITCODE -ne 0) {
    throw "PCG bootstrap failed with exit code $LASTEXITCODE"
}
```

该静态方法自己调用 `EditorApplication.Exit(0)`。运行后必须检查 Git diff，确认场景、prefab、data asset 和 Build Settings 的变化都是预期的。

### 4. 启动 ML-Agents 训练

项目本地 `.venv-mlagents` 和 `Training/results/` 都被 Git 忽略。环境存在时：

```powershell
.\.venv-mlagents\Scripts\Activate.ps1
mlagents-learn Training\pcg_navigation_ppo.yaml --run-id=pcg-navigation-v1
```

Trainer 开始监听后，在 Unity Editor 中打开 `PCG_Lab.unity`、进入 Play Mode，并在左上角 Debug Panel 启用 `ML-Agents Training Mode`。当前流程依赖交互式 Editor，不是完全 headless 的训练 Player 流程。

## 验证边界与已知缺口

- 本次只完成 Git 收口和交付文档，没有修改或重新验证玩法代码。
- 当前环境未找到 Unity CLI，因此没有新的编译、EditMode、Play Mode 或 WebGL build 结果。
- 仓库没有 CI 配置。
- 仓库中未找到调用 `BuildPipeline.BuildPlayer` 的 Editor 构建方法；`docs/agents.md` 中的 WebGL CLI 命令仍含 `<buildmethod>` 占位符。要稳定执行 WebGL CLI 构建，需先实现一个 Editor-only 静态构建入口，并让失败通过异常或非零 `EditorApplication.Exit` 返回。
- 当前自动化测试只有 PCG EditMode 程序集；场景内输入、视觉 sensor、dataset 文件 I/O、完整 episode 和训练通讯仍主要依赖 Play Mode smoke test。
- `Application.persistentDataPath/PCGDatasets/` 是运行时数据输出，不应提交到仓库。
- `.claude/settings.local.json` 是机器专用权限文件，已忽略，不要强制添加。

## 建议的继续开发顺序

1. 在精确 Unity 版本上执行导入和 26 个 EditMode 测试，保存 XML 与日志结果。
2. 打开 `PCG_Lab` 做最小 Play Mode smoke test：默认种子生成 16 个 chunk、角色移动/跳跃/冲刺、checkpoint/重生、训练模式切换和 Console 无新增错误。
3. 如果主要目标是 Unity CLI 自动化，先增加可测试的 WebGL batch build 入口，再将 import、EditMode tests 和 build 固化为脚本或 CI。
4. 如果主要目标是 Game AI，先训练一个可复现 PPO baseline，记录 run-id、种子集合、通关率、平均 checkpoint、episode return 和重生次数，再调整奖励或网络。
5. 为完整 episode、视觉 sensor 和 dataset recorder 增加 PlayMode 覆盖，避免只靠手工烟雾测试。

## 修改时必须保持的契约

- 不绕过 `InputReader` 和 Player 2 现有状态机另写一套 ML 移动控制。
- 不改变 20 值观测的顺序或缩放而不同时更新 encoder、agent、测试和训练版本说明。
- 不破坏同一 seed + config + ability profile 的确定性结果。
- 新 chunk 必须提供 entry/exit socket、能力要求、可达性元数据、难度元数据和对应 `.meta`。
- 事件订阅必须在组件禁用时对称退订；外部输入结束时必须释放 Jump/Dash 和方向状态。
- 场景和 prefab 改动优先由 Unity Editor/CLI 生成，避免手改大段 Unity YAML。
- 不提交 `Library/`、`Temp/`、`Logs/`、`WebBuild/`、`.venv-mlagents/`、训练结果或本地智能体权限文件。

## 可直接交给 Grok 的启动提示

```text
你正在接手 D:\unity-game\3D-Platformer。先阅读 docs/agents.md、docs/grok-handoff.md、
docs/pcg.md、docs/training.md、docs/changelog.md，以及
.claude/skills/unity-style-guide/SKILL.md。不要假设 Unity CLI、构建入口或测试已经可用；
先定位 Unity 2022.3.2f1，运行 import/compile 和 Platformer.PCG.Tests，并报告 XML 与日志结果。
保持 PCG 的确定性、20 值观测契约、Player 2 InputReader 外部控制通道和现有状态机。
修改前检查 git status，修改后给出静态检查、EditMode、Play Mode 和 build 的真实验证边界。
```
