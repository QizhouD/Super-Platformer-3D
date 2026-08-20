# PCG Lab

This folder contains the deterministic PCG assets and the playable validation lab.

## Run

1. Open `Assets/_Project/Scenes/PCG_Lab.unity`.
2. Enter Play Mode.
3. Move Player 2 with WASD, jump with Space, hold RMB to control the camera, and dash with Shift.
4. Use the top-left PCG Lab HUD to change the seed and regenerate. The old IMGUI debug panel is hidden while the presentation layer is active.
5. Toggle `Double Jump` or `Dash` to unlock the same ability on Player 2 and include gated chunks.
6. Use `COPY SEED` or `MANIFEST` to reproduce a generated layout.

The lab presentation is installed at Play Mode start by `PCGLabExperience`. It restyles platforms by chunk category, adds checkpoint beacons, dusk lighting/fog, HUD, and SFX. Generation, checkpoints, observations, and training controls stay the same.

The default seed is `82431` and the default level contains 16 chunks.

Generation is chunk-based and finite, not an endless spawner:

```text
Play
→ LevelGenerator.Generate(seed)
→ Rhythm role + difficulty-at-progress
→ ChunkSelector (ability, reach, grammar, overlap)
→ Instantiate existing chunk prefab
→ Checkpoint at each exit
→ Optional safe fallback + project-asset decoration
```

Reachability uses Player 2's jump/move numbers with a 0.82 safety factor when
`Sync Reach From Player` is enabled on `LevelGenerationConfig`. Same seed still
reproduces the same route. Scene gizmos color chunks green/yellow/red by
difficulty and draw entry/exit links.

The lab reuses the `Player 2` prefab, `InputReader`, `CameraManager`, and Cinemachine
FreeLook setup from `Level_Tutorial`. Each generated chunk adds an exit checkpoint.
Falling below the lab threshold returns the player to the furthest checkpoint.

## Reachability contract

Each chunk data asset records its required horizontal reach, vertical reach, and
ability flags. `ChunkSelector` rejects candidates outside the current player
capabilities before weighted selection, so a deterministic seed cannot produce a
known-unplayable ability gate.

## Generated chunk library

- `basic_01`
- `rising_01`
- `turn_left_01`
- `turn_right_01`
- `offset_left_01`
- `climb_01`
- `descend_01`
- `climb_turn_left_01`
- `moving_01`
- `timed_01`
- `double_jump_01`
- `dash_gap_01`
- `combat_01`
- `recovery_01`

`moving_01` contains a deterministic lateral oscillating platform. `timed_01`
contains a telegraphed platform cycling through visible, warning, and hidden states.

The spatial grammar limits consecutive flat and straight chunks to three. Turn,
lateral-offset, climb, descent, and combined climb-turn chunks are selected when a
path would otherwise become repetitive. Relative elevation is constrained to
`-4m...+8m` to prevent runaway vertical layouts. Horizontal platform placement is
scaled by `1.25x`, while platform footprints are scaled by `1.05x`, producing wider
gaps and a larger overall play area without exceeding the configured traversal model.

PCG platforms reuse `NoiseGround.mat` from `Level_Tutorial`; the disappearing surface
inside `timed_01` uses the tutorial `TimedPlatform.mat`.

## Runtime telemetry

`PCGRunTelemetry` records generation completion, checkpoint progress, player
respawns, and timed-platform state changes. It also maintains a lightweight player
position/velocity snapshot for future Game AI observation. Use `Copy Telemetry` in
the debug panel to export the current run as JSON.

## Multimodal Game AI

`PCGGameAIObservationSensor` produces a fixed 20-value structured observation at
5 Hz and a separate 84x84 RGB frame at 2 Hz. The structured channel contains player
pose and velocity, camera direction, progress, resets, upcoming chunk difficulty,
dynamic-platform state, and the current adaptive difficulty estimate. This contract
can be consumed by ML-Agents, an ONNX policy, or an external inference service
without coupling those systems to the generator internals.

`PCGAdaptiveDifficultyDirector` estimates player skill from checkpoint time and
respawns. It immediately changes moving-platform speed and timed-platform windows,
and applies a `-0.2...+0.2` difficulty bias to the next generated layout. The current
skill and bias are visible in the debug panel; adaptive difficulty can be toggled
during Play Mode.

## Dataset recording

Use `Start Dataset Recording` in the debug panel to create a framework-independent
multimodal episode under:

`Application.persistentDataPath/PCGDatasets/<episode-id>/`

Each episode contains:

- `observations.jsonl`: 20-value observations, heuristic behavior labels, rewards,
  episode-relative timestamps, and visual-frame references;
- `frames/*.png`: synchronized 84x84 RGB observations;
- `episode.json`: seed, duration, completion, resets, checkpoint progress, return,
  final skill estimate, and final difficulty bias.

The reward contract grants `10 * progress delta` and subtracts `1` per respawn.
Behavior labels distinguish idle, traversal, airborne, falling, and post-respawn
recovery states. This format is suitable for behavior cloning, supervised behavior
detection, offline reinforcement learning, or conversion to a Python dataset.

## ML-Agents dependency

The project pins `com.unity.ml-agents` to `2.0.1`, the stable package compatible
with Unity 2022.3. Its matching Python trainer (`mlagents==0.30.0`) is installed in
the project-local `.venv-mlagents` environment using Python 3.9. Training setup
instructions live in `Training/README.md`.

`PCGNavigationAgent` uses the existing Player 2 state machine through an external
`InputReader` channel. Its policy receives 20 normalized navigation values plus the
84x84 RGB sensor. Actions contain two continuous movement axes and two binary
branches for jump and dash. Checkpoints, completion, respawns, time, and target
approach provide the reward signal. Use the debug panel to switch between human
heuristic control and trainer control.

## Regenerate assets

Use `Platformer > PCG > Create First Batch`.

This command recreates the generated prefabs, data assets, configuration and lab
scene. Do not use it after manually editing generated assets unless overwriting those
changes is intended.

## Tests

The `Platformer.PCG.Tests` EditMode assembly currently contains 36 tests covering:

- deterministic random sequences and chunk selection;
- ability and minimum-progress filtering;
- horizontal/vertical reachability and traversal bonuses;
- category repetition limits;
- overlap validation;
- manifest serialization and failure reporting;
- deterministic generation of 16 chunks from the real generated asset library;
- adaptive difficulty scoring and smoothing;
- the stable 20-value Game AI observation contract;
- behavior labeling, reward calculation, and episode metadata serialization.
- normalized ML navigation observations and next-target direction encoding;
- lab presentation palette, category inference, and debug-panel API;
- reach safety, rhythm beats, safest-chunk fallback, and deterministic rhythm weights.
