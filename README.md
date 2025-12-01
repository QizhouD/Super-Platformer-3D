# 3D Platformer Game

A feature-rich 3D platformer developed with Unity 2022.3, featuring modular systems architecture, state machine-driven AI, and modern input handling for an immersive gameplay experience.

## 📋 Table of Contents

- Features

- Getting Started

- How to Play

- Core Systems

- Project Structure

- Technical Details

- Dependencies

- Documentation

- Acknowledgments

## ✨ Features

### Player Mechanics

- Smooth Character Movement: Camera-relative movement with dynamic rotation and acceleration

- Variable Height Jumping: Hold jump for higher arcs, release for precision

- Unlockable Abilities: Double Jump and Dash for advanced traversal

- Melee Combat: Sphere-cast attack system with damage detection

- Responsive Controls: Supports both keyboard/mouse and gamepad

### Camera System

- Cinemachine Free Look: Smooth 360° camera with orbit controls

- Contextual Camera Lock: Automatic cursor management for mouse/gamepad switching

- Dynamic Follow: Maintains optimal view of player and environment

### AI & Enemies

- State Machine Behavior: Wander, Chase, and Attack states with conditional transitions

- Vision-Based Detection: Cone-of-sight system with blind spots and detection ranges

- NavMesh Pathfinding: Intelligent obstacle avoidance and path calculation

- Balanced Combat: Attack cooldowns and proximity-based targeting

### Level Design

- Interactive Platforms: Timed, vanishing, and moving platforms

- Collectible System: Ability pickups with visual/audio feedback

- Progressive Difficulty: Tutorial level → challenging main levels

### Architecture

- Modular State Machine: Reusable for player/enemy behaviors

- Event-Driven Design: ScriptableObject-based event channels for decoupled communication

- Factory Pattern: Flexible entity/collectible spawning

- Strategy Pattern: Swappable detection and spawning logic

## 🚀 Getting Started

### Prerequisites

- Unity 2022.3 LTS or later

- Git (for repository cloning)

### Installation

- Clone the repository:

```bash

git clone https://github.com/QizhouD/Super-Platformer-3D.git
```

- Open the project in Unity Hub

- Load the tutorial level: Assets/_Project/Scenes/Level_Tutorial.unity

- Press Play to start the game

## 🎮 How to Play

### Controls

|Input|Keyboard/Mouse|Gamepad|
|---|---|---|
|Movement|WASD|Left Stick|
|Jump|Space (hold for height)|A Button|
|Dash|Shift (unlocked)|B Button|
|Attack|Left Mouse Button|X Button|
|Camera Rotation|Right Mouse Button (hold)|Right Stick|
|Pause Menu|ESC|Start Button|
### Objective

- Navigate platforming challenges and collect ability pickups

- Defeat or evade enemies using combat and traversal skills

- Reach the end of each level to progress

## 🏗️ Core Systems

### 1. State Machine System

A flexible framework for managing character behaviors with:

- IState interface for consistent state implementation

- Conditional transitions with predicate logic

- Support for Update/FixedUpdate lifecycle methods

- Any-state transitions for emergency behaviors (e.g., taking damage)

### 2. Player Controller

Third-person controller with:

- Camera-relative movement calculations

- Physics-based jumping with gravity scaling

- Ability cooldown management

- Attack hit detection via sphere casting

### 3. Enemy AI

State-driven behavior system:

- Wander: Random patrol within defined radius

- Chase: Pursue player when detected

- Attack: Melee strikes when in range

### 4. Spawn System

Factory-pattern spawning with:

- EntityFactory for creating entities from ScriptableObject data

- SpawnPointStrategy (Linear/Random) for spawn position selection

- SpawnManager for coordinating wave-based spawning

### 5. Input System

Event-driven input handling using Unity’s Input System:

- ScriptableObject-based input readers

- Device-agnostic control mapping

- Easy rebinding support

- Decoupled from gameplay logic

## 📁 Project Structure

```plaintext

Assets/  
├── _Project/                      # Core project files  
│   ├── Scenes/                    # Game levels & menus  
│   ├── Scripts/                   # C# scripts  
│   │   ├── StateMachine/          # State machine framework  
│   │   ├── SpawnSystem/           # Entity spawning  
│   │   ├── Input/                 # Input handling  
│   │   ├── UI/                    # UI controllers  
│   │   ├── Utils/                 # Helpers & utilities  
│   │   └── Core/                  # Player/Enemy/Camera logic  
│   ├── Prefabs/                   # Reusable GameObjects  
│   ├── Materials/                 # Shaders & materials  
│   ├── Models/                    # 3D assets  
│   ├── Animation/                 # Animator controllers & clips  
│   └── ScriptableObjects/         # Data assets (events, settings)  
├── Plugins/                       # Third-party tools  
└── TextMesh Pro/                  # UI text assets
```

## 🔧 Technical Details

### Unity Configuration

- Version: Unity 2022.3 LTS

- Render Pipeline: Universal Render Pipeline (URP) 14.0.8

- Physics: 3D Rigidbody physics with NavMesh pathfinding

- Animation: Animator State Machines with Animation Events

### Design Patterns

- State Pattern: Player/enemy behavior management

- Factory Pattern: Entity spawning

- Observer Pattern: Event-driven communication

- Strategy Pattern: Detection/spawning logic

- Object Pool Pattern: (Planned) For performance optimization

### Coding Standards

- Self-documenting naming conventions

- XML comments for public APIs

- Single-responsibility principle

- Separation of concerns (data vs. logic)

## 📦 Dependencies

### Unity Packages

|Package|Version|Purpose|
|---|---|---|
|Input System|1.6.1|Cross-device input|
|Cinemachine|2.9.7|Camera control|
|AI Navigation|1.1.7|NavMesh pathfinding|
|Universal RP|14.0.8|Rendering|
|TextMesh Pro|3.0.6|UI text|
|ProBuilder|5.0.7|Level design|
### External Tools

- DOTween: Animation tweening (Demigiant)

- Scene Ref Attribute: Scene reference validation (GitHub)

### Asset Store Assets

- Character/Enemy Models: RPG Tiny Hero Duo, RPG Monster DUO PBR Polyart

- Environment: Low Poly Ultimate Pack

- Audio: Casual Game Sounds

## 📚 Documentation

### System Guides

- Player Controller: Movement, abilities, and combat

- State Machine: Architecture and state creation

- Enemy AI: Behavior states and detection logic

- Spawn System: Factory pattern implementation

### Code Examples

#### Creating a New Player State

```csharp

public class DashState : BaseState {  
    public DashState(PlayerController player, Animator animator) : base(player, animator) {}  

    public override void OnEnter() {  
        animator.CrossFade("Dash", 0.1f);  
        player.ApplyDashForce();  
    }  

    public override void FixedUpdate() {  
        // Dash logic here  
    }  

    public override void OnExit() {  
        player.ResetDashState();  
    }  
}
```

#### Adding an Event Listener

```csharp

[SerializeField] private GameEvent onPlayerDash;  

private void OnEnable() {  
    onPlayerDash.AddListener(HandleDash);  
}  

private void HandleDash() {  
    // React to dash event  
}
```

## 🙏 Acknowledgments

### Learning Resources

- Unity Official Documentation
- Cinemachine & Input System Tutorials
- GameDev.tv Unity Courses
- Youtube Channel @git-amend

### Asset Credits

- RPG Tiny Hero Duo
- RPG Monster DUO PBR Polyart
- Low Poly Ultimate Pack

### Tools

- DOTween
- KBCore.Refs
