# AGENTS.md

This file provides guidance to Codex (Codex.ai/code) when working with code in this repository.

## Project Overview

Unity 2022.3 LTS 3D platformer using URP 14.0.8. Features modular state machines, ScriptableObject event channels, factory-pattern spawning, and Unity Input System.

## Common Commands

Build WebGL and package:
```bash
# Build WebGL (outputs to WebBuild/)
unity -batchmode -quit -projectPath . -buildTarget WebGL -executeMethod <buildmethod>
```

No test suite or CI pipeline is configured. Opening scenes in the Unity Editor and pressing Play is the primary test method.

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
- `PlayerO2.cs` has debug code: pressing W deals damage (likely for testing health bar).
- Two parallel collectible systems exist:
  - New system: `Collectible` + `CollectibleSpawnManager` (event-driven, `IntEventChannel`)
  - Old system: `NucliarWaste` + `Collector` (direct `FindObjectOfType`, `AddPoint()`)
- `NucliarWaste` prefab has been modified (in git diff).
- `com.bezi.sidekick` package is in `Packages/` (locally embedded).
- unity-mcp (`com.coplaydev.unity-mcp`) is installed for editor integration.
- WebGL builds output to `WebBuild/` (currently untracked/uncommitted except `WebBuild.zip`).
