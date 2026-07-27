# PCG Lab

This folder contains the deterministic PCG assets and the playable validation lab.

## Run

1. Open `Assets/_Project/Scenes/PCG_Lab.unity`.
2. Enter Play Mode.
3. Move Player 2 with WASD, jump with Space, hold RMB to control the camera, and dash with Shift.
4. Use the top-left debug panel to change the seed and regenerate.
5. Toggle `Double Jump` or `Dash` to unlock the same ability on Player 2 and include gated chunks.
6. Use `Copy Seed` or `Copy Manifest` to reproduce a generated layout.

The default seed is `82431` and the default level contains 12 chunks.

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
- `moving_01`
- `timed_01`
- `double_jump_01`
- `dash_gap_01`
- `combat_01`
- `recovery_01`

`moving_01` and `timed_01` are geometry archetypes in this milestone. Their runtime
movement/toggle behaviours will be connected after the deterministic generation and
validation layer is stable.

## Regenerate assets

Use `Platformer > PCG > Create First Batch`.

This command recreates the generated prefabs, data assets, configuration and lab
scene. Do not use it after manually editing generated assets unless overwriting those
changes is intended.

## Tests

The `Platformer.PCG.Tests` EditMode assembly currently contains 13 tests covering:

- deterministic random sequences and chunk selection;
- ability and minimum-progress filtering;
- horizontal/vertical reachability and traversal bonuses;
- category repetition limits;
- overlap validation;
- manifest serialization and failure reporting;
- deterministic generation of 12 chunks from the real generated asset library.
