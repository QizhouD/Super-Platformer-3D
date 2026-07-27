# Changelog

## [Unreleased] - 2026-07-27

### Added

- Added deterministic runtime behaviours for generated content:
  - `PCGOscillatingPlatform` for moving-platform challenges.
  - `PCGTimedPlatform` with visible, warning, hidden, and recovery phases.
- Added `PCGRunTelemetry` with bounded JSON-exportable event history and player snapshots.
  Telemetry covers generation completion, checkpoint progress, respawns, timed-platform
  transitions, player position, velocity, and generated chunk count.
- Added six spatial chunk archetypes:
  - `turn_left_01`
  - `turn_right_01`
  - `offset_left_01`
  - `climb_01`
  - `descend_01`
  - `climb_turn_left_01`
- Added spatial metadata for elevation, heading, and lateral displacement to
  `PlatformChunkData`.
- Added `Copy Telemetry` and live telemetry counters to the PCG debug panel.
- Added five EditMode tests for dynamic-platform timing, telemetry buffering, spatial
  variation requirements, and elevation-envelope filtering.

### Changed

- Increased the default generated level from 12 to 16 chunks.
- Added a spatial grammar that limits consecutive flat and straight chunks to three.
- Expanded the relative elevation envelope from `-2.5m...+6m` to `-4m...+8m`.
- Scaled horizontal platform placement by `1.25x` and platform footprints by `1.05x`
  to produce wider gaps and a larger play area.
- Updated traversal capability metadata to keep expanded gaps reachable by Player 2.
- Reused Tutorial presentation assets:
  - Normal and start platforms use `NoiseGround.mat`.
  - The disappearing timed surface uses `TimedPlatform.mat`.
- Expanded moving-platform travel distance and timing to match the larger layout.
- Extended generator and run-controller events so Game AI systems can observe
  generation, checkpoints, and respawns without polling scene objects.
- Updated generated prefabs, chunk data assets, `LevelGenerationConfig`, and
  `PCG_Lab.unity`.

### Fixed

- Prevented generated layouts from degenerating into a single straight, flat line.
- Filtered telemetry emitted by temporary chunk candidates rejected during generation.
- Kept generated routes inside a bounded vertical envelope.
- Preserved deterministic output, ability gating, reachability filtering, category
  limits, and overlap validation after the spatial expansion.

### Validation

- EditMode tests: **18 passed, 0 failed**.
- Default seed: `82431`.
- Runtime generation: **16 chunks and 16 checkpoints**.
- Measured layout:
  - X range: approximately `61.9m`
  - Z range: approximately `48.1m`
  - Y range: approximately `8m`
  - Heading variants: `4`
- Runtime material audit: no unexpected platform materials.
- Play Mode smoke test: generation completed with no new runtime errors.

### Suggested commit

```text
feat(pcg): expand spatial generation with dynamic platforms and telemetry

- add moving and timed platform runtime behaviours
- add turn, offset, climb and descent chunk archetypes
- enforce spatial variation and bounded elevation
- expand layouts to 16 chunks with wider traversal gaps
- reuse tutorial platform materials
- export PCG run telemetry for future Game AI observations
- extend EditMode coverage to 18 passing tests
```

### Commit scope note

Do not include `.claude/settings.local.json`; it is a local untracked settings file
unrelated to this PCG update.
