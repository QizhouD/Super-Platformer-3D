# Platforming feel and first blockout

Unity 2022.3 LTS. This note is for authored levels and for judging PCG_LAB density. It is not a rewrite of the generator.

## Principles (Odyssey / Astro Bot / A Hat in Time)

1. **Vertical density** — Stack routes in Y. The camera should see a path below and a reward above in the same glance.
2. **Teach → Test → Twist** — First jump is generous. Second asks for the same move under timing. Third adds a moving or crumbling surface.
3. **Collectibles lead the camera** — Coins sit on the intended line. A unique landmark (moon/star) sits on the risk line.
4. **Risk / reward** — Main path is always completable. Optional path is shorter or prettier but uses bounce, crumble, or dash.
5. **Short, high-density rooms** — 30–90 seconds to a checkpoint. Reset is cheap. No five-minute empty walks.
6. **Readable collision** — Silhouette bigger than the danger. Bright top face, darker sides, no camouflaged ledges.
7. **One new verb per room** — Do not mix dash-gap + timed + bouncy until each was taught alone.

## First prototype room (blockout)

Start on a 8x7 pad (existing PCG start scale). Facing +Z.

| Beat | Layout | Verb |
|---|---|---|
| 0 Teach | Two 4x4 pads, gap ~2.5m, same height | Walk + jump |
| 1 Test | Same gap, +1.2m rise | Full jump |
| 2 Twist | Path-moving pad across a 4m trench | Timing |
| 3 Reward spur | Left branch, three coins then a landmark on a crumbling pad | Risk |
| 4 Bounce | Spring into a 2.5m ledge the walk path cannot reach | Vertical |
| 5 Checkpoint | Flag at 18–22m from start, fall plane at Y = -8 | Reset |

Heights stay inside the player reach model (safety 0.82). Do not author gaps wider than a comfortable jump unless Dash is already unlocked in that room.

## Folder layout

```text
Assets/_Project/
  Scripts/
    Player/              PlayerFeelConfig
    Platforms/           Path / Bounce / Crumble / Rotate
    Collectibles/        Definition + Pickup
    Feel/                FeelFeedback
    PCG/                 existing generator (do not fork)
    Editor/              PlatformPlaceTool
  ScriptableObjects/
    PlayerFeelConfig.asset     (create via Create > Platformer)
    Collectibles/
  Prefabs/Platforms/     assemble from scripts + cubes
  Prefabs/Collectibles/
  Scenes/PCG_Lab.unity
docs/
  level-design.md
  pcg.md
```

## Editor tools

Menu `Platformer / Level Tools`:

- **Snap Selection To Grid** (`Ctrl+Shift+G`)
- **Create Ground Cube** (layer `Ground`)
- **Add Path Moving Platform** (expects a selected mesh; sets tag `MovingPlatform`)

## Inspector setup

1. Create `PlayerFeelConfig` and assign it on Player 2. Coyote 0.1s, buffer 0.12s, fall gravity 2.
2. Add `FeelFeedback` on Player 2. Leave shake target empty unless you have a camera child that Cinemachine does not overwrite.
3. Coin: empty with trigger collider + `CollectiblePickup` + a Coin definition (score 10).
4. Landmark: same, Unique checked, distinct `id`.
5. Authored checkpoints: trigger + `GameplayCheckpoint` (PCG_LAB already uses `ChunkCheckpoint`).

This project uses **Rigidbody**, not CharacterController. Do not add a second mover.

## Grok Imagine prompts

Style lock for every prompt: saturated stylized 3D platformer, readable silhouettes, soft ambient occlusion, Astro Bot / Super Mario Odyssey, no photoreal skin, no UI chrome unless asked.

1. **Ground albedo** — `Stylized 3D platformer tileable ground texture, warm sandstone with painted moss specks, high contrast grout lines for jump readability, Mario Odyssey kingdom tile, 2k, no text`
2. **Coin** — `Stylized 3D collectible coin, thick torus, glossy gold, star stamp in the center, Astro Bot toy material, orthographic three-quarter, transparent background`
3. **Landmark moon** — `Stylized 3D power moon / star collectible, crescent with a smiling face, glowing rim light, Odyssey style, turnaround on pure dark blue, transparent background`
4. **Spring pad** — `Stylized 3D bounce flower platform, red and cream petals, compressed coil visible, toy plastic, top-down and side view on one sheet, high readability`
5. **HUD icons** — `Game UI icon set, 64px, coin, moon, jump boot, dash spark, checkpoint flag, saturated flat 3D, Astro Bot HUD, consistent outline, white background`
6. **Crumble block** — `Stylized 3D sandstone platform that looks cracked, painted warning stripes on the rim, toy diorama lighting, three-quarter view, transparent background`
7. **Sky kingdom prop** — `Stylized 3D floating brick arch prop, pastel clouds bolted on like toys, Odyssey Cascade Kingdom, readable silhouette, no characters`
8. **Checkpoint flag** — `Stylized 3D checkpoint flag, fat pole, waving pennant with a star, bounce-ready base plate, saturated red and gold, orthographic`
