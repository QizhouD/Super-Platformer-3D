3D Platformer Game
A feature-rich 3D platformer built with Unity 2022.3, showcasing modular systems architecture, state machine-based AI, and modern input handling.


📋 Table of Contents
Features
Getting Started
Prerequisites
Installation
How to Play
Core Systems
Project Structure
Technical Details
Dependencies
Documentation
Contributing
License
✨ Features
Player Mechanics
Smooth Character Movement - Camera-relative movement with dynamic rotation
Variable Height Jumping - Hold to jump higher, release for precise control
Double Jump Ability - Unlockable mid-air jump for advanced platforming
Dash Ability - Burst of speed for crossing large gaps
Melee Combat - Sphere-cast attack system with damage dealing
Camera System
Cinemachine Free Look Camera - Smooth 360° camera control
Mouse & Gamepad Support - RMB activation for mouse control
Smart Cursor Management - Automatic locking/unlocking
AI & Enemies
State Machine AI - Wander, Chase, and Attack behaviors
Cone Detection System - Vision-based player detection with blind spots
NavMesh Pathfinding - Intelligent navigation around obstacles
Attack Cooldown System - Balanced combat timing
Level Design
Timed Platforms - Platforms that toggle visibility at intervals
Vanishing Platforms - One-way platforms that disappear after stepping on them
Moving Platforms - Horizontal and vertical moving platforms with wait times
Collectible System - Ability pickups with visual feedback
Architecture
Modular State Machine - Reusable for both player and enemy behaviors
Event-Driven Design - ScriptableObject-based event channels
Factory Pattern Spawning - Flexible entity and collectible spawning
Strategy Pattern Detection - Swappable detection algorithms
🚀 Getting Started
Prerequisites
Unity 2022.3.x or later
Git (for cloning the repository)

Installation
Clone the repository
git clone https://github.com/QizhouD/Super-Platformer-3D
Open in Unity Hub

Navigate to Assets/_Project/Scenes/Level_Tutorial.unity
Press Play to start the game
🎮 How to Play
Controls
Keyboard & Mouse
WASD - Move character
Space - Jump (hold for higher jump)
Shift - Dash (when unlocked)
Left Mouse Button - Attack
Right Mouse Button (Hold) - Rotate camera
ESC - Pause menu
Gamepad
Left Stick - Move character
Right Stick - Rotate camera
A Button - Jump
B Button - Dash (when unlocked)
X Button - Attack
Start - Pause menu
Objective
Navigate through platforming challenges
Collect ability pickups to unlock Double Jump and Dash
Defeat or avoid enemies
Reach the end of each level
🏗️ Core Systems
1. State Machine System
A flexible, reusable state machine supporting both player and enemy behaviors.

Features:
✓ IState interface for clean state implementation
✓ Conditional transitions with predicates
✓ Any-state transitions
✓ Update and FixedUpdate support
2. Player Controller
Camera-relative third-person controller with abilities.

Features:
✓ Smooth movement with acceleration
✓ Variable height jumping
✓ Double jump (unlockable)
✓ Dash (unlockable)
✓ Sphere-cast melee attack
3. Enemy AI
State machine-driven AI with detection system.

States:
→ Wander: Random patrol within radius
→ Chase: Pursue detected player
→ Attack: Deal damage when in range
4. Spawn System
Factory pattern-based spawning with strategies.

Components:
→ EntityFactory: Creates entities from data
→ SpawnPointStrategy: Linear or Random
→ SpawnManager: Coordinates spawning
5. Input System
Event-driven input using Unity's new Input System.

Benefits:
✓ ScriptableObject architecture
✓ Device-agnostic
✓ Easy rebinding
✓ Decoupled from gameplay code
📁 Project Structure
Assets/
├── _Project/                      # Main project files
│   ├── Scenes/                    # Game scenes
│   │   ├── Level_Tutorial.unity
│   │   └── Level_1.unity
│   ├── Scripts/                   # All C# scripts
│   │   ├── StateMachine/          # State machine framework
│   │   │   ├── StateMachine.cs
│   │   │   ├── IState.cs
│   │   │   ├── PlayerStates/
│   │   │   └── EnemyStates/
│   │   ├── SpawnSystem/           # Spawning framework
│   │   ├── Input/                 # Input handling
│   │   │   └── InputReader.cs
│   │   ├── UI/                    # UI controllers
│   │   ├── Utils/                 # Utilities & helpers
│   │   │   └── Timer.cs
│   │   ├── PlayerController.cs
│   │   ├── CameraManager.cs
│   │   ├── Enemy.cs
│   │   ├── Health.cs
│   │   └── PlayerDetector.cs
│   ├── Prefabs/                   # Reusable GameObjects
│   ├── Materials/                 # Materials
│   ├── Models/                    # 3D models
│   ├── Animation/                 # Animation assets
│   └── ScriptableObjects/         # Data assets
│       └── EventChannels/
├── Plugins/                       # Third-party plugins
└── TextMesh Pro/                  # TMP assets
🔧 Technical Details
Unity Version
Unity 2022.3 LTS

Render Pipeline
Universal Render Pipeline (URP) 14.0.8

Physics
3D Physics with Rigidbody-based movement
NavMesh for enemy pathfinding
Sphere casting for attack detection
Animation
Animator State Machines for character animations
Animation Events for attack timing
CrossFade for smooth transitions
Code Architecture
Design Patterns Used
State Pattern - Player and enemy behaviors
Factory Pattern - Entity spawning
Strategy Pattern - Detection algorithms, spawn point selection
Observer Pattern - Event channels for decoupled communication
Object Pool Pattern - Ready for entity pooling (future optimization)
Coding Standards
Self-explanatory naming conventions
XML comments for public APIs
Constants instead of magic numbers
Modular, single-responsibility classes
📦 Dependencies
Unity Packages
Package	Version	Purpose
Input System	1.6.1	Modern input handling
Cinemachine	2.9.7	Camera control
AI Navigation	1.1.7	NavMesh pathfinding
Universal RP	14.0.8	Rendering pipeline
TextMesh Pro	3.0.6	UI text rendering
ProBuilder	5.0.7	Level geometry
External Packages
Package	Source	Purpose
Scene Ref Attribute	GitHub	Scene reference validation
DOTween	Demigiant	Tweening animations
Asset Store Assets
RPG Monster DUO PBR Polyart
RPG Tiny Hero Duo
Low Poly Ultimate Pack
Casual Game Sounds
📚 Documentation
Full System Documentation
For detailed documentation on all systems, see the Systems Documentation.

Key Systems Overview
Player Controller - Movement, jumping, abilities
State Machine - Architecture and usage
Enemy AI - Behavior and states
Spawn System - Factory pattern implementation
Input System - Event-driven input handling
Detection System - Cone detection strategy
Code Examples
Creating a New Player State
public class CustomState : BaseState {
    public CustomState(PlayerController player, Animator animator) 
        : base(player, animator) { }
    
    public override void OnEnter() {
        animator.CrossFade(CustomHash, crossFadeDuration);
    }
    
    public override void FixedUpdate() {
        player.HandleMovement();
        // Custom logic here
    }
}
Adding a State Transition
At(locomotionState, customState, 
   new FuncPredicate(() => customCondition));


🙏 Acknowledgments
Learning Resources
Unity Documentation
Cinemachine Documentation
Input System Documentation
Asset Credits
Character Models: RPG Tiny Hero Duo
Enemy Models: RPG Monster DUO PBR Polyart
Environment: Low Poly Ultimate Pack
Sound Effects: Casual Game Sounds
Tools & Libraries
KBCore.Refs - Component validation
DOTween - Animation tweening
Scene Ref Attribute - Scene reference handling