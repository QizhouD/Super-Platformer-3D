# Project architecture

This file provides guidance to coding agents working in this repository.

## Project Overview

Unity 2022.3 LTS 3D platformer using URP 14.0.8. Features modular state machines, ScriptableObject event channels, factory-pattern spawning, and Unity Input System.

## Common Commands

Unity is pinned to 2022.3.2f1; do not upgrade the editor. Close the Editor before
running batchmode, because Unity cannot open the same project twice.

Run the PCG EditMode tests (PowerShell, from the repo root):
```powershell
$UnityExe = 'D:\unity eiditor\Unity 2022.3.2f1\Editor\Unity.exe'  # Jim's desktop; adjust per machine
$Project = (Resolve-Path .).Path  # Start-Process does not quote arguments; keep this path free of spaces
$unityArgs = @(
  '-runTests', '-batchmode', '-projectPath', $Project,
  '-testPlatform', 'EditMode', '-assemblyNames', 'Platformer.PCG.Tests',
  '-testResults', "$Project\Logs\pcg-editmode-results.xml",
  '-logFile', "$Project\Logs\pcg-editmode.log")
$p = Start-Process -FilePath $UnityExe -ArgumentList $unityArgs -Wait -PassThru
$p.ExitCode
```

Use `Start-Process -Wait`. Calling `& $UnityExe` returns immediately because Unity.exe
is a GUI-subsystem binary, so `$LASTEXITCODE` says nothing about the run. Read the
XML for pass/fail counts.

The project's automated tests are the `Platformer.PCG.Tests` EditMode assembly in
`Assets/_Project/Tests/PCG/Editor/` (40 tests). Keep `-assemblyNames`: the vendored
`NavMeshComponents-master` folder ships 26 more EditMode tests that cannot find their
scenes in this layout and always fail. There is no CI and no CLI build entry point
(nothing calls `BuildPipeline.BuildPlayer`), so WebGL builds are made from
File > Build Settings; output goes to `WebBuild/` and stays untracked. Play Mode in
the Editor remains the smoke test: `Level_Tutorial` for the main game, `PCG_Lab`
for the lab.

## Architecture

### Namespaces
- `Platformer` — all gameplay code (player, enemies, systems)
- `Utilities` — `Timer`, `CountdownTimer`, `StopwatchTimer`
- `Platformer._Project.Scripts.Utils` — `Helpers` (e.g. `QuitGame()`)

### State Machine (`Assets/_Project/Scripts/StateMachine/`)

The core behavioral framework used by both player and enemies.

| File | Role |
|------|------|
| `StateMachine.cs` | Runtime FSM — dictionary of `StateNode` by Type, HashSet of any-transitions. `Update()`/`FixedUpdate()` delegate to current state |
| `IState.cs` | Interface: `OnEnter()`, `Update()`, `FixedUpdate()`, `OnExit()` |
| `IPredicate.cs` | Interface: `bool Evaluate()` |
| `ITransition.cs` | Pairs an `IState` to-go with an `IPredicate` |
| `Transition.cs` | Concrete `ITransition` |
| `FuncPredicate.cs` | `IPredicate` wrapping a `Func<bool>` for inline lambda conditions |

Usage pattern:
```csharp
stateMachine = new StateMachine();
var idle = new SomeState(...);
var run = new OtherState(...);
At(idle, run, new FuncPredicate(() => someCondition));
Any(idle, new FuncPredicate(() => emergencyCondition));
stateMachine.SetState(idle);
// Then call stateMachine.Update() / FixedUpdate() each frame
```

**Player States** (`StateMachine/PlayerStates/`): `LocomotionState`, `JumpState`, `DashState`, `AttackState` — each extends `BaseState(PlayerController, Animator)`.

**Enemy States** (`StateMachine/EnemyStates/`): `EnemyWanderState`, `EnemyChaseState`, `EnemyAttackState` — each extends `EnemyBaseState(Enemy, Animator, NavMeshAgent, ...)`.

### Event System (`Assets/_Project/Scripts/EventSystem/`)

ScriptableObject-based observer pattern for decoupled communication:

- `EventChannel<T>` — abstract generic base with `Register`/`Deregister`/`Invoke(T)`
- `EventListener<T>` — MonoBehaviour that binds UnityEvent responses to channels
- Concrete channels: `FloatEventChannel`, `IntEventChannel`, `EventChannel` (parameterless, uses `Empty` struct)
- **Creation**: Right-click in Project → Create → Events → EventChannel

`Health` publishes `currentHealth / maxHealth` as float through `FloatEventChannel` when damaged. `Collectible` publishes score through `IntEventChannel` on pickup.

### Spawn System (`Assets/_Project/Scripts/SpawnSystem/`)

Factory + Strategy pattern:
- `EntityData` — ScriptableObject holding prefab reference (extend for specific types)
- `EntityFactory<T> where T : Entity` — instantiates from random `EntityData`, returns typed `T`
- `ISpawnPointStrategy` → `LinearSpawnPointStragegy` (note: typo in class name) / `RandomSpawnPointStrategy`
- `EntitySpawnManager` — abstract base, holds spawn points and strategy, exposes `abstract void Spawn()`
- `CollectibleSpawnManager` / `EntitySpawner` — concrete spawn managers

### Entity Hierarchy

`Entity` (abstract MonoBehaviour) → `Enemy`, `Collectible`

`Enemy` requires `NavMeshAgent` and `PlayerDetector`. Uses its own state machine (wander→chase→attack). `PlayerDetector` uses `ConeDetectionStrategy` (cone-of-sight with angles/radii) and exposes `CanDetectPlayer()` / `CanAttackPlayer()`.

### Input (`Assets/_Project/Scripts/Input/InputReader.cs`)

ScriptableObject asset implementing `IPlayerActions` from the generated `PlayerInputActions` class. Exposes C# events: `Move`, `Look`, `Jump`, `Dash`, `Attack`, `Pause`. `Direction` property exposes movement as `Vector3`. Components subscribe to events in `OnEnable`/unsubscribe in `OnDisable`.

Create via: Create → Platformer → InputReader

### Timer Utilities (`Assets/_Project/Scripts/Utils/Timer.cs`)

`CountdownTimer(float duration)` — `Tick(deltaTime)` counts down, fires `OnTimerStop` when reaching zero. Used for jump cooldown, dash duration, attack cooldown, enemy detection cooldown.

`StopwatchTimer()` — counts up from zero.

### Camera

`CameraManager` drives `CinemachineFreeLook` based on `InputReader.Look` events. RMB-hold locks cursor and enables mouse camera control. `PlayerController.Awake()` wires the FreeLookVCam to Follow/LookAt the player.

### Platforms

- `HorizontalPlatform` / `VerticalPlatform` — move along axes
- `TimedPlatform` — appears/disappears on timer
- `VanishingPlatform` — disappears on contact
- `PlatformMover` + `PlatformCollisionHandler` — newer platform system
- `MoveAndSpin` — rotating hazards

### KBCore.Refs Pattern

`ValidatedMonoBehaviour` base class with `[Self]`, `[Child]`, `[Anywhere]` attributes. Call `this.ValidateRefs()` in `OnValidate()` to auto-populate references at edit time. Used by `PlayerController`, `Enemy`, `CameraManager`.

## Key Conventions

- All gameplay scripts under `Assets/_Project/Scripts/`
- Scenes under `Assets/_Project/Scenes/` — **start with Level_Tutorial**, then Level_1
- Scene list in build settings: `EditorBuildSettings.asset` — any new scene must be added there
- Player tagged `"Player"` (referenced by `PlayerDetector` and `Collectible` via `FindGameObjectWithTag`)
- Enemy tagged `"Enemy"` (checked in `PlayerController.Attack()`)
- Existing scenes not in build settings: `Demo.unity`, `Gameplay.unity`, `Sandbox.unity`, `Level_Tutorial 1.unity`, `Level001.unity`

## Important Notes

- `Player.cs` is an unused legacy script (uses old Input system). The real player controller is `PlayerController.cs`.
- `PlayerControl.cs` exists alongside `PlayerController.cs` — verify which is attached in each scene.
- `PlayerNuke.cs` is an empty stub.
- `PlayerO2.cs` has debug code: pressing W deals damage (likely for testing health bar). It is only attached in `Level_Tutorial 1.unity`, which is not in the build.
- Two parallel collectible systems exist:
  - New system: `Collectible` + `CollectibleSpawnManager` (event-driven, `IntEventChannel`)
  - Old system: `NucliarWaste` + `Collector` (direct `FindObjectOfType`, `AddPoint()`)
- Double Jump and Dash are off on the `Player 2` prefab. `Level_Tutorial` unlocks them through the `DoubleJump` / `Dash` pickups, `Level_1` enables both through scene overrides, and `PCG_Lab` toggles them from the HUD (the generator uses the new ability profile on the next Generate). Do not force abilities in `PlayerController.Awake()` or `PCGPlayerAbilityBridge`.
- In `PCG_Lab`, `PCGRunController` owns falling and checkpoint respawn. The `ResetPlayer` component on the `Player 2` prefab is disabled in that scene through a prefab override; it stays active in the main levels.
- `com.bezi.sidekick` package is in `Packages/` (locally embedded).
- unity-mcp (`com.coplaydev.unity-mcp`) is installed for editor integration.
- WebGL builds output to `WebBuild/` and must stay untracked (see `.gitignore`).
