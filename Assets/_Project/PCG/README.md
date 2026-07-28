# PCG Lab

This folder contains the deterministic PCG assets and the playable validation lab.

## Run

1. Open `Assets/_Project/Scenes/PCG_Lab.unity`.
2. Enter Play Mode.
3. Move Player 2 with WASD, jump with Space, hold RMB to control the camera, and dash with Shift.
4. Use the top-left debug panel to change the seed and regenerate.
5. Toggle `Double Jump` or `Dash` to unlock the same ability on Player 2 and include gated chunks.
6. Use `Copy Seed` or `Copy Manifest` to reproduce a generated layout.

The default seed is `82431` and the default level contains 16 chunks.

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

## Regenerate assets

Use `Platformer > PCG > Create First Batch`.

This command recreates the generated prefabs, data assets, configuration and lab
scene. Do not use it after manually editing generated assets unless overwriting those
changes is intended.

## Tests

The `Platformer.PCG.Tests` EditMode assembly currently contains 21 tests covering:

- deterministic random sequences and chunk selection;
- ability and minimum-progress filtering;
- horizontal/vertical reachability and traversal bonuses;
- category repetition limits;
- overlap validation;
- manifest serialization and failure reporting;
- deterministic generation of 16 chunks from the real generated asset library;
- adaptive difficulty scoring and smoothing;
- the stable 20-value Game AI observation contract.
